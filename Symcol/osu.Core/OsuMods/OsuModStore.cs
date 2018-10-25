using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Logging;

namespace osu.Core.OsuMods
{
    public static class OsuModStore
    {
        public static List<OsuModSet> LoadedModSets = new List<OsuModSet>();

        private static Dictionary<Assembly, Type> loadedAssemblies = new Dictionary<Assembly, Type>();

        public static void ReloadModSets()
        {
            LoadedModSets = new List<OsuModSet>();

            loadedAssemblies = new Dictionary<Assembly, Type>();

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"osu.Mods.*.dll"))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                if (loadedAssemblies.Values.Any(t => t.Namespace == filename)) return;

                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    loadedAssemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(OsuModSet)));
                }
                catch (Exception)
                {
                    Logger.Log("Error loading a modset from a mod file! [filename = " + filename + "]", LoggingTarget.Runtime, LogLevel.Error);
                }
            }

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"osu.Game.Rulesets.*.dll"))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                if (loadedAssemblies.Values.Any(t => t.Namespace == filename)) return;

                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    loadedAssemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(OsuModSet)));
                }
                catch (Exception)
                {
                    Logger.Log("Error loading a modset from a ruleset (it probably didn't have one [filename = " + filename + "])");
                }
            }

            var instances = loadedAssemblies.Values.Select(g => (OsuModSet)Activator.CreateInstance(g)).ToList();

            //add any other mods
            foreach (OsuModSet s in instances)
                LoadedModSets.Add(s);
        }
    }
}
