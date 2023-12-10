namespace OpenSteamworks.Enums;

public enum EResult : System.UInt32
{
       NoResult = 0,
       OK = 1,
       Failure = 2,
       NoConnection = 3,
       InvalidPassword = 5,
       LoggedInElsewhere = 6,
       InvalidProtocol = 7,
       InvalidParameter = 8,
       FileNotFound = 9,
       Busy = 10,
       InvalidState = 11,
       InvalidName = 12,
       InvalidEmail = 13,
       DuplicateName = 14,
       AccessDenied = 15,
       Timeout = 16,
       Banned = 17,
       AccountNotFound = 18,
       InvalidSteamID = 19,
       ServiceUnavailable = 20,
       NotLoggedOn = 21,
       Pending = 22,
       EncryptionFailure = 23,
       InsufficientPrivilege = 24,
       LimitExceeded = 25,
       RequestRevoked = 26,
       LicenseExpired = 27,
       AlreadyRedeemed = 28,
       DuplicatedRequest = 29,
       AlreadyOwned = 30,
       IPAddressNotFound = 31,
       PersistenceFailed = 32,
       LockingFailed = 33,
       SessionReplaced = 34,
       ConnectionFailed = 35,
       HandshakeFailed = 36,
       IOOperationFailed = 37,
       DisconnectedByRemoteHost = 38,
       ShoppingCartNotFound = 39,
       Blocked = 40,
       Ignored = 41,
       NoMatch = 42,
       AccountDisabled = 43,
       ServiceReadOnly = 44,
       AccountNotFeatured = 45,
       AdministratorOK = 46,
       ContentVersion = 47,
       TryAnotherCM = 48,
       PasswordRequiredToKickSession = 49,
       AlreadyLoggedInElsewhere = 50,
       RequestSuspendedpaused = 51,
       RequestHasBeenCanceled = 52,
       CorruptedOrUnrecoverableDataError = 53,
       NotEnoughFreeDiskSpace = 54,
       RemoteCallFailed = 55,
       PasswordIsNotSet = 56,
       ExternalAccountIsNotLinkedToASteamAccount = 57,
       PSNTicketIsInvalid = 58,
       ExternalAccountLinkedToAnotherSteamAccount = 59,
       RemoteFileConflict = 60,
       IllegalPassword = 61,
       SameAsPreviousValue = 62,
       AccountLogonDenied = 63,
       CannotUseOldPassword = 64,
       InvalidLoginAuthCode = 65,
       AccountLogonDeniedNoMailSent = 66,
       HardwareNotCapableOfIPT = 67,
       IPTInitError = 68,
       OperationFailedDueToParentalControlRestrictionsForCurrentUser = 69,
       FacebookQueryReturnedAnError = 70,
       ExpiredLoginAuthCode = 71,
       IPLoginRestrictionFailed = 72,
       AccountLockedDown = 73,
       AccountLogonDeniedVerifiedEmailRequired = 74,
       NoMatchingURL = 75,
       BadResponse = 76,
       PasswordReentryRequired = 77,
       ValueIsOutOfRange = 78,
       UnexpectedError = 79,
       FeatureDisabled = 80,
       InvalidCEGSubmission = 81,
       RestrictedDevice = 82,
       RegionLocked = 83,
       RateLimitExceeded = 84,
       AccountLogonDeniedNeedTwofactorCode = 85,
       ItemOrEntryHasBeenDeleted = 86,
       TooManyLogonAttempts = 87,
       TwofactorCodeMismatch = 88,
       TwofactorActivationCodeMismatch = 89,
       AccountAssociatedWithMultiplePlayers = 90,
       NotModified = 91,
       NoMobileDeviceAvailable = 92,
       TimeIsOutOfSync = 93,
       SMSCodeFailed = 94,
       TooManyAccountsAccessThisResource = 95,
       TooManyChangesToThisAccount = 96,
       TooManyChangesToThisPhoneNumber = 97,
       YouMustRefundThisTransactionToWallet = 98,
       SendingOfAnEmailFailed = 99,
       PurchaseNotYetSettled = 100,
       NeedsCaptcha = 101,
       GameserverLoginTokenDenied = 102,
       GameserverLoginTokenOwnerDenied = 103,
       InvalidItemType = 104,
       IPAddressBanned = 105,
       GameserverLoginTokenExpired = 106,
       InsufficientFunds = 107,
       TooManyPending = 108,
       NoSiteLicensesFound = 109,
       NetworkSendExceeded = 110,
       AccountsNotFriends = 111,
       LimitedUserAccount = 112,
       CantRemoveItem = 113,
       AccountHasBeenDeleted = 114,
       AccountHasAnExistingUserCancelledLicense = 115,
       DeniedDueToCommunityCooldown = 116,
       NoLauncherSpecified = 117,
       MustAgreeToSSA = 118,
       ClientNoLongerSupported = 119,
       TheCurrentSteamRealmDoesNotMatchTheRequestedResource = 120,
       SignatureCheckFailed = 121,
       FailedToParseInput = 122,
       NoVerifiedPhoneNumber = 123,
       InsufficientBatteryCharge = 124,
       ChargerRequired = 125,
       CachedCredentialIsInvalid = 126,
       PhoneNumberIsVoiceOverIP = 127,
};