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
            string downloadUrl = "https://github.com/ThatOneCodeDev/Discord-MDT-Install/releases/download/1.0/MDT-InstallDiscord.exe";
            string localPublicPath = @"C:\Users\Public\MDT-InstallDiscord.exe";
            string discordFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord");
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "DiscordSetup.exe");
            string discordDownloadUrl = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64";

            try
            {
                if (args.Length > 0 && args[0].Equals("/install", StringComparison.OrdinalIgnoreCase))
                {
                    // Step 1: Download the executable from GitHub
                    Console.WriteLine("Starting /install process...");
                    if (!File.Exists(localPublicPath))
                    {
                        Console.WriteLine("Downloading executable from GitHub...");
                        using (var client = new System.Net.WebClient())
                        {
                            client.DownloadFile(downloadUrl, localPublicPath);
                        }
                        Console.WriteLine($"Executable downloaded successfully to: {localPublicPath}");
                    }
                    else
                    {
                        Console.WriteLine("Executable already exists in the public folder. Skipping download.");
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

                if (args.Length > 0 && args[0].Equals("/check", StringComparison.OrdinalIgnoreCase))
                {
                    // Step 1: Check if Discord is already installed
                    Console.WriteLine($"Checking if Discord is installed for user: {Environment.UserName}...");
                    if (Directory.Exists(discordFolder))
                    {
                        Console.WriteLine("Discord is already installed. Exiting...");
                        return 2; // Discord already installed
                    }

                    // Step 2: Download the latest Discord installer
                    Console.WriteLine("Discord is not installed. Downloading the latest installer...");
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(discordDownloadUrl, tempInstallerPath);
                    }
                    Console.WriteLine($"Discord installer downloaded to: {tempInstallerPath}");

                    // Step 3: Run the installer silently
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
