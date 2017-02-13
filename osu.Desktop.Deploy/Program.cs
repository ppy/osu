// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.IO.Network;

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

        const int keep_delta_count = 3;

        private static string codeSigningCmd => string.IsNullOrEmpty(codeSigningPassword) ? "" : $"-n \"/a /f {CodeSigningCert} /p {codeSigningPassword} /t http://timestamp.comodoca.com/authenticode\"";

        private static string homeDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static string solutionPath => Environment.CurrentDirectory;
        private static string stagingPath => Path.Combine(solutionPath, StagingFolder);
        private static string iconPath => Path.Combine(solutionPath, ProjectName, "lazer.ico");

        private static string nupkgFilename(string ver) => $"osulazer.{ver}.nupkg";
        private static string nupkgDistroFilename(string ver) => $"osulazer-{ver}-full.nupkg";

        private static string codeSigningPassword;

        public static void Main(string[] args)
        {
            findSolutionPath();

            if (!Directory.Exists(ReleasesFolder))
            {
                Console.WriteLine("WARNING: No files found in the release directory. Make sure you want this.");
                Directory.CreateDirectory(ReleasesFolder);
            }

            Console.WriteLine("Checking github releases...");

            var req = new JsonWebRequest<List<GitHubObject>>("https://api.github.com/repos/ppy/osu/releases");
            req.BlockingPerform();

            if (req.ResponseObject.Count > 0)
            {
                var release = req.ResponseObject[0];
                Console.WriteLine($"Last version pushed was {release.Name}");

                if (!File.Exists(Path.Combine(ReleasesFolder, nupkgDistroFilename(release.Name))))
                {
                    Console.WriteLine("Not found locally; let's pull down last release data.");

                    req = new JsonWebRequest<List<GitHubObject>>($"https://api.github.com/repos/ppy/osu/releases/{release.Id}/assets");
                    req.BlockingPerform();

                    foreach (var asset in req.ResponseObject)
                    {
                        Console.WriteLine($"Downloading {asset.Name}...");
                        var dl = new FileWebRequest(Path.Combine(ReleasesFolder, asset.Name), $"https://api.github.com/repos/ppy/osu/releases/assets/{asset.Id}");
                        dl.BlockingPerform();
                    }
                }
            }

            if (Directory.Exists(StagingFolder))
                Directory.Delete(StagingFolder, true);
            Directory.CreateDirectory(StagingFolder);

            string verBase = DateTime.Now.ToString("yyyy.Md.");
            int increment = 0;

            //increment build number until we have a unique one.
            while (Directory.GetFiles(ReleasesFolder, $"*{verBase}{increment}*").Any())
                increment++;

            string version = $"{verBase}{increment}";

            Console.Write($"Ready to deploy {version}: ");
            version += Console.ReadLine();

            Console.Write("Enter code signing password: ");
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            codeSigningPassword = Console.ReadLine();
            Console.ForegroundColor = fg;


            Console.WriteLine("Restoring NuGet packages...");
            runCommand(nuget_path, "restore " + solutionPath);

            Console.WriteLine("Updating AssemblyInfo...");
            updateAssemblyInfo(version);

            Console.WriteLine("Running build process...");
            runCommand(msbuild_path, $"/v:quiet /m /t:Client\\{ProjectName.Replace('.', '_')} /p:OutputPath={stagingPath};Configuration=Release osu.sln");

            Console.WriteLine("Creating NuGet deployment package...");
            runCommand(nuget_path, $"pack osu.Desktop\\osu.nuspec -Version {version} -Properties Configuration=Deploy -OutputDirectory {stagingPath} -BasePath {stagingPath}");

            Console.WriteLine("Pruning RELEASES...");

            var releaseLines = new List<ReleaseLine>();
            foreach (var l in File.ReadAllLines(Path.Combine(ReleasesFolder, "RELEASES"))) releaseLines.Add(new ReleaseLine(l));

            var fulls = releaseLines.Where(l => l.Filename.Contains("-full")).Reverse().Skip(1);

            //remove any FULL releases (except most recent)
            foreach (var l in fulls)
            {
                Console.WriteLine($"- Removing old release {l.Filename}");
                File.Delete(Path.Combine(ReleasesFolder, l.Filename));
                releaseLines.Remove(l);
            }

            //remove excess deltas
            var deltas = releaseLines.Where(l => l.Filename.Contains("-delta"));
            if (deltas.Count() > keep_delta_count)
            {
                foreach (var l in deltas.Take(deltas.Count() - keep_delta_count))
                {
                    Console.WriteLine($"- Removing old delta {l.Filename}");
                    File.Delete(Path.Combine(ReleasesFolder, l.Filename));
                    releaseLines.Remove(l);
                }
            }

            //ensure we have all files necessary
            foreach (var l in releaseLines)
                if (!File.Exists(Path.Combine(ReleasesFolder, l.Filename)))
                    error($"Local file missing {l.Filename}");

            List<string> lines = new List<string>();
            foreach (var l in releaseLines)
                lines.Add(l.ToString());
            File.WriteAllLines(Path.Combine(ReleasesFolder, "RELEASES"), lines);

            Console.WriteLine("Releasifying package...");
            runCommand(squirrel_path, $"--releasify {stagingPath}\\{nupkgFilename(version)} --setupIcon {iconPath} --icon {iconPath} {codeSigningCmd} --no-msi");

            //rename setup to install.
            File.Copy(Path.Combine(ReleasesFolder, "Setup.exe"), Path.Combine(ReleasesFolder, "install.exe"), true);
            File.Delete(Path.Combine(ReleasesFolder, "Setup.exe"));

            Console.WriteLine("Done!");
            Console.ReadLine();
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
