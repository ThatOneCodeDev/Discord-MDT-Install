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
            string networkSharePath = @"\\SSCSMS-BVVRND2\SSCS-DSOShare\DeploymentShare$\Applications\Discord - Discord\MDT-InstallDiscord.exe";
            string localPublicPath = @"C:\Users\Public\MDT-InstallDiscord.exe";
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");
            string discordDownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64";

            try
            {
                // /install: Copy executable and configure system
                if (args.Length > 0 && args[0].Equals("/install", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Starting /install process...");

                    // Step 1: Copy the executable to the public folder
                    if (!File.Exists(localPublicPath))
                    {
                        Console.WriteLine($"Copying from network share: {networkSharePath} to {localPublicPath}");
                        File.Copy(networkSharePath, localPublicPath, true);
                        Console.WriteLine("Executable copied to public folder successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Executable already exists in the public folder. Skipping copy.");
                    }

                    // Step 2: Configure the system to run at login with /check
                    Console.WriteLine("Configuring the system to check for Discord at login...");
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.SetValue(appName, $"\"{localPublicPath}\" /check");
                            Console.WriteLine("System configured successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to access the registry. Please run as administrator.");
                            return 1; // General failure
                        }
                    }

                    return 0; // Success
                }

                // /check: Run at login to install Discord if not already installed
                if (args.Length > 0 && args[0].Equals("/check", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Checking if Discord is installed for user: {Environment.UserName}...");
                    if (Directory.Exists(discordFolder))
                    {
                        Console.WriteLine("Discord is already installed. Exiting...");
                        return 2; // Discord already installed
                    }

                    Console.WriteLine("Discord is not installed. Downloading the latest installer...");

                    // Download the latest Discord installer
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(discordDownloadUrl, tempInstallerPath);
                    }
                    Console.WriteLine($"Discord installer downloaded to: {tempInstallerPath}");

                    // Run the installer silently
                    Console.WriteLine("Running the Discord installer...");
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
                        Console.WriteLine("Discord installer encountered an error.");
                        return 4; // Installer error
                    }

                    Console.WriteLine("Discord installation completed successfully.");
                    return 0; // Success
                }

                // Invalid argument handling
                Console.WriteLine("Invalid or missing argument. Use /install or /check.");
                return 100; // Invalid argument
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return 1; // General failure
            }
            finally
            {
                // Cleanup: Delete the installer file if it exists
                if (File.Exists(tempInstallerPath))
                {
                    File.Delete(tempInstallerPath);
                    Console.WriteLine("Temporary installer file cleaned up.");
                }
            }
        }
    }
}
