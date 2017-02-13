// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace osu.Desktop.Deploy
{
    internal static class Program
    {
        private const string nuget_path = @"packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe";
        private const string squirrel_path = @"packages\squirrel.windows.1.5.2\tools\Squirrel.exe";
        private const string msbuild_path = @"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe";

        public static string StagingFolder = "Staging";
        public static string ReleasesFolder = "Releases";

        public static string ProjectName = "osu.Desktop";
        public static string CodeSigningCert => Path.Combine(homeDir, "deanherbert.pfx");

        private static string codeSigningCmd => $"/a /f {CodeSigningCert} /p {codeSigningPassword} /t http://timestamp.comodoca.com/authenticode";

        private static string homeDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static string solutionPath => Environment.CurrentDirectory;
        private static string stagingPath => Path.Combine(solutionPath, StagingFolder);
        private static string iconPath => Path.Combine(solutionPath, ProjectName, "lazer.ico");

        private static string codeSigningPassword;

        public static void Main(string[] args)
        {
            findSolutionPath();

            if (Directory.Exists(StagingFolder))
                Directory.Delete(StagingFolder, true);
            Directory.CreateDirectory(StagingFolder);

            string verBase = DateTime.Now.ToString("yyyy.Md.");

            int increment = 0;

            if (!Directory.Exists(ReleasesFolder))
            {
                Console.WriteLine("WARNING: No files found in the release directory. Make sure you want this.");
                Directory.CreateDirectory(ReleasesFolder);
            }
            else
            {
                Console.WriteLine("Existing releases:");
                foreach (var l in File.ReadAllLines(Path.Combine(ReleasesFolder, "RELEASES")))
                    Console.WriteLine(l);
                Console.WriteLine();
            }

            //increment build number until we have a unique one.
            while (Directory.GetFiles(ReleasesFolder, $"*{verBase}{increment}*").Any())
                increment++;

            string ver = $"{verBase}{increment}";

            Console.Write(ver);

            ver += Console.ReadLine();

            Console.WriteLine("Enter code signing password:");

            codeSigningPassword = Console.ReadLine();

            Console.WriteLine("Restoring NuGet packages...");
            runCommand(nuget_path, "restore " + solutionPath);

            Console.WriteLine("Running build process...");
            runCommand(msbuild_path, $"/v:quiet /m /t:Client\\{ProjectName.Replace('.', '_')} /p:OutputPath={stagingPath};Configuration=Release osu.sln");

            Console.WriteLine("Creating NuGet deployment package...");
            runCommand(nuget_path, $"pack osu.Desktop\\osu.nuspec -Version {ver} -Properties Configuration=Deploy -OutputDirectory {stagingPath} -BasePath {stagingPath}");

            Console.WriteLine("Releasifying package...");
            runCommand(squirrel_path, $"--releasify {stagingPath}\\osulazer.{ver}.nupkg --setupIcon {iconPath} --icon {iconPath} -n \"{codeSigningCmd}\" --no-msi");

            //rename setup to install.
            File.Copy(Path.Combine(ReleasesFolder, "Setup.exe"), Path.Combine(ReleasesFolder, "install.exe"), true);
            File.Delete(Path.Combine(ReleasesFolder, "Setup.exe"));

            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        /// <summary>
        /// Find the base path of the osu! solution (git checkout location)
        /// </summary>
        private static void findSolutionPath()
        {
            string path = Path.GetDirectoryName(Environment.CommandLine.Replace("\"", "").Trim());

            if (string.IsNullOrEmpty(path))
                path = Environment.CurrentDirectory;

            while (!File.Exists(path + "\\osu.sln"))
                path = path.Remove(path.LastIndexOf('\\'));
            path += "\\";

            Environment.CurrentDirectory = path;
        }

        private static bool runCommand(string command, string args)
        {
            var psi = new ProcessStartInfo(command, args)
            {
                WorkingDirectory = solutionPath,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            if (p.ExitCode == 0) return true;

            Console.WriteLine(output);
            error($"Command {command} {args} failed!");
            return false;
        }

        private static void error(string p)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + p);

            Console.ReadLine();
            Environment.Exit(-1);
        }
    }
}
