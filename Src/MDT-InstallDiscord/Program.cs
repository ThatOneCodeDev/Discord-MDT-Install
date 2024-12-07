using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace SentinelSec_InstallDiscord
{
    internal class Program
    {
        static int Main(string[] args)
        {
            const string appName = "SentinelSec_DiscordProvision";
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");
            string discordDownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64";
            string localPublicPath = @"C:\Users\Public\MDT-InstallDiscord.exe";
            string registryBaseKey = @"Software\SentinelSec\DiscordProvisioning";
            string sourceCodeUrl = "https://github.com/ThatOneCodeDev/Discord-MDT-Install";

            // Set console title
            Console.Title = "SentinelSec Studios - Discord Provisioning Utility";

            try
            {
                if (args.Length == 0 || string.Equals(args[0], "/help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowHelp(sourceCodeUrl);
                    return 0;
                }

                string currentUser = Environment.UserName.ToLowerInvariant();

                // Handle restricted accounts
                if (currentUser == "administrator" || currentUser == "sscs_admin")
                {
                    Console.WriteLine("[SentinelSec Studios] Restricted account detected. Skipping Discord provisioning.");
                    return 0;
                }

                // Handle /install argument
                if (string.Equals(args[0], "/install", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Starting installation...");
                    ConfigureForLoginCheck(appName, localPublicPath, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Installation complete. Login checks enabled.");
                    return 0;
                }

                // Handle /check argument
                if (string.Equals(args[0], "/check", StringComparison.OrdinalIgnoreCase))
                {
                    return ProvisionDiscord(tempInstallerPath, discordFolder, discordDownloadUrl);
                }

                // Handle /optout argument (removes boot logon entry)
                if (string.Equals(args[0], "/optout", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsAdministrator())
                    {
                        Console.WriteLine("[SentinelSec Studios] Opt-out requires administrative privileges.");
                        return 1;
                    }

                    OptOutMachine(appName, registryBaseKey);
                    Console.WriteLine("[SentinelSec Studios] Machine-wide opt-out complete. Boot logon entry removed.");
                    return 0;
                }

                // Invalid or missing arguments
                Console.WriteLine("[SentinelSec Studios] Invalid or missing argument. Use /install, /check, or /optout.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] An error occurred: {ex.Message}");
                return 1;
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

        private static void ShowHelp(string sourceCodeUrl)
        {
            Console.WriteLine("SentinelSec Studios - Discord Provisioning Utility");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("This utility ensures Discord is installed and operational on user systems.");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  /install         - Installs the utility, configures login checks.");
            Console.WriteLine("  /check           - Runs the login check manually (normally auto-run at login).");
            Console.WriteLine("  /optout          - Removes boot logon entry for this utility (requires admin).");
            Console.WriteLine("  /help            - Displays this help message.");
            Console.WriteLine($"\nFor more information or to view the source code: {sourceCodeUrl}");
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

        private static int ProvisionDiscord(string installerPath, string discordFolder, string discordUrl)
        {
            if (Directory.Exists(discordFolder))
            {
                Console.WriteLine("[SentinelSec Studios] Discord is already installed.");
                return 0;
            }

            Console.WriteLine("[SentinelSec Studios] Downloading Discord...");
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(discordUrl, installerPath);
            }

            Console.WriteLine("[SentinelSec Studios] Installing Discord...");
            Process? installerProcess = Process.Start(installerPath, "/S");

            installerProcess?.WaitForExit();

            if (installerProcess?.ExitCode != 0)
            {
                Console.WriteLine("[SentinelSec Studios] Discord installation failed.");
                return 1;
            }

            Console.WriteLine("[SentinelSec Studios] Discord installed successfully.");
            return 0;
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
