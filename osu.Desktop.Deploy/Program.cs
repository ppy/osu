// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using WebRequest = osu.Framework.IO.Network.WebRequest;

namespace osu.Desktop.Deploy
{
    internal static class Program
    {
        private const string nuget_path = @"packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe";
        private const string squirrel_path = @"packages\squirrel.windows.1.7.5\tools\Squirrel.exe";
        private const string msbuild_path = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";

        public static string StagingFolder = ConfigurationManager.AppSettings["StagingFolder"];
        public static string ReleasesFolder = ConfigurationManager.AppSettings["ReleasesFolder"];
        public static string SolutionName = ConfigurationManager.AppSettings["SolutionName"];
        public static string ProjectName = ConfigurationManager.AppSettings["ProjectName"];
        public static string NuSpecName = ConfigurationManager.AppSettings["NuSpecName"];
        public static string TargetName = ConfigurationManager.AppSettings["TargetName"];
        public static string PackageName = ConfigurationManager.AppSettings["PackageName"];
        public static string IconName = ConfigurationManager.AppSettings["IconName"];
        public static string CodeSigningCertificate = ConfigurationManager.AppSettings["CodeSigningCertificate"];

        /// <summary>
        /// How many previous build deltas we want to keep when publishing.
        /// </summary>
        private const int keep_delta_count = 3;

        private static string codeSigningCmd => string.IsNullOrEmpty(codeSigningPassword) ? "" : $"-n \"/a /f {codeSigningCertPath} /p {codeSigningPassword} /t http://timestamp.comodoca.com/authenticode\"";

        private static string homeDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string codeSigningCertPath => Path.Combine(homeDir, CodeSigningCertificate);
        private static string solutionPath => Environment.CurrentDirectory;
        private static string stagingPath => Path.Combine(solutionPath, StagingFolder);
        private static string iconPath => Path.Combine(solutionPath, ProjectName, IconName);

        private static string nupkgFilename(string ver) => $"{PackageName}.{ver}.nupkg";
        private static string nupkgDistroFilename(string ver) => $"{PackageName}-{ver}-full.nupkg";

        private static readonly Stopwatch sw = new Stopwatch();

        private static string codeSigningPassword;

        public static void Main(string[] args)
        {
            displayHeader();

            findSolutionPath();

            if (!Directory.Exists(ReleasesFolder))
            {
                write("WARNING: No release directory found. Make sure you want this!", ConsoleColor.Yellow);
                Directory.CreateDirectory(ReleasesFolder);
            }

            refreshDirectory(StagingFolder);

            //increment build number until we have a unique one.
            string verBase = DateTime.Now.ToString("yyyy.Mdd.");
            int increment = 0;
            while (Directory.GetFiles(ReleasesFolder, $"*{verBase}{increment}*").Any())
                increment++;

            string version = $"{verBase}{increment}";

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Ready to deploy {version}: ");
            Console.ReadLine();

            sw.Start();

            if (!string.IsNullOrEmpty(CodeSigningCertificate))
            {
                Console.Write("Enter code signing password: ");
                codeSigningPassword = readLineMasked();
            }

            write("Restoring NuGet packages...");
            runCommand(nuget_path, "restore " + solutionPath);

            write("Updating AssemblyInfo...");
            updateAssemblyInfo(version);

            write("Running build process...");
            runCommand(msbuild_path, $"\"/v:quiet /m /t:{TargetName.Replace('.', '_')} /p:OutputPath={stagingPath};Configuration=Release {SolutionName}.sln\"");

            write("Creating NuGet deployment package...");
            runCommand(nuget_path, $"pack {NuSpecName} -Version {version} -Properties Configuration=Deploy -OutputDirectory {stagingPath} -BasePath {stagingPath}");

            //prune once before checking for files so we can avoid erroring on files which aren't even needed for this build.
            pruneReleases();

            checkReleaseFiles();

            write("Running squirrel build...");
            runCommand(squirrel_path, $"--releasify {stagingPath}\\{nupkgFilename(version)} --setupIcon {iconPath} --icon {iconPath} {codeSigningCmd} --no-msi");

            //prune again to clean up before upload.
            pruneReleases();

            //rename setup to install.
            File.Copy(Path.Combine(ReleasesFolder, "Setup.exe"), Path.Combine(ReleasesFolder, "install.exe"), true);
            File.Delete(Path.Combine(ReleasesFolder, "Setup.exe"));

            //reset assemblyinfo.
            updateAssemblyInfo("0.0.0");

            write("Done!", ConsoleColor.White);
            Console.ReadLine();
        }

