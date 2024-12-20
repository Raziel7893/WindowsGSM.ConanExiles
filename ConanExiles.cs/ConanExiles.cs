﻿using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;
using System.IO;
using System.Collections.Generic;

namespace WindowsGSM.Plugins
{
    public class ConanExiles : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.ConanExiles", // WindowsGSM.XXXX
            author = "Soul",
            description = "\U0001f9e9 A plugin version of the Conan Exiles Dedicated server for WindowsGSM",
            version = "1.2",
            url = "https://github.com/Soulflare3/WindowsGSM.ConanExiles", // Github repository link (Best practice)
            color = "#7a0101" // Color Hex
        };

        // - Standard Constructor and properties
        public ConanExiles(ServerConfig serverData) : base(serverData) => base.serverData =serverData;
        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "443030";
        public string GameId => "440900";
        public string ModListFile => "ModList.txt";

        // - Game server Fixed variables
        public override string StartPath => @"ConanSandbox\Binaries\Win64\ConanSandboxServer-Win64-Shipping.exe";
        public string FullName = "Conan Exiles Dedicated Server";
        public bool AllowsEmbedConsole = true;
        public int PortIncrements = 2;
        public object QueryMethod = new A2S();

        // - Game server default values
        public string Port = "7777";
        public string ServerName = "Conan Exiles Dedicated Server";
        public string QueryPort = "27015";
        public string Defaultmap = "game.db";
        public string Maxplayers = "40";
        public string Additional = "";

        public struct ModInfo
        {
            public string AppId;
            public string ModId;
            public string FileName;
        }

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            //Download Engine.ini
            string configPath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, @"ConanSandbox\Saved\Config\WindowsServer\Engine.ini");
            if (await Functions.Github.DownloadGameServerConfig(configPath, FullName))
            {
                string configText = File.ReadAllText(configPath);
                configText = configText.Replace("{{ServerName}}", serverData.ServerName);
                configText = configText.Replace("{{ServerPassword}}", serverData.GetRCONPassword());
                File.WriteAllText(configPath, configText);
            }
        }

        public async Task<Process> Start()
        {
            string shipExePath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }

            string param = string.IsNullOrWhiteSpace(serverData.ServerIP) ? string.Empty : $" -MultiHome={serverData.ServerIP}";
            param += string.IsNullOrWhiteSpace(serverData.ServerPort) ? string.Empty : $" -Port={serverData.ServerPort}";
            param += string.IsNullOrWhiteSpace(serverData.ServerQueryPort) ? string.Empty : $" -QueryPort={serverData.ServerQueryPort}";
            param += string.IsNullOrWhiteSpace(serverData.ServerMaxPlayer) ? string.Empty : $" -MaxPlayers={serverData.ServerMaxPlayer}";
            param += $" {serverData.ServerParam}" + (!serverData.EmbedConsole ? " -log" : string.Empty);

            Process p;
            if (!AllowsEmbedConsole)
            {
                p = new Process
                {
                    StartInfo =
                    {
                        FileName = shipExePath,
                        Arguments = param,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };
                p.Start();
            }
            else
            {
                p = new Process
                {
                    StartInfo =
                    {
                        FileName = shipExePath,
                        Arguments = param,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                var serverConsole = new Functions.ServerConsole(serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }

        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            UpdateMods();
            var (p, error) = await Installer.SteamCMD.UpdateEx(serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);

            Error = error;
            return p;
        }

        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p.StartInfo.CreateNoWindow)
                {
                    p.CloseMainWindow();
                }
                else
                {
                    Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                    Functions.ServerConsole.SendWaitToMainWindow("^c");
                }
            });
        }

        private void UpdateMods()
        {
            if (File.Exists(Functions.ServerPath.GetServersServerFiles(serverData.ServerID, ModListFile)))
            {
                var modDestFolder = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, "ConanSandbox", "Mods");
                Directory.CreateDirectory(modDestFolder);

                string[] lines = File.ReadAllLines(Functions.ServerPath.GetServersServerFiles(serverData.ServerID, ModListFile));
                List<ModInfo> mods = new List<ModInfo>();

                foreach (string line in lines)
                {
                    string tmpLine = line.Replace("\\", "/");
                    var elements = tmpLine.Split('/');
                    if (elements.Length > 3)
                    {
                        mods.Add(new ModInfo { AppId = elements[elements.Length - 3], ModId = elements[elements.Length - 2], FileName = elements[elements.Length - 1] });
                    }
                }
                DownloadMods(mods);
                var modlistContent = new StringBuilder();
                foreach (ModInfo mod in mods)
                {
                    modlistContent.AppendLine($"*{mod.FileName}");
                }

                CopyModPaks(modDestFolder);

                File.WriteAllText($"{modDestFolder}\\{ModListFile}", modlistContent.ToString());
            }
        }

        private void CopyModPaks(string destination)
        {
            string sourcePath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, "steamapps\\workshop\\content", GameId);
            if (!Directory.Exists(sourcePath))
                return;
            var files = Directory.GetFiles(sourcePath, "*.pak", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string dest = Path.Combine(destination, Path.GetFileName(file));
                //only copy if changed
                if (new FileInfo(file).CreationTimeUtc > new FileInfo(dest).CreationTimeUtc)
                    File.Copy(file, dest);
            }
        }

        private void DownloadMods(List<ModInfo> mods)
        {
            string _exeFile = "steamcmd.exe";
            string _installPath = ServerPath.GetBin("steamcmd");

            string exePath = Path.Combine(_installPath, _exeFile);

            if (!File.Exists(exePath))
            {
                Error = $"SteamCMD not available, break up";
                return;
            }

            StringBuilder sb = new StringBuilder($"+force_install_dir \"{Functions.ServerPath.GetServersServerFiles(serverData.ServerID)}\" +login anonymous");
            foreach (var mod in mods)
            {
                if (!string.IsNullOrEmpty(mod.AppId) && !string.IsNullOrEmpty(mod.ModId) && !string.IsNullOrEmpty(mod.FileName))
                    sb.Append($" +workshop_download_item {mod.AppId} {mod.ModId}");
            }

            sb.Append($" +quit");

            Process p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = _installPath,
                    FileName = exePath,
                    Arguments = sb.ToString(),
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            p.Start();
            p.WaitForExit();
            return;
        }
    }
}