// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Development;
using osu.Game.Database;
using SQLite.Net;

namespace osu.Game.Rulesets
{
    /// <summary>
    /// Todo: All of this needs to be moved to a RulesetStore.
    /// </summary>
    public class RulesetStore : DatabaseBackedStore
    {
        private readonly List<Ruleset> instances = new List<Ruleset>();

        public IEnumerable<RulesetInfo> AllRulesets => Query<RulesetInfo>().Where(r => r.Available);

        public RulesetStore(SQLiteConnection connection) : base(connection)
        {
        }

        private const string ruleset_library_prefix = "osu.Game.Rulesets";

        protected override void Prepare(bool reset = false)
        {
            instances.Clear();

            Connection.CreateTable<RulesetInfo>();

            if (reset)
            {
                Connection.DeleteAll<RulesetInfo>();
            }

            // todo: don't do this on deploy
            var sln = DebugUtils.GetSolutionPath();

            if (sln != null)
            {
                foreach (string dir in Directory.GetDirectories(sln, $"{ruleset_library_prefix}.*"))
                    foreach (string file in Directory.GetFiles(Path.Combine(dir, "bin", DebugUtils.IsDebug ? "Debug" : "Release"), $"{ruleset_library_prefix}.*.dll"))
                        loadRulesetFromFile(file);
            }

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"{ruleset_library_prefix}.*.dll"))
                loadRulesetFromFile(file);

            Connection.BeginTransaction();

            //add all legacy modes in correct order
            foreach (var r in instances.Where(r => r.LegacyID >= 0).OrderBy(r => r.LegacyID))
            {
                Connection.InsertOrReplace(createRulesetInfo(r));
            }

            //add any other modes
            foreach (var r in instances.Where(r => r.LegacyID < 0))
            {
                var us = createRulesetInfo(r);

                var existing = Query<RulesetInfo>().Where(ri => ri.InstantiationInfo == us.InstantiationInfo).FirstOrDefault();

                if (existing == null)
                    Connection.Insert(us);
            }

            //perform a consistency check
            foreach (var r in Query<RulesetInfo>())
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

                Connection.Update(r);
            }

            Connection.Commit();
        }

        private void loadRulesetFromFile(string file)
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (instances.Any(i => i.GetType().Namespace == filename))
                return;

            try
            {
                var assembly = Assembly.LoadFile(file);
                var rulesets = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Ruleset)));

                if (rulesets.Count() != 1)
                    return;

                instances.Add((Ruleset)Activator.CreateInstance(rulesets.First(), new RulesetInfo()));
            }
            catch (Exception) { }
        }

        private RulesetInfo createRulesetInfo(Ruleset ruleset) => new RulesetInfo
        {
            Name = ruleset.Description,
            InstantiationInfo = ruleset.GetType().AssemblyQualifiedName,
            ID = ruleset.LegacyID
        };

        protected override Type[] ValidTypes => new[] { typeof(RulesetInfo) };

        public RulesetInfo GetRuleset(int id) => Query<RulesetInfo>().First(r => r.ID == id);
    }
}
