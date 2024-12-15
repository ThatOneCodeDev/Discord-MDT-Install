using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace MDT_InstallDiscord
{
    internal class Program
    {
        static int Main(string[] args)
        {
            const string appName = "CheckAndInstallDiscord";
            string localPublicPath = @"C:\Users\Public\MDT-InstallDiscord.exe";
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");
            string discordDownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64";

            try
            {
                // /install: Copy executable and configure system
                if (args.Length > 0 && args[0].Equals("/install", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Starting /install process...");

                    // Step 1: Copy the executable to the public folder
                    if (!File.Exists(localPublicPath))
                    {
                        Console.WriteLine($"[SentinelSec Studios] Copying to public folder: {localPublicPath}");
                        File.Copy(Process.GetCurrentProcess().MainModule.FileName, localPublicPath, true);
                        Console.WriteLine("[SentinelSec Studios] Executable copied to public folder successfully.");
                    }
                    else
                    {
                        Console.WriteLine("[SentinelSec Studios] Executable already exists in the public folder. Skipping copy.");
                    }

                    // Step 2: Configure the system to run at login with /check
                    Console.WriteLine("[SentinelSec Studios] Configuring the system to check for Discord at login...");
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.SetValue(appName, $"\"{localPublicPath}\" /check");
                            Console.WriteLine("[SentinelSec Studios] System configured successfully.");
                        }
                        else
                        {
                            Console.WriteLine("[SentinelSec Studios] Failed to access the registry. Please run as administrator.");
                            return 1; // General failure
                        }
                    }

                    return 0; // Success
                }

                // /check: Run at login to install Discord if not already installed
                if (args.Length > 0 && args[0].Equals("/check", StringComparison.OrdinalIgnoreCase))
                {
                    if (Environment.UserName.ToLower() == "administrator" || Environment.UserName ==  "sscs_admin") { return 0; }
                    Console.WriteLine($"[SentinelSec Studios] Checking if Discord is installed for user: {Environment.UserName}...");

                    // Step 1: Check if Discord is already installed
                    if (Directory.Exists(discordFolder))
                    {
                        Console.WriteLine("[SentinelSec Studios] Discord is already installed. Exiting...");
                        return 0; // Discord already installed
                    }

                    // Step 2: Download the latest Discord installer
                    Console.WriteLine("[SentinelSec Studios] Discord is not installed. Downloading the latest installer...");
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(discordDownloadUrl, tempInstallerPath);
                    }
                    Console.WriteLine($"[SentinelSec Studios] Discord installer downloaded to: {tempInstallerPath}");

                    // Step 3: Run the installer silently
                    Console.WriteLine("[SentinelSec Studios] Running the Discord installer...");
                    Process installerProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = tempInstallerPath,
                        Arguments = "/S",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    });
                    installerProcess.WaitForExit();

                    if (installerProcess.ExitCode != 0)
                    {
                        Console.WriteLine("[SentinelSec Studios] Discord installer encountered an error.");
                        return 4; // Installer error
                    }

                    Console.WriteLine("[SentinelSec Studios] Discord installation completed successfully.");
                    return 0; // Success
                }

                // /optout: Remove Run registry entry
                if (args.Length > 0 && args[0].Equals("/optout", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[SentinelSec Studios] Opting out of automatic login checks...");

                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null && key.GetValue(appName) != null)
                        {
                            key.DeleteValue(appName, false);
                            Console.WriteLine("[SentinelSec Studios] Opt-out successful. Login check entry removed.");
                        }
                        else
                        {
                            Console.WriteLine("[SentinelSec Studios] No login check entry found to remove.");
                        }
                    }

                    return 0; // Success
                }

                // Invalid argument handling
                Console.WriteLine("[SentinelSec Studios] Invalid or missing argument. Use /install, /check, or /optout.");
                return 100; // Invalid argument
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SentinelSec Studios] An unexpected error occurred: {ex.Message}");
                return 1; // General failure
            }
            finally
            {
                // Cleanup: Delete the installer file if it exists
                if (File.Exists(tempInstallerPath))
                {
                    try
                    {
                        File.Delete(tempInstallerPath);
                        Console.WriteLine("[SentinelSec Studios] Temporary installer file cleaned up.");
                    }
                    catch (Exception cleanupEx)
                    {
                        Console.WriteLine($"[SentinelSec Studios] Failed to clean up temporary file: {cleanupEx.Message}");
                    }
                }
            }
        }
    }
}
