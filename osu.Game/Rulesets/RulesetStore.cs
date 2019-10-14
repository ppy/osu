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
        private static readonly Lazy<Dictionary<Assembly, Type>> loaded_assemblies = new Lazy<Dictionary<Assembly, Type>>(() => loadRulesets());

        static RulesetStore()
        {
            AppDomain.CurrentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
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

        private static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => loaded_assemblies.Value.Keys.FirstOrDefault(a => a.FullName == args.Name);

        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        private void addMissingRulesets()
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                var instances = loaded_assemblies.Value.Values.Select(r => (Ruleset)Activator.CreateInstance(r, (RulesetInfo)null)).ToList();

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

        /// <summary>
        /// Loads the rulesets that are in the current appdomain and in the current directory.
        /// </summary>
        /// <returns>The rulesets that were loaded.</returns>
        private static Dictionary<Assembly, Type> loadRulesets()
        {
            var rulesets = new Dictionary<Assembly, Type>();

            foreach (var rulesetAssembly in getRulesetAssemblies())
            {
                try
                {
                    rulesets[rulesetAssembly] = rulesetAssembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to add ruleset {rulesetAssembly}");
                }
            }

            return rulesets;
        }

        /// <summary>
        /// Scans the current appdomain and current directory for ruleset assemblies.
        /// Rulesets that were found in the current directory are automatically loaded.
        /// </summary>
        /// <returns>The ruleset assemblies that were found in the current appdomain or in the current directory.</returns>
        private static IEnumerable<Assembly> getRulesetAssemblies()
        {
            var rulesetAssemblies = new HashSet<Assembly>();

            // load from appdomain
            foreach (var ruleset in AppDomain.CurrentDomain.GetAssemblies())
            {
                string rulesetName = ruleset.GetName().Name;

                if (!rulesetName.StartsWith(ruleset_library_prefix, StringComparison.InvariantCultureIgnoreCase) || ruleset.GetName().Name.Contains("Tests"))
                    continue;

                rulesetAssemblies.Add(ruleset);
            }

            // load from current directory
            try
            {
                string[] files = Directory.GetFiles(Environment.CurrentDirectory, $"{ruleset_library_prefix}.*.dll");

                foreach (string file in files.Where(f => !Path.GetFileName(f).Contains("Tests")))
                {
                    try
                    {
                        rulesetAssemblies.Add(Assembly.LoadFrom(file));
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Failed to load ruleset assembly {Path.GetFileNameWithoutExtension(file)}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Could not load rulesets from directory {Environment.CurrentDirectory}");
            }

            return rulesetAssemblies;
        }
    }
}
