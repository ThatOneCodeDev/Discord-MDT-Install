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
            string publicPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MDT-InstallDiscord.exe");
            string discordDownloadUrl = "https://github.com/ThatOneCodeDev/Discord-MDT-Install/releases/download/1.0/MDT-InstallDiscord.exe";
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
                    Console.WriteLine("[SentinelSec Studios] Starting installation...");
                    if (!DownloadFile(discordDownloadUrl, publicPath))
                    {
                        Console.WriteLine("[SentinelSec Studios] Failed to download the installer.");
                        return 1; // Exit code 1: Download failed
                    }

                    ConfigureForLoginCheck(appName, publicPath, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Installation complete. Login checks enabled.");
                    return 0; // Exit code 0: Success
                }

                if (string.Equals(args[0], "/check", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Checking Discord installation...");
                    return CheckDiscordInstallation();
                }

                if (string.Equals(args[0], "/optout", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsAdministrator())
                    {
                        Console.WriteLine("[SentinelSec Studios] Opt-out requires administrative privileges.");
                        return 2; // Exit code 2: Insufficient privileges
                    }

                    OptOutMachine(appName, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Machine-wide opt-out complete.");
                    return 0; // Exit code 0: Success
                }

                Console.WriteLine("[SentinelSec Studios] Invalid or missing argument. Use /install, /check, or /optout.");
                return 3; // Exit code 3: Invalid argument
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] An error occurred: {ex.Message}");
                return 4; // Exit code 4: General error
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("SentinelSec Studios - Discord Provisioning Utility");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("This utility ensures Discord is installed and operational on user systems.");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  /install         - Installs the utility, downloads the executable, and configures login checks.");
            Console.WriteLine("  /check           - Runs the utility in check mode to ensure Discord is installed.");
            Console.WriteLine("  /optout          - Disables the utility for all users (requires admin).");
            Console.WriteLine("  /help            - Displays this help message.");
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

        private static int CheckDiscordInstallation()
        {
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");

            if (Directory.Exists(discordFolder))
            {
                Console.WriteLine("[SentinelSec Studios] Discord is already installed.");
                return 0; // Exit code 0: Success
            }

            Console.WriteLine("[SentinelSec Studios] Discord is not installed. Please run the utility with /install to enable provisioning.");
            return 5; // Exit code 5: Discord not installed
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
