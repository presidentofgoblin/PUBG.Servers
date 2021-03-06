namespace PlayerUnknown.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Gameloop.Vdf;
    using Gameloop.Vdf.JsonConverter;

    using Newtonsoft.Json.Linq;

    using PlayerUnknown.Launcher.Helpers;

    public class Program
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public static string[] Parameters
        {
            get
            {
                return new[]
                {
                    "-LobbyUrl=https://prod-live-front.playbattlegrounds.com/index.html",
                    "-chineseLicensing"
                };
            }
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            if (SteamHelper.IsGameInstalled("PUBG"))
            {
                var ConfigPath    = Path.Combine(SteamHelper.GetConfigPath(), "loginusers.vdf");
                var ConfigFile    = new FileInfo(ConfigPath);

                if (ConfigFile.Exists == false)
                {
                    throw new FileNotFoundException("loginusers.vdf don't exist.");
                }

                var ConfigValue   = File.ReadAllText(ConfigFile.FullName);
                var ConfigUsers   = VdfConvert.Deserialize(ConfigValue).ToJson();
                var Users         = ConfigUsers.Value;

                if (Users.HasValues == false)
                {
                    return;
                }

                var MostRecent    = Users.Values().FirstOrDefault(User => User.Value<int>("mostrecent") == 1);

                if (MostRecent == null)
                {
                    return;
                }

                var CurrentUser   = new Dictionary<string, object>
                {
                    {
                      "STEAMID", MostRecent.Parent.ToObject<JProperty>().Name
                    },
                    {
                      "SteamUser", MostRecent.Value<string>("AccountName")
                    },
                    {
                      "SteamAppUser", MostRecent.Value<string>("AccountName")
                    },
                    {
                      "SteamAppId", "578080"
                    },
                    {
                      "SteamPath", SteamHelper.GetSteamPath()
                    }
                };

                Log.Info(typeof(Program), "Game is installed.");
                Log.Info(typeof(Program), "Path       : " + SteamHelper.GetGamePath("PUBG"));

                var ExecutablePath = Path.Combine(SteamHelper.GetGamePath("PUBG"), @"TslGame\Binaries\Win64\TslGame.exe");
                var ExecutableFile = new FileInfo(ExecutablePath);

                Log.Info(typeof(Program), "Executable : " + ExecutablePath);

                if (ExecutableFile.Exists)
                {
                    var Executable = new ProcessStartInfo(ExecutablePath, arguments: string.Join(" ", Parameters))
                    {
                        UseShellExecute = false
                    };

                    // Steam

                    Executable.EnvironmentVariables.Add("STEAMID",              (string) CurrentUser["STEAMID"]);
                    Executable.EnvironmentVariables.Add("SteamPath",            (string) CurrentUser["SteamPath"]);
                    Executable.EnvironmentVariables.Add("SteamUser",            (string) CurrentUser["SteamUser"]);
                    Executable.EnvironmentVariables.Add("SteamAppUser",         (string) CurrentUser["SteamAppUser"]);
                    Executable.EnvironmentVariables.Add("SteamGameId",          (string) CurrentUser["SteamAppId"]);
                    Executable.EnvironmentVariables.Add("SteamAppId",           (string) CurrentUser["SteamAppId"]);
                    Executable.EnvironmentVariables.Add("SteamControllerAppId", (string) CurrentUser["SteamAppId"]);

                    // Extras

                    Executable.EnvironmentVariables.Add("ENABLE_VK_LAYER_VALVE_steam_overlay_1", "1");
                    Executable.EnvironmentVariables.Add("SDL_GAMECONTROLLER_ALLOW_STEAM_VIRTUAL_GAMEPAD", "1");
                    Executable.EnvironmentVariables.Add("EnableConfiguratorSupport", "0");

                    // Streaming

                    Executable.EnvironmentVariables.Add("SteamStreamingHardwareEncodingNVIDIA", "1");
                    Executable.EnvironmentVariables.Add("SteamStreamingHardwareEncodingAMD", "1");
                    Executable.EnvironmentVariables.Add("SteamStreamingHardwareEncodingIntel", "1");

                    var Processus   = Process.Start(Executable);

                    // Started !

                    var Handle      = Processus.Handle;
                    var SafeHandle  = Processus.SafeHandle;
                    var ProcId      = Processus.Id;

                    Processus.WaitForInputIdle();

                    bool BEnabled   = Processus.Modules.Cast<ProcessModule>().Any(Module => Module.ModuleName.StartsWith("BattlEye"));

                    if (BEnabled)
                    {
                        Log.Warning(typeof(Program), "BattlEye is enabled.");
                    }
                }
                else
                {
                    Log.Warning(typeof(Program), "Executable not found.");
                }
            }
            else
            {
                Log.Warning(typeof(Program), "Game not found.");
            }

            Console.ReadKey(false);
        }
    }
}
