using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace osu.Desktop.Deploy
{
    class Program
    {
        static string NUGET_PATH = @"packages\NuGet.CommandLine.3.5.0\tools\NuGet.exe";
        static string SQUIRREL_PATH = @"packages\squirrel.windows.1.5.2\tools\Squirrel.exe";
        static string MSBUILD_PATH = @"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe";

        static string STAGING_FOLDER = "Staging";
        static string PROJECT_NAME = "osu.Desktop";

        static string CODE_SIGNING_CMD => $"/a /f {CODE_SIGNING_CERT} /p {codeSigningPassword} /t http://timestamp.comodoca.com/authenticode";
        static string IconPath => Path.Combine(SolutionPath, PROJECT_NAME, "lazer.ico");

        static internal string HomeDir { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); } }
        static internal string CODE_SIGNING_CERT => Path.Combine(HomeDir, "deanherbert.pfx");

        static string SolutionPath => Environment.CurrentDirectory;
        static string StagingPath => Path.Combine(SolutionPath, STAGING_FOLDER);

        static string codeSigningPassword;

        static void Main(string[] args)
        {
            FindSolutionPath();

            if (Directory.Exists(STAGING_FOLDER))
                Directory.Delete(STAGING_FOLDER, true);
            Directory.CreateDirectory(STAGING_FOLDER);

            string verBase = DateTime.Now.ToString("yyyy.Md.");

            int increment = 0;

            while (Directory.GetFiles("Releases", $"*{verBase}{increment}*").Count() > 0)
                increment++;

            string ver = $"{verBase}{increment}";

            Console.Write(ver);

            ver += Console.ReadLine();

            Console.WriteLine("Enter code signing password:");

            codeSigningPassword = Console.ReadLine();

            Console.WriteLine("Restoring NuGet packages...");
            RunCommand(NUGET_PATH, "restore " + SolutionPath);

            Console.WriteLine("Running build process...");
            RunCommand(MSBUILD_PATH, $"/v:quiet /m /t:Client\\{PROJECT_NAME.Replace('.', '_')} /p:OutputPath={StagingPath};Configuration=Release osu.sln");

            Console.WriteLine("Creating NuGet deployment package...");
            RunCommand(NUGET_PATH, $"pack osu.Desktop\\osu.nuspec -Version {ver} -Properties Configuration=Deploy -OutputDirectory {StagingPath} -BasePath {StagingPath}");

            Console.WriteLine("Releasifying package...");
            RunCommand(SQUIRREL_PATH, $"--releasify {StagingPath}\\osulazer.{ver}.nupkg --setupIcon {IconPath} --icon {IconPath} -n \"{CODE_SIGNING_CMD}\" --no-msi");

            File.Copy("Releases\\Setup.exe", "Releases\\install.exe", true);
            File.Delete("Releases\\Setup.exe");

            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        /// <summary>
        /// Find the base path of the osu! solution (git checkout location)
        /// </summary>
        private static void FindSolutionPath()
        {
            string path = Path.GetDirectoryName(Environment.CommandLine.Replace("\"", "").Trim());

            if (string.IsNullOrEmpty(path))
                path = Environment.CurrentDirectory;

            while (!File.Exists(path + "\\osu.sln"))
                path = path.Remove(path.LastIndexOf('\\'));
            path += "\\";

            Environment.CurrentDirectory = path;
        }

        private static bool RunCommand(string command, string args)
        {
            var psi = new ProcessStartInfo(command, args);
            if (psi != null)
            {
                psi.WorkingDirectory = SolutionPath;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                if (p.ExitCode != 0)
                {
                    Console.WriteLine(output);
                    Error($"Command {command} {args} failed!");
                    return false;
                }

                return true;
            }

            return false;
        }

        private static void Error(string p)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + p);

            Console.ReadLine();
            Environment.Exit(-1);
        }
    }
}
