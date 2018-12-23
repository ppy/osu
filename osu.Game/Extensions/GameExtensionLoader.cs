// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Logging;

namespace osu.Game.Extensions
{
    public static class GameExtensionLoader
    {
        private static readonly Dictionary<Assembly, Type> loaded_assemblies = new Dictionary<Assembly, Type>();

        static GameExtensionLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += currentDomain_AssemblyResolve;

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"{extension_library_prefix}.*.dll"))
                loadExtensionFromFile(file);
        }

        private static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => loaded_assemblies.Keys.FirstOrDefault(a => a.FullName == args.Name);

        private const string extension_library_prefix = "osu.Game.Extensions";

        static void loadExtensionFromFile(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (loaded_assemblies.Values.Any(t => t.Namespace == filename))
                return;

            try
            {
                var assembly = Assembly.LoadFrom(file);
                loaded_assemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && !t.IsAbstract && typeof(IExtension).IsAssignableFrom(t));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to load extension \"{Path.GetFileNameWithoutExtension(file)}\"");
            }
        }

        public static IEnumerable<(Assembly assembly, IExtension extension)> GetExtensions() =>
            loaded_assemblies.Select(pair => (pair.Key, (IExtension)Activator.CreateInstance(pair.Value)));
    }
}