        private static void displayHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("  Please note that OSU! and PPY are registered trademarks and as such covered by trademark law." + " \n  Do not distribute builds of this project publicly that make use of these.");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Ensure we have all the files in the release directory which are expected to be there.
        /// This should have been accounted for in earlier steps, and just serves as a verification step.
        /// </summary>
        private static void checkReleaseFiles()
        {
            var releaseLines = getReleaseLines();

            //ensure we have all files necessary
            foreach (var l in releaseLines)
                if (!File.Exists(Path.Combine(ReleasesFolder, l.Filename)))
                    error($"Local file missing {l.Filename}");
        }

        private static IEnumerable<ReleaseLine> getReleaseLines() => File.ReadAllLines(Path.Combine(ReleasesFolder, "RELEASES")).Select(l => new ReleaseLine(l));

        private static void pruneReleases()
        {
            write("Pruning RELEASES...");

            var releaseLines = getReleaseLines().ToList();

            var fulls = releaseLines.Where(l => l.Filename.Contains("-full")).Reverse().Skip(1);

            //remove any FULL releases (except most recent)
            foreach (var l in fulls)
            {
                write($"- Removing old release {l.Filename}", ConsoleColor.Yellow);
                File.Delete(Path.Combine(ReleasesFolder, l.Filename));
                releaseLines.Remove(l);
            }

            //remove excess deltas
            var deltas = releaseLines.Where(l => l.Filename.Contains("-delta")).ToArray();
            if (deltas.Length > keep_delta_count)
            {
                foreach (var l in deltas.Take(deltas.Length - keep_delta_count))
                {
                    write($"- Removing old delta {l.Filename}", ConsoleColor.Yellow);
                    File.Delete(Path.Combine(ReleasesFolder, l.Filename));
                    releaseLines.Remove(l);
                }
            }

            var lines = new List<string>();
            releaseLines.ForEach(l => lines.Add(l.ToString()));
            File.WriteAllLines(Path.Combine(ReleasesFolder, "RELEASES"), lines);
        }

        private static void refreshDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }

        private static void updateAssemblyInfo(string version)
        {
            string file = Path.Combine(ProjectName, "Properties", "AssemblyInfo.cs");

            var l1 = File.ReadAllLines(file);
            List<string> l2 = new List<string>();
            foreach (var l in l1)
            {
                if (l.StartsWith("[assembly: AssemblyVersion("))
                    l2.Add($"[assembly: AssemblyVersion(\"{version}\")]");
                else if (l.StartsWith("[assembly: AssemblyFileVersion("))
                    l2.Add($"[assembly: AssemblyFileVersion(\"{version}\")]");
                else
                    l2.Add(l);
            }

            File.WriteAllLines(file, l2);
        }

        /// <summary>
        /// Find the base path of the active solution (git checkout location)
        /// </summary>
        private static void findSolutionPath()
        {
            string path = Path.GetDirectoryName(Environment.CommandLine.Replace("\"", "").Trim());

            if (string.IsNullOrEmpty(path))
                path = Environment.CurrentDirectory;

            while (!File.Exists(Path.Combine(path, $"{SolutionName}.sln")))
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
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = Process.Start(psi);
            if (p == null) return false;

            string output = p.StandardOutput.ReadToEnd();
            output += p.StandardError.ReadToEnd();

            if (p.ExitCode == 0) return true;

            write(output);
            error($"Command {command} {args} failed!");
            return false;
        }

        private static string readLineMasked()
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            var ret = Console.ReadLine();
            Console.ForegroundColor = fg;

            return ret;
        }

        private static void error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL ERROR: {message}");

            Console.ReadLine();
            Environment.Exit(-1);
        }

        private static void write(string message, ConsoleColor col = ConsoleColor.Gray)
        {
            if (sw.ElapsedMilliseconds > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(sw.ElapsedMilliseconds.ToString().PadRight(8));
            }
            Console.ForegroundColor = col;
            Console.WriteLine(message);
        }
    }

    internal class RawFileWebRequest : WebRequest
    {
        public RawFileWebRequest(string url) : base(url)
        {
        }

        protected override HttpWebRequest CreateWebRequest(string requestString = null)
        {
            var req = base.CreateWebRequest(requestString);
            req.Accept = "application/octet-stream";
            return req;
        }
    }

    internal class ReleaseLine
    {
        public string Hash;
        public string Filename;
        public int Filesize;

        public ReleaseLine(string line)
        {
            var split = line.Split(' ');
            Hash = split[0];
            Filename = split[1];
            Filesize = int.Parse(split[2]);
        }

        public override string ToString() => $"{Hash} {Filename} {Filesize}";
    }
}
