// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
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
                        var resolvedType = Type.GetType(r.InstantiationInfo);

                        if (resolvedType == null)
                        {
                            // ruleset DLL was probably deleted.
                            r.Available = false;
                            continue;
                        }

                        var instance = (Activator.CreateInstance(resolvedType) as Ruleset);
                        var instanceInfo = instance?.RulesetInfo
                                           ?? throw new RulesetLoadException(@"Instantiation failure");

                        if (!checkRulesetUpToDate(instance))
                        {
                            throw new ArgumentOutOfRangeException(nameof(instance.RulesetAPIVersionSupported),
                                $"Ruleset API version is too old (was {instance.RulesetAPIVersionSupported}, expected {Ruleset.CURRENT_RULESET_API_VERSION})");
                        }

                        // If a ruleset isn't up-to-date with the API, it could cause a crash at an arbitrary point of execution.
                        // To eagerly handle cases of missing implementations, enumerate all types here and mark as non-available on throw.
                        resolvedType.Assembly.GetTypes();

                        r.Name = instanceInfo.Name;
                        r.ShortName = instanceInfo.ShortName;
                        r.InstantiationInfo = instanceInfo.InstantiationInfo;
                        r.Available = true;

                        testRulesetCompatibility(r);

                        detachedRulesets.Add(r.Clone());
                    }
                    catch (Exception ex)
                    {
                        r.Available = false;
                        LogFailedLoad(r.Name, ex);
                    }
                }

                availableRulesets.AddRange(detachedRulesets.Order());
            });
        }

        private bool checkRulesetUpToDate(Ruleset instance)
        {
            switch (instance.RulesetAPIVersionSupported)
            {
                // The default `virtual` implementation leaves the version string empty.
                // Consider rulesets which haven't override the version as up-to-date for now.
                // At some point (once ruleset devs add versioning), we'll probably want to disallow this for deployed builds.
                case @"":
                // Ruleset is up-to-date, all good.
                case Ruleset.CURRENT_RULESET_API_VERSION:
                    return true;

                default:
                    return false;
            }
        }

        private void testRulesetCompatibility(RulesetInfo rulesetInfo)
        {
            // do various operations to ensure that we are in a good state.
            // if we can avoid loading the ruleset at this point (rather than erroring later in runtime) then that is preferred.
            var instance = rulesetInfo.CreateInstance();

            instance.CreateAllMods();
            instance.CreateIcon();
            instance.CreateResourceStore();

            var beatmap = new Beatmap();
            var converter = instance.CreateBeatmapConverter(beatmap);

            instance.CreateBeatmapProcessor(converter.Convert());
        }
    }
}
