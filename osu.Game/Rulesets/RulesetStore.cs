﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"{ruleset_library_prefix}.*.dll"))
                loadRulesetFromFile(file);
        }

        public RulesetStore(Func<OsuDbContext> factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Retrieve a ruleset using a known ID.
        /// </summary>
        /// <param name="id">The ruleset's internal ID.</param>
        /// <returns>A ruleset, if available, else null.</returns>
        public RulesetInfo GetRuleset(int id) => AvailableRulesets.FirstOrDefault(r => r.ID == id);

        /// <summary>
        /// All available rulesets.
        /// </summary>
        public IEnumerable<RulesetInfo> AvailableRulesets => GetContext().RulesetInfo.Where(r => r.Available);

        private static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => loaded_assemblies.Keys.FirstOrDefault(a => a.FullName == args.Name);

        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        protected override void Prepare(bool reset = false)
        {
            var context = GetContext();

            if (reset)
            {
                context.Database.ExecuteSqlCommand("DELETE FROM RulesetInfo");
            }

            var instances = loaded_assemblies.Values.Select(r => (Ruleset)Activator.CreateInstance(r, new RulesetInfo())).ToList();

            //add all legacy modes in correct order
            foreach (var r in instances.Where(r => r.LegacyID >= 0).OrderBy(r => r.LegacyID))
            {
                var rulesetInfo = createRulesetInfo(r);
                if (context.RulesetInfo.SingleOrDefault(rsi => rsi.ID == rulesetInfo.ID) == null)
                {
                    context.RulesetInfo.Add(rulesetInfo);
                }
            }

            context.SaveChanges();

            //add any other modes
            foreach (var r in instances.Where(r => r.LegacyID < 0))
            {
                var us = createRulesetInfo(r);

                var existing = context.RulesetInfo.FirstOrDefault(ri => ri.InstantiationInfo == us.InstantiationInfo);

                if (existing == null)
                    context.RulesetInfo.Add(us);
            }

            context.SaveChanges();

            //perform a consistency check
            foreach (var r in context.RulesetInfo)
            {
                try
                {
                    r.CreateInstance();
                    r.Available = true;
                }
                catch
                {
                    r.Available = false;
                }
            }

            context.SaveChanges();
        }

        private static void loadRulesetFromFile(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (loaded_assemblies.Values.Any(t => t.Namespace == filename))
                return;

            try
            {
                var assembly = Assembly.LoadFrom(file);
                loaded_assemblies[assembly] = assembly.GetTypes().First(t => t.IsSubclassOf(typeof(Ruleset)));
            }
            catch (Exception)
            {
            }
        }

        private RulesetInfo createRulesetInfo(Ruleset ruleset) => new RulesetInfo
        {
            Name = ruleset.Description,
            InstantiationInfo = ruleset.GetType().AssemblyQualifiedName,
            ID = ruleset.LegacyID
        };
    }
}
