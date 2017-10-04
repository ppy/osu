// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        public IEnumerable<RulesetInfo> AllRulesets => Connection.RulesetInfo.Where(r => r.Available);

        public RulesetStore(OsuDbContext connection) : base(connection)
        {
        }

        static RulesetStore()
        {
            AppDomain.CurrentDomain.AssemblyResolve += currentDomain_AssemblyResolve;

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"{ruleset_library_prefix}.*.dll"))
                loadRulesetFromFile(file);
        }

        private static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => loaded_assemblies.Keys.FirstOrDefault(a => a.FullName == args.Name);

        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        protected override void Prepare(bool reset = false)
        {
            if (reset)
            {
                Connection.Database.ExecuteSqlCommand("DELETE FROM RulesetInfo");
            }
            
            var instances = loaded_assemblies.Values.Select(r => (Ruleset)Activator.CreateInstance(r, new RulesetInfo()));

            //add all legacy modes in correct order
            foreach (var r in instances.Where(r => r.LegacyID >= 0).OrderBy(r => r.LegacyID))
            {
                var rulesetInfo = createRulesetInfo(r);
                if (Connection.RulesetInfo.SingleOrDefault(rsi=>rsi.Id==rulesetInfo.Id)==null)
                {
                    Connection.RulesetInfo.Add(rulesetInfo);
                }
            }

            //add any other modes
            foreach (var r in instances.Where(r => r.LegacyID < 0))
            {
                var us = createRulesetInfo(r);

                var existing = Connection.RulesetInfo.Where(ri => ri.InstantiationInfo == us.InstantiationInfo).FirstOrDefault();

                if (existing == null)
                    Connection.RulesetInfo.Add(us);
            }

            //perform a consistency check
            foreach (var r in Connection.RulesetInfo)
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

                //Connection.RulesetInfo.Update(r);
            }
            Connection.SaveChanges();
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
            catch (Exception) { }
        }

        private RulesetInfo createRulesetInfo(Ruleset ruleset) => new RulesetInfo
        {
            Name = ruleset.Description,
            InstantiationInfo = ruleset.GetType().AssemblyQualifiedName,
            Id = ruleset.LegacyID
        };

        protected override Type[] ValidTypes => new[] { typeof(RulesetInfo) };

        public RulesetInfo GetRuleset(int id) => Connection.RulesetInfo.First(r => r.Id == id);

        public RulesetInfo QueryRulesetInfo(Func<RulesetInfo, bool> query)
        {
            return Connection.RulesetInfo.FirstOrDefault(query);
        }

        public List<RulesetInfo> QueryRulesets(Func<RulesetInfo, bool> query = null)
        {
            var rulesets = Connection.RulesetInfo;
            if (query != null)
                return rulesets.Where(query).ToList();
            return rulesets.ToList();
        }
    }
}
