using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using Microsoft.Win32;

namespace SentinelSec_InstallDiscord
{
    internal class Program
    {
        static int Main(string[] args)
        {
            const string appName = "SentinelSec_DiscordProvision";
            const string discordDownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64";
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");
            string registryBaseKey = @"Software\SentinelSec\DiscordProvisioning";

            // Set console title
            Console.Title = "SentinelSec Studios - Discord Provisioning Utility";

            try
            {
                if (args.Length == 0 || string.Equals(args[0], "/help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp();
                    return 0; // Exit code 0: Success
                }

                if (string.Equals(args[0], "/install", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Installing Discord provisioning utility...");
                    ConfigureForLoginCheck(appName, tempInstallerPath, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Installation complete. Login checks enabled.");
                    return 0; // Exit code 0: Success
                }

                if (string.Equals(args[0], "/check", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Checking Discord installation...");
                    return CheckAndInstallDiscord(discordFolder, discordDownloadUrl, tempInstallerPath);
                }

                if (string.Equals(args[0], "/optout", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsAdministrator())
                    {
                        Console.WriteLine("[SentinelSec Studios] Opt-out requires administrative privileges.");
                        return 2; // Exit code 2: Insufficient privileges
                    }

                    OptOutMachine(appName, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Machine-wide opt-out complete. Boot logon entry removed.");
                    return 0; // Exit code 0: Success
                }

                Console.WriteLine("[SentinelSec Studios] Invalid or missing argument. Use /install, /check, /optout, or /help.");
                return 3; // Exit code 3: Invalid argument
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] An error occurred: {ex.Message}");
                return 4; // Exit code 4: General error
            }
            finally
            {
                if (File.Exists(tempInstallerPath))
                {
                    File.Delete(tempInstallerPath);
                    Console.WriteLine("[SentinelSec Studios] Temporary installer file cleaned up.");
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("SentinelSec Studios - Discord Provisioning Utility");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("This utility ensures Discord is installed and operational on user systems.");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  /install         - Configures login checks and enables provisioning.");
            Console.WriteLine("  /check           - Checks for Discord installation and installs it if missing.");
            Console.WriteLine("  /optout          - Removes login checks for all users (requires admin).");
            Console.WriteLine("  /help            - Displays this help message.");
        }

        private static int CheckAndInstallDiscord(string discordFolder, string downloadUrl, string installerPath)
        {
            if (Directory.Exists(discordFolder))
            {
                Console.WriteLine("[SentinelSec Studios] Discord is already installed.");
                return 0; // Exit code 0: Success
            }

            Console.WriteLine("[SentinelSec Studios] Discord is not installed. Downloading the installer...");

            if (!DownloadFile(downloadUrl, installerPath))
            {
                Console.WriteLine("[SentinelSec Studios] Failed to download the Discord installer.");
                return 1; // Exit code 1: Download failed
            }

            Console.WriteLine("[SentinelSec Studios] Installing Discord...");
            if (!RunInstaller(installerPath))
            {
                Console.WriteLine("[SentinelSec Studios] Discord installation failed.");
                return 2; // Exit code 2: Installation failed
            }

            Console.WriteLine("[SentinelSec Studios] Discord installed successfully.");
            return 0; // Exit code 0: Success
        }

        private static bool DownloadFile(string url, string destinationPath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, destinationPath);
                }

                Console.WriteLine($"[SentinelSec Studios] File downloaded to {destinationPath}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] Failed to download file: {ex.Message}");
                return false;
            }
        }

        private static bool RunInstaller(string installerPath)
        {
            try
            {
                Process? process = Process.Start(installerPath, "/S"); // Silent installation
                process?.WaitForExit();

                if (process?.ExitCode != 0)
                {
                    Console.WriteLine($"[SentinelSec Studios] Installer exited with code {process.ExitCode}.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] Failed to run installer: {ex.Message}");
                return false;
            }
        }

        private static void ConfigureForLoginCheck(string appName, string executablePath, string baseKey)
        {
            using (RegistryKey? runKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                runKey?.SetValue(appName, $"\"{executablePath}\" /check");
            }

            using (RegistryKey? key = Registry.LocalMachine.CreateSubKey(baseKey))
            {
                key?.SetValue("Installed", "True");
            }
        }

        private static void OptOutMachine(string appName, string baseKey)
        {
            using (RegistryKey? runKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                runKey?.DeleteValue(appName, false);
            }

            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\SentinelSec", true))
            {
                key?.DeleteSubKeyTree("DiscordProvisioning", false);
            }
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
