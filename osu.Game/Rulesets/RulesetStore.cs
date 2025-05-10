// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Rulesets
{
    public abstract class RulesetStore : IDisposable, IRulesetStore
    {
        private const string ruleset_library_prefix = @"osu.Game.Rulesets";

        protected readonly Dictionary<Assembly, Type> LoadedAssemblies = new Dictionary<Assembly, Type>();
        protected readonly HashSet<Assembly> UserRulesetAssemblies = new HashSet<Assembly>();
        protected readonly Storage? RulesetStorage;

        /// <summary>
        /// All available rulesets.
        /// </summary>
        public abstract IEnumerable<RulesetInfo> AvailableRulesets { get; }

        protected RulesetStore(Storage? storage = null)
        {
            // On android in release configuration assemblies are loaded from the apk directly into memory.
            // We cannot read assemblies from cwd, so should check loaded assemblies instead.
            loadFromAppDomain();

            // This null check prevents Android from attempting to load the rulesets from disk,
            // as the underlying path "AppContext.BaseDirectory", despite being non-nullable, it returns null on android.
            // See https://github.com/xamarin/xamarin-android/issues/3489.
            if (RuntimeInfo.StartupDirectory.IsNotNull())
                loadFromDisk();

            // the event handler contains code for resolving dependency on the game assembly for rulesets located outside the base game directory.
            // It needs to be attached to the assembly lookup event before the actual call to loadUserRulesets() else rulesets located out of the base game directory will fail
            // to load as unable to locate the game core assembly.
            AppDomain.CurrentDomain.AssemblyResolve += resolveRulesetDependencyAssembly;

            RulesetStorage = storage?.GetStorageForDirectory(@"rulesets");
            if (RulesetStorage != null)
                loadUserRulesets(RulesetStorage);
        }

        /// <summary>
        /// Retrieve a ruleset using a known ID.
        /// </summary>
        /// <param name="id">The ruleset's internal ID.</param>
        /// <returns>A ruleset, if available, else null.</returns>
        public RulesetInfo? GetRuleset(int id) => AvailableRulesets.FirstOrDefault(r => r.OnlineID == id);

        /// <summary>
        /// Retrieve a ruleset using a known short name.
        /// </summary>
        /// <param name="shortName">The ruleset's short name.</param>
        /// <returns>A ruleset, if available, else null.</returns>
        public RulesetInfo? GetRuleset(string shortName) => AvailableRulesets.FirstOrDefault(r => r.ShortName == shortName);

        private Assembly? resolveRulesetDependencyAssembly(object? sender, ResolveEventArgs args)
        {
            var asm = new AssemblyName(args.Name);

            // the requesting assembly may be located out of the executable's base directory, thus requiring manual resolving of its dependencies.
            // this attempts resolving the ruleset dependencies on game core and framework assemblies by returning assemblies with the same assembly name
            // already loaded in the AppDomain.
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                          // Given name is always going to be equally-or-more qualified than the assembly name.
                                          .Where(a =>
                                          {
                                              string? name = a.GetName().Name;
                                              if (name == null)
                                                  return false;

                                              return args.Name.Contains(name, StringComparison.Ordinal);
                                          }).MaxBy(a => a.GetName().Version);

            if (domainAssembly != null)
                return domainAssembly;

            return LoadedAssemblies.Keys.FirstOrDefault(a => a.FullName == asm.FullName);
        }

        private void loadFromAppDomain()
        {
            foreach (var ruleset in AppDomain.CurrentDomain.GetAssemblies())
            {
                string? rulesetName = ruleset.GetName().Name;

                if (rulesetName == null)
                    continue;

                if (!rulesetName.StartsWith(ruleset_library_prefix, StringComparison.InvariantCultureIgnoreCase) || rulesetName.Contains(@"Tests"))
                    continue;

                addRuleset(ruleset);
            }
        }

        private void loadUserRulesets(Storage rulesetStorage)
        {
            var rulesets = rulesetStorage.GetFiles(@".", @$"{ruleset_library_prefix}.*.dll");

            foreach (string? ruleset in rulesets.Where(f => !f.Contains(@"Tests")))
            {
                var assembly = loadRulesetFromFile(rulesetStorage.GetFullPath(ruleset));
                if (assembly != null)
                    UserRulesetAssemblies.Add(assembly);
            }
        }

        private void loadFromDisk()
        {
            try
            {
                // On net6-android (Debug), StartupDirectory can be different from where assemblies are placed.
                // Search sub-directories too.

                string[] files = Directory.GetFiles(RuntimeInfo.StartupDirectory, @$"{ruleset_library_prefix}.*.dll", SearchOption.AllDirectories);

                foreach (string file in files.Where(f => !Path.GetFileName(f).Contains("Tests")))
                    loadRulesetFromFile(file);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Could not load rulesets from directory {RuntimeInfo.StartupDirectory}");
            }
        }

        private Assembly? loadRulesetFromFile(string file)
        {
            string filename = Path.GetFileNameWithoutExtension(file);

            if (LoadedAssemblies.Values.Any(t => Path.GetFileNameWithoutExtension(t.Assembly.Location) == filename))
                return null;

            try
            {
                var assembly = Assembly.LoadFrom(file);
                addRuleset(assembly);
                return assembly;
            }
            catch (Exception e)
            {
                LogFailedLoad(filename, e);
            }

            return null;
        }

        private void addRuleset(Assembly assembly)
        {
            if (LoadedAssemblies.ContainsKey(assembly))
                return;

            // the same assembly may be loaded twice in the same AppDomain (currently a thing in certain Rider versions https://youtrack.jetbrains.com/issue/RIDER-48799).
            // as a failsafe, also compare by FullName.
            if (LoadedAssemblies.Any(a => a.Key.FullName == assembly.FullName))
                return;

            try
            {
                LoadedAssemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
            }
            catch (Exception e)
            {
                LogFailedLoad(assembly.GetName().Name!.Split('.').Last(), e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= resolveRulesetDependencyAssembly;
        }

        protected void LogFailedLoad(string name, Exception exception)
        {
            Logger.Log($"Could not load ruleset \"{name}\". Please check for an update from the developer.", level: LogLevel.Error);
            Logger.Log($"Ruleset load failed: {exception}");
        }

        #region Implementation of IRulesetStore

        IRulesetInfo? IRulesetStore.GetRuleset(int id) => GetRuleset(id);
        IRulesetInfo? IRulesetStore.GetRuleset(string shortName) => GetRuleset(shortName);
        IEnumerable<IRulesetInfo> IRulesetStore.AvailableRulesets => AvailableRulesets;

        #endregion
    }
}
