#include <netbridge.h>

#if defined(_WIN32)
typedef int(CORECLR_DELEGATE_CALLTYPE *pMain_fn)(int argc, const wchar_t *argv[]);
#else
typedef int(CORECLR_DELEGATE_CALLTYPE *pMain_fn)(int argc, const char *argv[]);
#endif

CNetBridge *netbridge = nullptr;

int CNetBridge::Run()
{
    // Get the current executable's directory
    // This sample assumes the managed assembly to load and its runtime configuration file are next to the host
    char_t host_path[MAX_PATH];

#if _WIN32
    auto size = ::GetFullPathNameW(argv[0], sizeof(host_path) / sizeof(char_t), host_path, nullptr);
    if (size == 0) {
        std::cout << "Failure: size == 0" << std::endl;
        abort();
    }
#else
    std::cout << "argv0: " << argv[0] << std::endl;
    auto resolved = realpath(argv[0], host_path);
    if (resolved == nullptr) {
        std::cout << "Failure: resolved == nullptr" << std::endl;
        abort();
    }

#endif

    string_t root_path = host_path;
    auto pos = root_path.find_last_of(DIR_SEPARATOR);
    if (pos == string_t::npos) {
        std::cout << "Failure: pos == string_t::npos" << std::endl;
        abort();
    }

    root_path = root_path.substr(0, pos + 1);

    return this->run_component(root_path);
}

