// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Logging;
using osu.Game.Database;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// Todo: All of this needs to be moved to a RulesetStore.
    /// </summary>
    public class RulesetStore : DatabaseBackedStore
    {
        private static readonly Dictionary<Assembly, Type> loaded_assemblies = new Dictionary<Assembly, Type>();

        static RulesetStore()
        {
            AppDomain.CurrentDomain.AssemblyResolve += currentDomain_AssemblyResolve;

            // On android in release configuration assemblies are loaded from the apk directly into memory.
            // We cannot read assemblies from cwd, so should check loaded assemblies instead.
            loadFromAppDomain();

            loadFromDisk();
        }

        public RulesetStore(IDatabaseContextFactory factory)
            : base(factory)
        {
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

        private static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => loaded_assemblies.Keys.FirstOrDefault(a => a.FullName == args.Name);

        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        private void addMissingRulesets()
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                var instances = loaded_assemblies.Values.Select(r => (Ruleset)Activator.CreateInstance(r, (RulesetInfo)null)).ToList();

                //add all legacy modes in correct order
                foreach (var r in instances.Where(r => r.LegacyID != null).OrderBy(r => r.LegacyID))
                {
                    if (context.RulesetInfo.SingleOrDefault(rsi => rsi.ID == r.RulesetInfo.ID) == null)
                        context.RulesetInfo.Add(r.RulesetInfo);
                }

                context.SaveChanges();

                //add any other modes
                foreach (var r in instances.Where(r => r.LegacyID == null))
                    if (context.RulesetInfo.FirstOrDefault(ri => ri.InstantiationInfo == r.RulesetInfo.InstantiationInfo) == null)
                        context.RulesetInfo.Add(r.RulesetInfo);

                context.SaveChanges();

                //perform a consistency check
                foreach (var r in context.RulesetInfo)
                {
                    try
                    {
                        var instanceInfo = ((Ruleset)Activator.CreateInstance(Type.GetType(r.InstantiationInfo, asm =>
                        {
                            // for the time being, let's ignore the version being loaded.
                            // this allows for debug builds to successfully load rulesets (even though debug rulesets have a 0.0.0 version).
                            asm.Version = null;
                            return Assembly.Load(asm);
                        }, null), (RulesetInfo)null)).RulesetInfo;

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

        private static void loadFromAppDomain()
        {
            foreach (var ruleset in AppDomain.CurrentDomain.GetAssemblies())
            {
                string rulesetName = ruleset.GetName().Name;

                if (!rulesetName.StartsWith(ruleset_library_prefix, StringComparison.InvariantCultureIgnoreCase) || ruleset.GetName().Name.Contains("Tests"))
                    continue;

                addRuleset(ruleset);
            }
        }

        private static void loadFromDisk()
        {
            try
            {
                string[] files = Directory.GetFiles(Environment.CurrentDirectory, $"{ruleset_library_prefix}.*.dll");

                foreach (string file in files.Where(f => !Path.GetFileName(f).Contains("Tests")))
                    loadRulesetFromFile(file);
            }
            catch
            {
                Logger.Log($"Could not load rulesets from directory {Environment.CurrentDirectory}");
            }
        }

        private static void loadRulesetFromFile(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (loaded_assemblies.Values.Any(t => t.Namespace == filename))
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

        private static void addRuleset(Assembly assembly)
        {
            if (loaded_assemblies.ContainsKey(assembly))
                return;

            try
            {
                loaded_assemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to add ruleset {assembly}");
            }
        }
    }
}
