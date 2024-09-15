using System.Diagnostics.CodeAnalysis;
using OpenSteamworks.Client.Config;
using OpenSteamworks;
using OpenSteamworks.Attributes;
using OpenSteamworks.Callbacks.Structs;
using OpenSteamworks.Data.Enums;
using OpenSteamworks.Data.Structs;
using OpenSteamworks.Client.Utils;
using OpenSteamworks.Client.CommonEventArgs;
using OpenSteamworks.ClientInterfaces;
using OpenSteamworks.Messaging;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Net;
using OpenSteamworks.Client.Login;
using OpenSteamworks.Protobuf;
using System.Security.Cryptography;
using System.Text;
using static OpenSteamworks.Callbacks.CallbackManager;
using OpenSteamClient.DI;
using OpenSteamworks.Utils;
using OpenSteamworks.Callbacks;
using OpenSteamClient.DI.Lifetime;
using OpenSteamworks.Client.DI;
using OpenSteamClient.Logging;

namespace OpenSteamworks.Client.Managers;

public class LoggedOnEventArgs : EventArgs
{
    public LoggedOnEventArgs(LoginUser user) { User = user; }
    public LoginUser User { get; } 
}

public class LoggedOffEventArgs : EventArgs
{
    public LoggedOffEventArgs(LoginUser user, EResult error) { User = user; Error = error; }
    public LoginUser User { get; } 
    public EResult Error { get; }
}

public class LogOnFailedEventArgs : EventArgs
{
    public LogOnFailedEventArgs(LoginUser user, EResult error) { User = user; Error = error; }
    public LoginUser User { get; } 
    public EResult Error { get; }
}

public class QRGeneratedEventArgs : EventArgs
{
    public QRGeneratedEventArgs(string url) { URL = url; }
    public string URL { get; }
}

public class SecondFactorNeededEventArgs : EventArgs
{
    public SecondFactorNeededEventArgs(IEnumerable<CAuthentication_AllowedConfirmation> allowedConfirmations) { AllowedConfirmations = allowedConfirmations; }
    public IEnumerable<CAuthentication_AllowedConfirmation> AllowedConfirmations { get; }
}

public class LoginManager : IClientLifetime
{
    public delegate void LoggedOnEventHandler(object sender, LoggedOnEventArgs e);
    public delegate void LoggedOffEventHandler(object sender, LoggedOffEventArgs e);
    public delegate void LogOnStartedEventHandler(object sender, EventArgs e);
    public delegate void LogOnFailedEventHandler(object sender, LogOnFailedEventArgs e);
    public delegate void QRGeneratedEventHandler(object sender, QRGeneratedEventArgs e);
    public delegate void SecondFactorNeededEventHandler(object sender, SecondFactorNeededEventArgs e);
    public event LoggedOnEventHandler? LoggedOn;
    public event EventHandler? LoggingOff;
    public event LoggedOffEventHandler? LoggedOff;
    public event LogOnStartedEventHandler? LogonStarted;
    public event LogOnFailedEventHandler? LogOnFailed;
    public event QRGeneratedEventHandler? QRGenerated;
    public event SecondFactorNeededEventHandler? SecondFactorNeeded;

    private readonly ISteamClient steamClient;
    private readonly LoginUsers loginUsers;
    private readonly IContainer container;
    private readonly ILogger logger;
    private readonly ConfigManager configManager;
    private readonly ClientApps clientApps;

    private LoginPoll? QRPoller;
    private LoginPoll? CredentialsPoller;
    /// <summary>
    /// Used only when a logon is in progress. To get the logged in user's SteamID, use IClientUser::GetSteamID or LoginManager::CurrentUser::SteamID
    /// </summary>
    public CSteamID InProgressLogonSteamID { get; private set; } = 0;
    public LoginUser? CurrentUser { get; private set; }

    public bool IsAnonUser => this.CurrentUser?.SteamID.AccountType == EAccountType.AnonUser;

    public LoginManager(ISteamClient steamClient, ClientApps clientApps, ILoggerFactory loggerFactory, LoginUsers loginUsers, ConfigManager configManager, IContainer container, InstallManager installManager)
    {
        this.clientApps = clientApps;
        this.configManager = configManager;
        this.logger = loggerFactory.CreateLogger("LoginManager");
        this.container = container;
        this.steamClient = steamClient;
        this.loginUsers = loginUsers;
        steamClient.CallbackManager.Register<SteamServerConnectFailure_t>(OnSteamServerConnectFailure);
        steamClient.CallbackManager.Register<SteamServersDisconnected_t>(OnSteamServersDisconnected);
        steamClient.CallbackManager.Register<PostLogonState_t>(OnPostLogonState);
    }

