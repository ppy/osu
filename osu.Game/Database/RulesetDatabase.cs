// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Platform;
using osu.Game.Rulesets;
using SQLite.Net;

namespace osu.Game.Database
{
    /// <summary>
    /// Todo: All of this needs to be moved to a RulesetDatabase.
    /// </summary>
    public class RulesetDatabase : Database
    {
        public IEnumerable<RulesetInfo> AllRulesets => Query<RulesetInfo>().Where(r => r.Available);

        public RulesetDatabase(Storage storage, SQLiteConnection connection)
            : base(storage, connection)
        {
        }

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<RulesetInfo>();

            if (reset)
            {
                Connection.DeleteAll<RulesetInfo>();
            }

            List<Ruleset> instances = new List<Ruleset>();

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, @"osu.Game.Rulesets.*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var rulesets = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Ruleset)));

                    if (rulesets.Count() != 1)
                        continue;

                    foreach (Type rulesetType in rulesets)
                        instances.Add((Ruleset)Activator.CreateInstance(rulesetType));
                }
                catch (Exception) { }
            }

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

                var existing = Query<RulesetInfo>().FirstOrDefault(ri => ri.InstantiationInfo == us.InstantiationInfo);

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
