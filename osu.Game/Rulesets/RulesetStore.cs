// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Rulesets
{
    public class RulesetStore : DatabaseBackedStore, IRulesetStore, IDisposable
    {
        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        private readonly Dictionary<Assembly, Type> loadedAssemblies = new Dictionary<Assembly, Type>();

        private readonly Storage rulesetStorage;

        public RulesetStore(IDatabaseContextFactory factory, Storage storage = null)
            : base(factory)
        {
            rulesetStorage = storage?.GetStorageForDirectory("rulesets");

            // On android in release configuration assemblies are loaded from the apk directly into memory.
            // We cannot read assemblies from cwd, so should check loaded assemblies instead.
            loadFromAppDomain();

            // This null check prevents Android from attempting to load the rulesets from disk,
            // as the underlying path "AppContext.BaseDirectory", despite being non-nullable, it returns null on android.
            // See https://github.com/xamarin/xamarin-android/issues/3489.
            if (RuntimeInfo.StartupDirectory != null)
                loadFromDisk();

            // the event handler contains code for resolving dependency on the game assembly for rulesets located outside the base game directory.
            // It needs to be attached to the assembly lookup event before the actual call to loadUserRulesets() else rulesets located out of the base game directory will fail
            // to load as unable to locate the game core assembly.
            AppDomain.CurrentDomain.AssemblyResolve += resolveRulesetDependencyAssembly;
            loadUserRulesets();
            addMissingRulesets();
        }

        /// <summary>
        /// Retrieve a ruleset using a known ID.
        /// </summary>
        /// <param name="id">The ruleset's internal ID.</param>
        /// <returns>A ruleset, if available, else null.</returns>
        public RulesetInfo GetRuleset(int id) => AvailableRulesets.FirstOrDefault(r => r.ID == id);

        /// <summary>
        /// Retrieve a ruleset using a known short name.
        /// </summary>
        /// <param name="shortName">The ruleset's short name.</param>
        /// <returns>A ruleset, if available, else null.</returns>
        public RulesetInfo GetRuleset(string shortName) => AvailableRulesets.FirstOrDefault(r => r.ShortName == shortName);

        /// <summary>
        /// All available rulesets.
        /// </summary>
        public IEnumerable<RulesetInfo> AvailableRulesets { get; private set; }

        private Assembly resolveRulesetDependencyAssembly(object sender, ResolveEventArgs args)
        {
            var asm = new AssemblyName(args.Name);

            // the requesting assembly may be located out of the executable's base directory, thus requiring manual resolving of its dependencies.
            // this attempts resolving the ruleset dependencies on game core and framework assemblies by returning assemblies with the same assembly name
            // already loaded in the AppDomain.
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                          // Given name is always going to be equally-or-more qualified than the assembly name.
                                          .Where(a => args.Name.Contains(a.GetName().Name, StringComparison.Ordinal))
                                          // Pick the greatest assembly version.
                                          .OrderByDescending(a => a.GetName().Version)
                                          .FirstOrDefault();

            if (domainAssembly != null)
                return domainAssembly;

            return loadedAssemblies.Keys.FirstOrDefault(a => a.FullName == asm.FullName);
        }

        private void addMissingRulesets()
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                var instances = loadedAssemblies.Values.Select(r => (Ruleset)Activator.CreateInstance(r)).ToList();

                // add all legacy rulesets first to ensure they have exclusive choice of primary key.
                foreach (var r in instances.Where(r => r is ILegacyRuleset))
                {
                    if (context.RulesetInfo.SingleOrDefault(dbRuleset => dbRuleset.ID == r.RulesetInfo.ID) == null)
                        context.RulesetInfo.Add(r.RulesetInfo);
                }

                context.SaveChanges();

                var existingRulesets = context.RulesetInfo.ToList();

                // add any other rulesets which have assemblies present but are not yet in the database.
                foreach (var r in instances.Where(r => !(r is ILegacyRuleset)))
                {
                    if (existingRulesets.FirstOrDefault(ri => ri.InstantiationInfo.Equals(r.RulesetInfo.InstantiationInfo, StringComparison.Ordinal)) == null)
                    {
                        var existingSameShortName = existingRulesets.FirstOrDefault(ri => ri.ShortName == r.RulesetInfo.ShortName);

                        if (existingSameShortName != null)
                        {
                            // even if a matching InstantiationInfo was not found, there may be an existing ruleset with the same ShortName.
                            // this generally means the user or ruleset provider has renamed their dll but the underlying ruleset is *likely* the same one.
                            // in such cases, update the instantiation info of the existing entry to point to the new one.
                            existingSameShortName.InstantiationInfo = r.RulesetInfo.InstantiationInfo;
                        }
                        else
                            context.RulesetInfo.Add(r.RulesetInfo);
                    }
                }

                context.SaveChanges();

                // perform a consistency check
                foreach (var r in context.RulesetInfo)
                {
                    try
                    {
                        var resolvedType = Type.GetType(r.InstantiationInfo)
                                           ?? throw new RulesetLoadException(@"Type could not be resolved");

                        var instanceInfo = (Activator.CreateInstance(resolvedType) as Ruleset)?.RulesetInfo
                                           ?? throw new RulesetLoadException(@"Instantiation failure");

                        r.Name = instanceInfo.Name;
                        r.ShortName = instanceInfo.ShortName;
                        r.InstantiationInfo = instanceInfo.InstantiationInfo;
                        r.Available = true;
                    }
                    catch
                    {
                        r.Available = false;
                    }
                }

                context.SaveChanges();

                AvailableRulesets = context.RulesetInfo.Where(r => r.Available).ToList();
            }
        }

        private void loadFromAppDomain()
        {
            foreach (var ruleset in AppDomain.CurrentDomain.GetAssemblies())
            {
                string rulesetName = ruleset.GetName().Name;

                if (!rulesetName.StartsWith(ruleset_library_prefix, StringComparison.InvariantCultureIgnoreCase) || ruleset.GetName().Name.Contains("Tests"))
                    continue;

                addRuleset(ruleset);
            }
        }

        private void loadUserRulesets()
        {
            if (rulesetStorage == null) return;

            var rulesets = rulesetStorage.GetFiles(".", $"{ruleset_library_prefix}.*.dll");

            foreach (string ruleset in rulesets.Where(f => !f.Contains("Tests")))
                loadRulesetFromFile(rulesetStorage.GetFullPath(ruleset));
        }

        private void loadFromDisk()
        {
            try
            {
                string[] files = Directory.GetFiles(RuntimeInfo.StartupDirectory, $"{ruleset_library_prefix}.*.dll");

                foreach (string file in files.Where(f => !Path.GetFileName(f).Contains("Tests")))
                    loadRulesetFromFile(file);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Could not load rulesets from directory {RuntimeInfo.StartupDirectory}");
            }
        }

        private void loadRulesetFromFile(string file)
        {
            string filename = Path.GetFileNameWithoutExtension(file);

            if (loadedAssemblies.Values.Any(t => Path.GetFileNameWithoutExtension(t.Assembly.Location) == filename))
                return;

            try
            {
                addRuleset(Assembly.LoadFrom(file));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to load ruleset {filename}");
            }
        }

        private void addRuleset(Assembly assembly)
        {
            if (loadedAssemblies.ContainsKey(assembly))
                return;

            // the same assembly may be loaded twice in the same AppDomain (currently a thing in certain Rider versions https://youtrack.jetbrains.com/issue/RIDER-48799).
            // as a failsafe, also compare by FullName.
            if (loadedAssemblies.Any(a => a.Key.FullName == assembly.FullName))
                return;

            try
            {
                loadedAssemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to add ruleset {assembly}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= resolveRulesetDependencyAssembly;
        }

        #region Implementation of IRulesetStore

        IRulesetInfo IRulesetStore.GetRuleset(int id) => GetRuleset(id);
        IRulesetInfo IRulesetStore.GetRuleset(string shortName) => GetRuleset(shortName);
        IEnumerable<IRulesetInfo> IRulesetStore.AvailableRulesets => AvailableRulesets;

        #endregion
    }
}