    public bool RemoveAccount(LoginUser loginUser) {
        logger.Info("Removing account " + loginUser.AccountName);
        var success = loginUsers.RemoveUser(loginUser);
        steamClient.IClientUser.DestroyCachedCredentials(loginUser.AccountName, (int)EAuthTokenRevokeAction.EauthTokenRevokePermanent);
        configManager.Save(loginUsers);
        return success;
    }

    public bool AddAccount(LoginUser loginUser) {
        logger.Info("Adding account " + loginUser.AccountName);
        loginUser.Remembered = steamClient.IClientUser.BHasCachedCredentials(loginUser.AccountName);
        var success = loginUsers.AddUser(loginUser);
        configManager.Save(loginUsers);
        return success;
    }

    /// <summary>
    /// Sets a progress object for logging in. To set one for logging out and shutting down, set it within the container
    /// </summary>
    /// <param name="progress"></param>
    public void SetProgress(IProgress<OperationProgress>? progress) {
        this.loginProgress = progress;
    }

    private IProgress<OperationProgress>? loginProgress;

    /// <summary>
    /// Asynchronous background tasks may throw. If no exception handler is explicitly specified, C# will eat the errors and silently fail.
    /// </summary>
    public void SetExceptionHandler(Action<AggregateException> exceptionHandler) {
        this.exceptionHandler = exceptionHandler;
    }
    private Action<AggregateException>? exceptionHandler;

    /// <summary>
    /// Starts generating QR codes for login. Will call QRGenerated everytime a new QR code comes in. Call this method AFTER registering a handler for QRGenerated.
    /// </summary>
    public void StartQRAuthLoop() {
        if (QRPoller != null && QRPoller.IsPolling) {
            logger.Error("QR Auth loop already running");
            throw new InvalidOperationException("QR Auth loop already running");
        }

        Task.Run(async () =>
        {
            using (SharedConnection conn = SharedConnection.AllocateConnection())
            {
                ProtoMsg<CAuthentication_BeginAuthSessionViaQR_Request> beginMsg = new("Authentication.BeginAuthSessionViaQR#1", true);
                beginMsg.Body.DeviceDetails = DeviceDetails;

                var beginResp = await conn.SendAndWaitForServiceResponse<CAuthentication_BeginAuthSessionViaQR_Response, CAuthentication_BeginAuthSessionViaQR_Request>(beginMsg);

                QRGenerated?.Invoke(this, new QRGeneratedEventArgs(beginResp.Body.ChallengeUrl));
                
                QRPoller = new LoginPoll(beginResp.Body.ClientId, beginResp.Body.RequestId, beginResp.Body.Interval);
                QRPoller.ChallengeUrlGenerated += (object sender, ChallengeUrlGeneratedEventArgs e) => this.QRGenerated?.Invoke(this, new QRGeneratedEventArgs(e.URL));
                QRPoller.RefreshTokenGenerated += OnRefreshTokenGenerated;
                QRPoller.Error += (object sender, EResultEventArgs e) => exceptionHandler?.Invoke(new AggregateException("Error occurred in QR Poller: " + e.EResult));
                QRPoller.StartPolling();
            }
        });
    }