int CNetBridge::run_component(const string_t& root_path)
{
    //
    // STEP 1: Load HostFxr and get exported hosting functions
    //
    if (!load_hostfxr(nullptr))
    {
        std::cout << "Failure: load_hostfxr()" << std::endl;
        abort();
    }

    std::cout << "Loaded hostfxr" << std::endl;

    //
    // STEP 2: Initialize and start the .NET Core runtime
    //
    //const string_t config_path = root_path + STR("OpenSteamClient.runtimeconfig.json");
    this->managed_path = root_path + STR("OpenSteamClient.dll");
    this->load_assembly_and_get_function_pointer = get_dotnet_load_assembly(root_path.c_str());
    if (this->load_assembly_and_get_function_pointer == nullptr) {
        std::cout << "Failure: get_dotnet_load_assembly()" << std::endl;
        abort();
    }

    std::cout << "Started .NET Core runtime" << std::endl;

    //
    // STEP 3: Load managed assembly and get function pointer to a managed method
    //
    const char_t *dotnet_type = STR("OpenSteamClient.Entry, OpenSteamClient");
    pMain_fn managedMain = (pMain_fn)this->GetFunction(
        dotnet_type, 
        STR("Main"),
        STR("OpenSteamClient.Entry+MainDelegate, OpenSteamClient")
    );
    
    if (managedMain == nullptr)
    {
        std::cout << "Failure: managedMain == nullptr" << std::endl;
        abort();
    }

    // Load all bootstrapper functions

    this->pSteamBootstrapper_GetInstallDir = (pSteamBootstrapper_GetInstallDir_fn)this->GetFunction(
        dotnet_type, 
        STR("SteamBootstrapper_GetInstallDir"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_GetInstallDirDelegate, OpenSteamClient")
    );

    if (this->pSteamBootstrapper_GetInstallDir == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_GetInstallDir" << std::endl;
    }

    this->pSteamBootstrapper_GetLoggingDir = (pSteamBootstrapper_GetLoggingDir_fn)this->GetFunction(
        dotnet_type, 
        STR("SteamBootstrapper_GetLoggingDir"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_GetLoggingDirDelegate, OpenSteamClient")
    );

    if (this->pSteamBootstrapper_GetLoggingDir == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_GetLoggingDir" << std::endl;
    }

    this->pStartCheckingForUpdates = (pStartCheckingForUpdates_fn)this->GetFunction(
        dotnet_type, 
        STR("StartCheckingForUpdates"),
        STR("OpenSteamClient.Entry+StartCheckingForUpdatesDelegate, OpenSteamClient")
    );

    if (this->pStartCheckingForUpdates == nullptr) {
        std::cout << "Warning: Failed to get managed StartCheckingForUpdates" << std::endl;
    }

    this->pSteamBootstrapper_GetEUniverse = (pSteamBootstrapper_GetEUniverse_fn)this->GetFunction(
        dotnet_type,  
        STR("SteamBootstrapper_GetEUniverse"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_GetEUniverseDelegate, OpenSteamClient")
    );
    
    if (this->pSteamBootstrapper_GetEUniverse == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_GetEUniverse" << std::endl;
    }

    this->pGetBootstrapperVersion = (pGetBootstrapperVersion_fn)this->GetFunction(
        dotnet_type,  
        STR("GetBootstrapperVersion"),
        STR("OpenSteamClient.Entry+GetBootstrapperVersionDelegate, OpenSteamClient")
    );
    
    if (this->pGetBootstrapperVersion == nullptr) {
        std::cout << "Warning: Failed to get managed GetBootstrapperVersion" << std::endl;
    }

    this->pGetCurrentClientBeta = (pGetCurrentClientBeta_fn)this->GetFunction(
        dotnet_type,  
        STR("GetCurrentClientBeta"),
        STR("OpenSteamClient.Entry+GetCurrentClientBetaDelegate, OpenSteamClient")
    );
    
    if (this->pGetCurrentClientBeta == nullptr) {
        std::cout << "Warning: Failed to get managed GetCurrentClientBeta" << std::endl;
    }

    this->pClientUpdateRunFrame = (pClientUpdateRunFrame_fn)this->GetFunction(
        dotnet_type,  
        STR("ClientUpdateRunFrame"),
        STR("OpenSteamClient.Entry+ClientUpdateRunFrameDelegate, OpenSteamClient")
    );
    
    if (this->pClientUpdateRunFrame == nullptr) {
        std::cout << "Warning: Failed to get managed ClientUpdateRunFrame" << std::endl;
    }

    this->pIsClientUpdateAvailable = (pIsClientUpdateAvailable_fn)this->GetFunction(
        dotnet_type,  
        STR("IsClientUpdateAvailable"),
        STR("OpenSteamClient.Entry+IsClientUpdateAvailableDelegate, OpenSteamClient")
    );
    
    if (this->pIsClientUpdateAvailable == nullptr) {
        std::cout << "Warning: Failed to get managed IsClientUpdateAvailable" << std::endl;
    }

    this->pIsClientUpdateOutOfDiskSpace = (pIsClientUpdateOutOfDiskSpace_fn)this->GetFunction(
        dotnet_type,  
        STR("IsClientUpdateOutOfDiskSpace"),
        STR("OpenSteamClient.Entry+IsClientUpdateOutOfDiskSpaceDelegate, OpenSteamClient")
    );
    
    if (this->pIsClientUpdateOutOfDiskSpace == nullptr) {
        std::cout << "Warning: Failed to get managed IsClientUpdateOutOfDiskSpace" << std::endl;
    }

    this->pCanSetClientBeta = (pCanSetClientBeta_fn)this->GetFunction(
        dotnet_type,  
        STR("CanSetClientBeta"),
        STR("OpenSteamClient.Entry+CanSetClientBetaDelegate, OpenSteamClient")
    );
    
    if (this->pCanSetClientBeta == nullptr) {
        std::cout << "Warning: Failed to get managed CanSetClientBeta" << std::endl;
    }

    this->pSetClientBeta = (pSetClientBeta_fn)this->GetFunction(
        dotnet_type,  
        STR("SetClientBeta"),
        STR("OpenSteamClient.Entry+SetClientBetaDelegate, OpenSteamClient")
    );
    
    if (this->pSetClientBeta == nullptr) {
        std::cout << "Warning: Failed to get managed SetClientBeta" << std::endl;
    }

    this->pSteamBootstrapper_GetBaseUserDir = (pSteamBootstrapper_GetBaseUserDir_fn)this->GetFunction(
        dotnet_type,  
        STR("SteamBootstrapper_GetBaseUserDir"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_GetBaseUserDirDelegate, OpenSteamClient")
    );
    
    if (this->pSteamBootstrapper_GetBaseUserDir == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_GetBaseUserDir" << std::endl;
    }

    this->pSteamBootstrapper_GetForwardedCommandLine = (pSteamBootstrapper_GetForwardedCommandLine_fn)this->GetFunction(
        dotnet_type,  
        STR("SteamBootstrapper_GetForwardedCommandLine"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_GetForwardedCommandLineDelegate, OpenSteamClient")
    );
    
    if (this->pSteamBootstrapper_GetForwardedCommandLine == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_GetForwardedCommandLine" << std::endl;
    }

    this->pSteamBootstrapper_SetCommandLineToRunOnExit = (pSteamBootstrapper_SetCommandLineToRunOnExit_fn)this->GetFunction(
        dotnet_type,  
        STR("SteamBootstrapper_SetCommandLineToRunOnExit"),
        STR("OpenSteamClient.Entry+SteamBootstrapper_SetCommandLineToRunOnExitDelegate, OpenSteamClient")
    );
    
    if (this->pSteamBootstrapper_SetCommandLineToRunOnExit == nullptr) {
        std::cout << "Warning: Failed to get managed SteamBootstrapper_SetCommandLineToRunOnExit" << std::endl;
    }

    this->pGetClientLauncherType = (pGetClientLauncherType_fn)this->GetFunction(
        dotnet_type,  
        STR("GetClientLauncherType"),
        STR("OpenSteamClient.Entry+GetClientLauncherTypeDelegate, OpenSteamClient")
    );
    
    if (this->pGetClientLauncherType == nullptr) {
        std::cout << "Warning: Failed to get managed GetClientLauncherType" << std::endl;
    }

    this->pForceUpdateNextRestart = (pForceUpdateNextRestart_fn)this->GetFunction(
        dotnet_type,  
        STR("ForceUpdateNextRestart"),
        STR("OpenSteamClient.Entry+ForceUpdateNextRestartDelegate, OpenSteamClient")
    );
    
    if (this->pForceUpdateNextRestart == nullptr) {
        std::cout << "Warning: Failed to get managed ForceUpdateNextRestart" << std::endl;
    }

    std::cout << "Got main method, running" << std::endl;
    return managedMain(argc, argv);
}

CNetBridge::CNetBridge(int argc, const char_t **argv)
{
    if (netbridge != nullptr) {
        std::cout << "Failure: netbridge != nullptr" << std::endl;
        abort();
    }
    
    netbridge = this;
    this->argc = argc;
    this->argv = argv;
}

CNetBridge::~CNetBridge()
{
    if (netbridge == this) {
        netbridge = nullptr;
    }
}

// <SnippetLoadHostFxr>
// Using the nethost library, discover the location of hostfxr and get exports
bool CNetBridge::load_hostfxr(const char_t *assembly_path)
{
    get_hostfxr_parameters params { sizeof(get_hostfxr_parameters), assembly_path, nullptr };
    // Pre-allocate a large buffer for the path to hostfxr
    char_t buffer[MAX_PATH];
    size_t buffer_size = sizeof(buffer) / sizeof(char_t);
    int rc = get_hostfxr_path(buffer, &buffer_size, &params);
    if (rc != 0)
        return false;

    // Load hostfxr and get desired exports
    void *lib = load_library(buffer);
    init_for_cmd_line_fptr = (hostfxr_initialize_for_dotnet_command_line_fn)get_export(lib, "hostfxr_initialize_for_dotnet_command_line");
    init_for_config_fptr = (hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
    get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
    run_app_fptr = (hostfxr_run_app_fn)get_export(lib, "hostfxr_run_app");
    close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

    return (init_for_config_fptr && get_delegate_fptr && close_fptr);
}

// Load and initialize .NET Core and get desired function pointer for scenario
load_assembly_and_get_function_pointer_fn CNetBridge::get_dotnet_load_assembly(const char_t *root_dir)
{
    // This isn't ideal, but it works.
    auto str = string_t(root_dir);
    str.append(STR("/"));
    str.append(STR("OpenSteamClient.dll"));

    // Copy the args but replace first arg with the DLL to avoid errors
    // This line is a bit evil, but since we never delete the array it's basically const
    const char_t **fakeargs = (const char_t**)new char_t*[argc];
    for (size_t i = 0; i < argc; i++)
    {
        if (i == 0) {
            fakeargs[0] = str.c_str();
            continue;
        }

        fakeargs[i] = argv[i];
    }

    // Load .NET Core
    void *load_assembly_and_get_function_pointer = nullptr;
    cxt = nullptr;

    hostfxr_initialize_parameters params;
    params.size = sizeof(params);
    params.dotnet_root = root_dir;        // Three levels up from hostfxr typically
    params.host_path = str.c_str(); // Path to the current executable

    int rc = init_for_cmd_line_fptr(argc, fakeargs, &params, &cxt);
    // int rc = init_for_config_fptr(config_path, nullptr, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        std::cerr << "Init failed: " << std::hex << std::showbase << rc << std::endl;
        close_fptr(cxt);
        return nullptr;
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hdt_load_assembly_and_get_function_pointer,
        &load_assembly_and_get_function_pointer);
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
        std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;

    return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
}

void *CNetBridge::GetFunction(const string_t &className, const string_t &funcName, const char_t *delegate_type_name) {
    if (this->load_assembly_and_get_function_pointer == nullptr) {
        std::cout << "Failure: load_assembly_and_get_function_pointer == nullptr" << std::endl;
        abort();
    }

    void *func = nullptr;

    int rc = this->load_assembly_and_get_function_pointer(
        this->managed_path.c_str(),
        className.c_str(),
        funcName.c_str(),
        delegate_type_name,
        nullptr,
        (void**)&func);

    if (rc != 0 || func == nullptr) {
        std::cout << "Failure: load_assembly_and_get_function_pointer()" << std::endl;
        return nullptr;
    }

    return func;
}