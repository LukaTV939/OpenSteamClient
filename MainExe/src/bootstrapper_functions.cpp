#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <ostream>
#include <netbridge.h>
#include <exports.h>
#include <cassert>

EXPORT const char *SteamBootstrapper_GetInstallDir()
{
    return netbridge->pSteamBootstrapper_GetInstallDir();
}

EXPORT const char* SteamBootstrapper_GetLoggingDir() {
    return netbridge->pSteamBootstrapper_GetLoggingDir();
}

EXPORT bool StartCheckingForUpdates() {
  return netbridge->pStartCheckingForUpdates();
}

// First function called by steamclient
EXPORT int SteamBootstrapper_GetEUniverse() {
    std::cout << "SteamBootstrapper_GetEUniverse native called" << std::endl;
    assert(netbridge != nullptr);
    assert(netbridge->pSteamBootstrapper_GetEUniverse != nullptr);
    return netbridge->pSteamBootstrapper_GetEUniverse();
}

EXPORT long long int GetBootstrapperVersion() {
    std::cout << "GetBootstrapperVersion native called" << std::endl;
    return netbridge->pGetBootstrapperVersion();
}

EXPORT const char* GetCurrentClientBeta() {
    std::cout << "GetCurrentClientBeta native called" << std::endl;
    return netbridge->pGetCurrentClientBeta();
}

EXPORT void ClientUpdateRunFrame() {
    netbridge->pClientUpdateRunFrame();
}

EXPORT bool IsClientUpdateAvailable() {
    return netbridge->pIsClientUpdateAvailable();
}

EXPORT bool IsClientUpdateOutOfDiskSpace() {
    return netbridge->pIsClientUpdateOutOfDiskSpace();
}

EXPORT bool CanSetClientBeta() {
    return netbridge->pCanSetClientBeta();
}

EXPORT void SetClientBeta(const char* beta) {
    netbridge->pSetClientBeta(beta);
}

EXPORT const char* SteamBootstrapper_GetBaseUserDir() {
    return netbridge->pSteamBootstrapper_GetBaseUserDir();
}

EXPORT void PermitDownloadClientUpdates(bool permit) {
    netbridge->pPermitDownloadClientUpdates(permit);
}

EXPORT int SteamBootstrapper_GetForwardedCommandLine(char* buf, int bufLen) {
    return netbridge->pSteamBootstrapper_GetForwardedCommandLine(buf, bufLen);
}

EXPORT void SteamBootstrapper_SetCommandLineToRunOnExit(const char* cmdLine) {
    netbridge->pSteamBootstrapper_SetCommandLineToRunOnExit(cmdLine);
}

EXPORT int GetClientLauncherType() {
    return netbridge->pGetClientLauncherType();
}

EXPORT void ForceUpdateNextRestart() {
    netbridge->pForceUpdateNextRestart();
}