    /// <summary>
    /// Starts an auth session via credentials. Add extra steam guard details, etc with other functions.
    /// Will fire events for needed extra details, etc
    /// </summary>
    /// <returns>True if initial credentials were ok, false if session failed to start</returns>
    public async Task<EResult> StartAuthSessionWithCredentials(string username, string password, bool rememberPassword) {
        if (CredentialsPoller != null && CredentialsPoller.IsPolling) {
            logger.Error("Credential logon already in progress");
            throw new InvalidOperationException("Credential logon already in progress");
        }

        if (username == "anonymous") {
            // There's probably a better way to create the SteamID, but let's just do this for now
            BeginLogonToUser(new LoginUser(new CSteamID(1, EUniverse.Public, EAccountType.AnonUser), "anonymous", "g", false));
            // steamClient.IClientUser.SetLoginInformation("anonymous", "g", false);
            // steamClient.IClientUser.LogOn(new CSteamID(1, EUniverse.Public, EAccountType.AnonUser));
            return EResult.OK;
        }

        using (SharedConnection conn = SharedConnection.AllocateConnection())
        {
            // Get password RSA hash
            ProtoMsg<CAuthentication_GetPasswordRSAPublicKey_Request> rsaRequest = new("Authentication.GetPasswordRSAPublicKey#1", true);
            rsaRequest.Body.AccountName = username;

            var rsaResp = await conn.SendAndWaitForServiceResponse<CAuthentication_GetPasswordRSAPublicKey_Response, CAuthentication_GetPasswordRSAPublicKey_Request>(rsaRequest);

            // Encrypt the password (this simple in C#)
            string encryptedPassword;
            using (var rsa = RSA.Create())
            {
                
                var rsaParameters = new RSAParameters
                {
                    Modulus = Convert.FromHexString(rsaResp.Body.PublickeyMod),
                    Exponent = Convert.FromHexString(rsaResp.Body.PublickeyExp),
                };
                rsa.ImportParameters(rsaParameters);
                encryptedPassword = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(password), RSAEncryptionPadding.Pkcs1));
            }

            ProtoMsg<CAuthentication_BeginAuthSessionViaCredentials_Request> beginMsg = new("Authentication.BeginAuthSessionViaCredentials#1", true);
            beginMsg.Body.DeviceDetails = DeviceDetails;
            beginMsg.Body.AccountName = username;
            beginMsg.Body.EncryptedPassword = encryptedPassword;
            beginMsg.Body.EncryptionTimestamp = rsaResp.Body.Timestamp;
            // This one is unused, but let's set it just in case
            beginMsg.Body.RememberLogin = rememberPassword;
            // This determines password remembered yes/no
            beginMsg.Body.Persistence = rememberPassword ? ESessionPersistence.Persistent : ESessionPersistence.Ephemeral;
            beginMsg.Body.PlatformType = EAuthTokenPlatformType.SteamClient;
            beginMsg.Body.WebsiteId = "Client";
            beginMsg.Body.DeviceFriendlyName = DeviceDetails.DeviceFriendlyName;

            var beginResp = await conn.SendAndWaitForServiceResponse<CAuthentication_BeginAuthSessionViaCredentials_Response, CAuthentication_BeginAuthSessionViaCredentials_Request>(beginMsg);
            if (beginResp.Header.Eresult != (int)EResult.OK) {
                return (EResult)beginResp.Header.Eresult;
            }

            // This should always have a steamid here, otherwise we can't get it from anywhere
            InProgressLogonSteamID = beginResp.Body.Steamid;

            // Interval is 0.1 and/or allowedconfirmations is set to [k_EAuthSessionGuardType_None] when user has no 2fa
            if (beginResp.Body.AllowedConfirmations.All(elem => elem.ConfirmationType == EAuthSessionGuardType.None)) {
                // Only one session type, and that's the None type
            } else {
                SecondFactorNeeded?.Invoke(this, new SecondFactorNeededEventArgs(beginResp.Body.AllowedConfirmations));
            }

