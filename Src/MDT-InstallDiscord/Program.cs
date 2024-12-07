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
            const string discordDownloadUrl = "https://github.com/ThatOneCodeDev/Discord-MDT-Install/releases/download/1.0/MDT-InstallDiscord.exe";
            string publicDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "MDT-InstallDiscord.exe");
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");

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
                    Console.WriteLine("[SentinelSec Studios] Starting installation...");

                    // Download the utility to Public Documents
                    if (!DownloadFile(discordDownloadUrl, publicDocumentsPath))
                    {
                        Console.WriteLine("[SentinelSec Studios] Failed to download the utility.");
                        return 1; // Exit code 1: Download failed
                    }

                    // Add Run-on-Logon registry entry
                    ConfigureForLoginCheck(appName, publicDocumentsPath);
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

                    OptOutMachine(appName);
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
            Console.WriteLine("  /install         - Downloads the utility to Public Documents and configures login checks.");
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

        private static void ConfigureForLoginCheck(string appName, string executablePath)
        {
            using (RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (runKey == null)
                {
                    Console.WriteLine("[SentinelSec Studios] Failed to access Run registry key.");
                    return;
                }

                runKey.SetValue(appName, $"\"{executablePath}\" /check");
                Console.WriteLine("[SentinelSec Studios] Login check configured successfully.");
            }
        }

        private static void OptOutMachine(string appName)
        {
            using (RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (runKey == null)
                {
                    Console.WriteLine("[SentinelSec Studios] Failed to access Run registry key.");
                    return;
                }

                runKey.DeleteValue(appName, false);
                Console.WriteLine("[SentinelSec Studios] Run-on-logon entry removed.");
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
