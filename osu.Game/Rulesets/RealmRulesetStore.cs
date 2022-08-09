// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Rulesets
{
    public class RealmRulesetStore : RulesetStore
    {
        public override IEnumerable<RulesetInfo> AvailableRulesets => availableRulesets;

        private readonly List<RulesetInfo> availableRulesets = new List<RulesetInfo>();

        public RealmRulesetStore(RealmAccess realm, Storage? storage = null)
            : base(storage)
        {
            prepareDetachedRulesets(realm);
        }

        private void prepareDetachedRulesets(RealmAccess realmAccess)
        {
            realmAccess.Write(realm =>
            {
                var rulesets = realm.All<RulesetInfo>();

                List<Ruleset> instances = LoadedAssemblies.Values
                                                          .Select(r => Activator.CreateInstance(r) as Ruleset)
                                                          .Where(r => r != null)
                                                          .Select(r => r.AsNonNull())
                                                          .ToList();

                // add all legacy rulesets first to ensure they have exclusive choice of primary key.
                foreach (var r in instances.Where(r => r is ILegacyRuleset))
                {
                    if (realm.All<RulesetInfo>().FirstOrDefault(rr => rr.OnlineID == r.RulesetInfo.OnlineID) == null)
                        realm.Add(new RulesetInfo(r.RulesetInfo.ShortName, r.RulesetInfo.Name, r.RulesetInfo.InstantiationInfo, r.RulesetInfo.OnlineID));
                }

                // add any other rulesets which have assemblies present but are not yet in the database.
                foreach (var r in instances.Where(r => !(r is ILegacyRuleset)))
                {
                    if (rulesets.FirstOrDefault(ri => ri.InstantiationInfo.Equals(r.RulesetInfo.InstantiationInfo, StringComparison.Ordinal)) == null)
                    {
                        var existingSameShortName = rulesets.FirstOrDefault(ri => ri.ShortName == r.RulesetInfo.ShortName);

                        if (existingSameShortName != null)
                        {
                            // even if a matching InstantiationInfo was not found, there may be an existing ruleset with the same ShortName.
                            // this generally means the user or ruleset provider has renamed their dll but the underlying ruleset is *likely* the same one.
                            // in such cases, update the instantiation info of the existing entry to point to the new one.
                            existingSameShortName.InstantiationInfo = r.RulesetInfo.InstantiationInfo;
                        }
                        else
                            realm.Add(new RulesetInfo(r.RulesetInfo.ShortName, r.RulesetInfo.Name, r.RulesetInfo.InstantiationInfo, r.RulesetInfo.OnlineID));
                    }
                }

                List<RulesetInfo> detachedRulesets = new List<RulesetInfo>();

                // perform a consistency check and detach final rulesets from realm for cross-thread runtime usage.
                foreach (var r in rulesets.OrderBy(r => r.OnlineID))
                {
                    try
                    {
                        var resolvedType = Type.GetType(r.InstantiationInfo)
                                           ?? throw new RulesetLoadException(@"Type could not be resolved");

                        var instanceInfo = (Activator.CreateInstance(resolvedType) as Ruleset)?.RulesetInfo
                                           ?? throw new RulesetLoadException(@"Instantiation failure");

                        // If a ruleset isn't up-to-date with the API, it could cause a crash at an arbitrary point of execution.
                        // To eagerly handle cases of missing implementations, enumerate all types here and mark as non-available on throw.
                        resolvedType.Assembly.GetTypes();

                        r.Name = instanceInfo.Name;
                        r.ShortName = instanceInfo.ShortName;
                        r.InstantiationInfo = instanceInfo.InstantiationInfo;
                        r.Available = true;

                        detachedRulesets.Add(r.Clone());
                    }
                    catch (Exception ex)
                    {
                        r.Available = false;
                        Logger.Log($"Could not load ruleset {r}: {ex.Message}");
                    }
                }

                availableRulesets.AddRange(detachedRulesets.OrderBy(r => r));
            });
        }
    }
}