            CredentialsPoller = new LoginPoll(beginResp.Body.ClientId, beginResp.Body.RequestId, beginResp.Body.Interval);
            CredentialsPoller.RefreshTokenGenerated += OnRefreshTokenGenerated;
            CredentialsPoller.Error += (object sender, EResultEventArgs e) => exceptionHandler?.Invoke(new AggregateException("Error occurred in Credentials Poller: " + e.EResult));
            CredentialsPoller.StartPolling();
            return EResult.OK;
        }
    }

    public async Task<EResult> UpdateAuthSessionWithTwoFactor(string code, EAuthSessionGuardType codeType) {
        if (CredentialsPoller == null || !CredentialsPoller.IsPolling) {
            throw new InvalidOperationException("Credential logon isn't running, cannot add steam guard code");
        }

        using (SharedConnection conn = SharedConnection.AllocateConnection())
        {
            ProtoMsg<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request> updateMsg = new("Authentication.UpdateAuthSessionWithSteamGuardCode#1", true);
            updateMsg.Body.ClientId = CredentialsPoller.ClientID;
            updateMsg.Body.CodeType = codeType;
            updateMsg.Body.Code = code;
            updateMsg.Body.Steamid = InProgressLogonSteamID;

            var updateResp = await conn.SendAndWaitForServiceResponse<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response, CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request>(updateMsg);
            return (EResult)updateResp.Header.Eresult;
        }
    }

    public void StopQRAuthLoop() {
        QRPoller?.StopPolling();
        QRPoller = null;
    }

    private void OnRefreshTokenGenerated(object sender, TokenGeneratedEventArgs e) {
        BeginLogonToUser(new LoginUser(e.AccountName, e.Token));
    }

    private CAuthentication_DeviceDetails? deviceDetails;
    public CAuthentication_DeviceDetails DeviceDetails {
        get {
            if (deviceDetails == null) {
                DetermineDeviceDetails();
            }
            return deviceDetails;
        }
    }

    [MemberNotNull(nameof(deviceDetails))]
    private void DetermineDeviceDetails() {
        //TODO: allow users to opt out of this data collection. Not that it matters anyway, as steamclient collects this data itself anyway (does it also rewrite our message?).
        deviceDetails = new();

        // This means Steam Deck/Steam Link
        // Desktop platforms is 0
        deviceDetails.GamingDeviceType = 0;

        if (OperatingSystem.IsWindows()) {
            //TODO: more accurate logic here
            if (OperatingSystem.IsWindowsVersionAtLeast(10)) {
                deviceDetails.OsType = (int)EOSType.Win10;
            } else if (OperatingSystem.IsWindowsVersionAtLeast(8, 1)) {
                deviceDetails.OsType = (int)EOSType.Win81;
            } else if (OperatingSystem.IsWindowsVersionAtLeast(7)) {
                deviceDetails.OsType = (int)EOSType.Win7;
            } else {
                deviceDetails.OsType = (int)EOSType.Windows;
            }
        } else if (OperatingSystem.IsMacOS()) {
            //TODO: logic for determining OSX version
            deviceDetails.OsType = (int)EOSType.Macos;
        } else if (OperatingSystem.IsLinux()) {
            //TODO: logic for determining kernel version
            deviceDetails.OsType = (int)EOSType.Linux;
        }
        
        // If this isn't specified, the auth tokens we receive will not let us login
        deviceDetails.PlatformType = EAuthTokenPlatformType.SteamClient;

        try
        {
            //TODO: allow user to specify this somewhere, maybe in a first-run wizard, in settings, config file etc
            deviceDetails.DeviceFriendlyName = Dns.GetHostName();
        }
        catch (System.Exception)
        {
            logger.Warning("Failed to determine hostname.");
        }
    }

    /// <summary>
    /// Gets all saved users. Does not need remember password.
    /// </summary>
    public IEnumerable<LoginUser> GetSavedUsers() {
        return loginUsers.Users;
    }

    public bool TryAutologin() {
        if (this.IsLoggedOn()) {
            return true;
        }
        
        var autologinUser = this.loginUsers.GetAutologin();
        if (autologinUser == null) {
            logger.Info("No autologin user, didn't start autologon.");
            return false;
        }

        autologinUser.LoginMethod = LoginUser.ELoginMethod.Cached;
        BeginLogonToUser(autologinUser);
        return true;
    }

    /// <summary>
    /// Gets whether there is a user logged in or in offline mode.
    /// </summary>
    [MemberNotNullWhen(true, nameof(CurrentUser))]
    public bool IsLoggedOn() {
        return this.steamClient.IClientUser.BLoggedOn();
    }

    /// <summary>
    /// Checks that there is a connection to Steam's CMs.
    /// </summary>
    public bool IsOnline() {
        var state = steamClient.IClientUser.GetLogonState();
        return state == ELogonState.LoggedOn || state == ELogonState.Connected;
    }

    /// <summary>
    /// Inverse of IsOnline.
    /// </summary>
    public bool IsOffline() {
        return !IsOnline();
    }

    private bool isLoggingOn = false;
    private EResult? loginFinishResult;
    public void OnSteamServerConnectFailure(ICallbackHandler handler, SteamServerConnectFailure_t failure) {
        if (isLoggingOn) {
            loginFinishResult = failure.m_EResult;
        }
    }

    public void OnSteamServersDisconnected(ICallbackHandler handler, SteamServersDisconnected_t disconnect) {
        if (CurrentUser != null) {
            if (disconnect.m_EResult == EResult.OK) {
                OnLoggedOff(new LoggedOffEventArgs(CurrentUser, disconnect.m_EResult));
            }
        }
    }


    public void OnPostLogonState(ICallbackHandler handler, PostLogonState_t stateUpdate) {
        if (isLoggingOn) {
            if (stateUpdate.hasAppInfo == 1 && stateUpdate.connectedToCMs == 1) {
				loginProgress?.Report(new(string.Empty, string.Empty, 100)); 
				loginFinishResult = EResult.OK;
            }
        }
    }
    
    public void SetMachineName(string machineName) {
        steamClient.IClientUser.SetUserMachineName(machineName);
    }

    public bool HasCachedCredentials(LoginUser user) {
        return steamClient.IClientUser.BHasCachedCredentials(user.AccountName);
    }

    public void BeginLogonToUser(LoginUser user) {
        logger.Info("Trying to logon to " + user.AccountName);
        if (this.isLoggingOn) {
            logger.Error("Logon already in progress");
            throw new InvalidOperationException("Logon already in progress");
        }

        if (user.LoginMethod == null)
        {
            logger.Error("LoginMethod must be set");
            throw new ArgumentException("LoginMethod must be set");
        }

        Task.Run(async () =>
        {
            switch (user.LoginMethod)
            {
                case LoginUser.ELoginMethod.UsernamePassword:
                    UtilityFunctions.AssertNotNull(user.AccountName);
                    UtilityFunctions.AssertNotNull(user.Password);
                    steamClient.IClientUser.SetLoginInformation(user.AccountName, user.Password, user.Remembered);
                    if (user.SteamGuardCode != null) {
                        steamClient.IClientUser.SetTwoFactorCode(user.SteamGuardCode);
                    }

                    break;
                case LoginUser.ELoginMethod.Cached:
                    UtilityFunctions.AssertNotNull(user.AccountName);

                    if (!steamClient.IClientUser.BHasCachedCredentials(user.AccountName))
                    {
                        OnLogonFailed(new LogOnFailedEventArgs(user, EResult.CachedCredentialIsInvalid));
                        return;
                    }

                    steamClient.IClientUser.SetAccountNameForCachedCredentialLogin(user.AccountName, false);
                    break;
                case LoginUser.ELoginMethod.JWT:
                    UtilityFunctions.AssertNotNull(user.AccountName);
                    UtilityFunctions.AssertNotNull(user.LoginToken);
                    steamClient.IClientUser.SetLoginToken(user.LoginToken, user.AccountName);

                    // Parse the JWT (format roughly XXXXXX.XXXXXXXXXXXXXXXXXXXXXXXXXXXX.XXXXXXXXX where X is just base64 encoded json data)
                    string middleBase64 = user.LoginToken.Split('.')[1];

                    // The base64 is not always padded properly... Add padding if necessary (FromBase64String is standards-enforcing, meaning it doesn't take in anything but perfect strings)
                    switch (middleBase64.Length % 4) // Pad with trailing '='s
                    {
                        case 0: break; // No pad chars in this case
                        case 2: middleBase64 += "=="; break; // Two pad chars
                        case 3: middleBase64 += "="; break; // One pad char
                        default: throw new System.ArgumentOutOfRangeException("middleBase64", "Illegal base64url string!");
                    }

                    JsonNode? obj = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(middleBase64)));
                    UtilityFunctions.AssertNotNull(obj);
                    string? steamidStr = (string?)obj["sub"];
                    UtilityFunctions.AssertNotNull(steamidStr);

                    user.SteamID = new CSteamID(ulong.Parse(steamidStr));

                    break;
            }

            this.loginFinishResult = null;
            this.isLoggingOn = true;
            LogonStarted?.Invoke(this, EventArgs.Empty);

            if (user.SteamID == 0) {
                logger.Error("SteamID is 0!");
                OnLogonFailed(new LogOnFailedEventArgs(user, EResult.InvalidSteamID));
                return;
            }

            var appInfoUpdateComplete = steamClient.CallbackManager.WaitAsync<AppInfoUpdateComplete_t>();

            loginProgress?.Report(new("Logging on " + user.AccountName, string.Empty, -1));
            EResult beginLogonResult = steamClient.IClientUser.LogOn(user.SteamID);
            logger.Info("BeginLogon returned " + beginLogonResult);
            if (beginLogonResult != EResult.OK) {
                this.isLoggingOn = false;
                OnLogonFailed(new LogOnFailedEventArgs(user, beginLogonResult));
                return;
            }
            
            logger.Info("Waiting for logon to finish");
            EResult result = await WaitForLogonToFinish();
            logger.Info("Logon finished with " + result);

            // The steam client will sometimes automatically update appinfo and wait for it if necessary
            if (result == EResult.OK)
            {   
                loginUsers.SetUserAsMostRecent(user);
                if (user.AllowAutoLogin == true)
                {
                    loginUsers.SetUserAsAutologin(user);
                }

                await configManager.SaveAsync(loginUsers);
                
                logger.Info("Waiting for appinfo update");
                loginProgress?.Report(new("Logging on " + user.AccountName, "Waiting for appinfo update", -1));
                await clientApps.UpdateAppInfoForMissingApps(clientApps.OwnedAppIDs);

                logger.Info("Finishing up");
                await OnLoggedOn(new LoggedOnEventArgs(user));
            } else {
                OnLogonFailed(new LogOnFailedEventArgs(user, result));
            }
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted) {
                exceptionHandler?.Invoke(t.Exception!);
            }
        });
    }

    public async Task LogoutAsync(IProgress<OperationProgress>? operation = null, bool forget = false) {
        UtilityFunctions.AssertNotNull(this.CurrentUser);
        await Task.Run(() => this.LoggingOff?.Invoke(this, EventArgs.Empty));

        if (operation == null) {
            operation = new Progress<OperationProgress>();
        }

        await container.Get<LifetimeManager>().RunLogoff(operation);

        var oldUser = CurrentUser;

        await Task.Run(() =>
        {
            this.steamClient.IClientUser.LogOff();
            while (this.steamClient.IClientUser.BLoggedOn())
            {
                System.Threading.Thread.Sleep(50);
            }
        });

        if (forget) {
            await Task.Run(() => RemoveAccount(oldUser));
        }
    }

    public string GetUserConfigDirectory() {
        if (!IsLoggedOn()) {
            throw new InvalidOperationException("GetUserConfigDirectory called but we're not logged in");
        }

        StringBuilder builder = new(4096);
        if (this.steamClient.IClientUser.GetUserConfigFolder(builder, builder.Capacity)) {
            return builder.ToString();
        }

        throw new Exception("Failed to get user config path.");
    }

    private void OnLogonFailed(LogOnFailedEventArgs e) {
        this.loginFinishResult = null;
        this.isLoggingOn = false;
        LogOnFailed?.Invoke(this, e);
    }

    public async Task OnLoggedOn(LoggedOnEventArgs e) {
        AddAccount(e.User);
        this.loginUsers.SetUserAsMostRecent(e.User);

        await RunLogon(e);
    }

    private async Task RunLogon(LoggedOnEventArgs e) {
        CurrentUser = e.User;
        InProgressLogonSteamID = 0;
        this.loginFinishResult = null;
        await container.Get<LifetimeManager>().RunLogon(loginProgress ?? new Progress<OperationProgress>());
        this.isLoggingOn = false;
        await Task.Run(() => LoggedOn?.Invoke(this, e));
    }

    private void OnLoggedOff(LoggedOffEventArgs e) {
        CurrentUser = null;
        LoggedOff?.Invoke(this, e);
    }

    private async Task<EResult> WaitForLogonToFinish() {
        return await Task.Run<EResult>(() =>
        {
            do
            {
                Thread.Sleep(30);
            } while (!this.loginFinishResult.HasValue);
            this.isLoggingOn = false;
            return this.loginFinishResult.Value;
        });
    }

    public async Task RunStartup(IProgress<OperationProgress> operation)
    {
        await Task.CompletedTask;
    }

    public async Task RunShutdown(IProgress<OperationProgress> operation)
    {
        if (this.IsLoggedOn() && this.steamClient.ConnectedWith == ConnectionType.NewClient) {
            operation.Report(new("Logging out"));

            logger.Info("Shutting down and logged in, logging out");
            try
            {
                await LogoutAsync(operation);
            }
            catch (System.Exception e)
            {
                logger.Error("Failed to log out.");
                logger.Error(e);
            }
        }
    }
}