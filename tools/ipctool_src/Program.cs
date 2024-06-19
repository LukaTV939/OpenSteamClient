﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Globalization;

public class Program
{

    private static IPCClient? anyClient;
    public static void Main(string[] args)
    {
        if (args.Length < 1 || args[0] == "client") {
            string ipAddr = "127.0.0.1:57343";
            if (args.Length == 2) {
                ipAddr = args[1];
            }

            IPCClient client = new(ipAddr, IPCClient.IPCConnectionType.Client);
            anyClient = client;
            Console.WriteLine("Post connect client");

            // while (true)
            // {
            //     System.Threading.Thread.Sleep(1000);
            //     //var data = client.WaitForAnyResponse();
            //     if (client.BGetCallback(out int? callbackID, out byte[]? data)) {
            //         Console.WriteLine("Success! ID: " + callbackID.Value + ", length: " + data.Length);
            //     }
            // }

            // Console.WriteLine("BLoggedOn: " + client.CallIPCFunctionClient<bool>(1, 0x6585013d, 0x658ba6d7));
            // {
            //     GetFunctionInfoFromDump("IClientUser", "GetSteamID", out byte interfaceid, out uint functionid, out uint fencepost, out uint argc);
            //     Console.WriteLine("GetSteamID: " + client.CallIPCFunction<ulong>(anyClient.HSteamUser, interfaceid, functionid, fencepost, Array.Empty<object>()));
            // }

            // {
            //     StringBuilder builder = new(1024);
            //     GetAccountName(builder, 1024);
            // }

            {
                GetFunctionInfoFromDump("IClientUtils", "GetSteamRealm", out byte interfaceid, out uint functionid, out uint fencepost, out uint argc);
                Console.WriteLine("GetSteamRealm: " + client.CallIPCFunction<uint>(0, interfaceid, functionid, fencepost, Array.Empty<object>()));
            }

            // Console.WriteLine("GetInstallPath: " + client.CallIPCFunctionClient<string>(4, 0xab7236cd, 0xad0b8048));
            // // AppId_t appid, DepotId_t depotId, uint workshopItemID, uint unk2, ulong targetManifestID, ulong deltaManifestID, string? targetInstallPath
            // Console.WriteLine("DownloadDepot: " + client.CallIPCFunctionClient<ulong>(16, 0x279a7a09, 0x2a1205ff, (uint)730, (uint)2347771, (ulong)0, (ulong)0, (ulong)0, (uint)0, "/mnt/deathclaw/test"));
        } else if (args[0] == "service") {
            string ipAddr = "127.0.0.1:57344";
            if (args.Length == 2) {
                ipAddr = args[1];
            }

            IPCClient serviceclient = new(ipAddr, IPCClient.IPCConnectionType.Service);
            anyClient = serviceclient;
            Console.WriteLine("Post connect service");
            //serviceclient.CallIPCFunctionService<uint>(2, 0xfe43df34, (uint)231430, "ENVVAR=TEST");
        } else {

            Console.WriteLine("Unhandled IPC connection target " + args[0]);
        }

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        anyClient?.Shutdown();
    }

    public static void GetFunctionInfoFromDump(string interfaceName, string functionName, out byte interfaceid, out uint functionid, out uint fencepost, out uint argc) {
        var jsonfile = JsonDocument.Parse(File.ReadAllText($"../dumped_data/{interfaceName}Map.json")).RootElement;
        JsonElement functions = jsonfile.GetProperty("functions");
        foreach (JsonElement functionJSON in functions.EnumerateArray())
        {
            Console.WriteLine(functionJSON.GetProperty("name").GetString() + " == " + functionName);
            if (functionJSON.GetProperty("name").GetString() == functionName) {
                interfaceid = (byte)uint.Parse(functionJSON.GetProperty("interfaceid").GetString()!, CultureInfo.InvariantCulture.NumberFormat);
                functionid = uint.Parse(functionJSON.GetProperty("functionid").GetString()!, CultureInfo.InvariantCulture.NumberFormat);
                fencepost = uint.Parse(functionJSON.GetProperty("fencepost").GetString()!, CultureInfo.InvariantCulture.NumberFormat);
                argc = uint.Parse(functionJSON.GetProperty("argc").GetString()!, CultureInfo.InvariantCulture.NumberFormat);
                return;
            }
        }

        throw new Exception("Didn't find function");
    }

    //TODO: what is the purpose of this shared IPC memory thingy?
    private static string GetShmFilename() {
        [DllImport("libc")]
        static extern int getuid();
    
        return $"/dev/shm/u{getuid()}-ValveIPCSharedObj-Steam";
    }
}