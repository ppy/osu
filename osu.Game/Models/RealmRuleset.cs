// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    [MapTo("Ruleset")]
    public class RealmRuleset : RealmObject, IEquatable<RealmRuleset>, IRulesetInfo
    {
        [PrimaryKey]
        public string ShortName { get; set; } = string.Empty;

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public string Name { get; set; } = string.Empty;

        public string InstantiationInfo { get; set; } = string.Empty;

        public RealmRuleset(string shortName, string name, string instantiationInfo, int onlineID)
        {
            ShortName = shortName;
            Name = name;
            InstantiationInfo = instantiationInfo;
            OnlineID = onlineID;
        }

        [UsedImplicitly]
        private RealmRuleset()
        {
        }

        public RealmRuleset(int? onlineID, string name, string shortName, bool available)
        {
            OnlineID = onlineID ?? -1;
            Name = name;
            ShortName = shortName;
            Available = available;
        }

        public bool Available { get; set; }

        public bool Equals(RealmRuleset? other) => other != null && OnlineID == other.OnlineID && Available == other.Available && Name == other.Name && InstantiationInfo == other.InstantiationInfo;

        public bool Equals(IRulesetInfo? other) => other is RealmRuleset b && Equals(b);

        public override string ToString() => Name;

        public RealmRuleset Clone() => new RealmRuleset
        {
            OnlineID = OnlineID,
            Name = Name,
            ShortName = ShortName,
            InstantiationInfo = InstantiationInfo,
            Available = Available
        };

        public Ruleset CreateInstance()
        {
            if (!Available)
                throw new RulesetLoadException(@"Ruleset not available");

            var type = Type.GetType(InstantiationInfo);

            if (type == null)
                throw new RulesetLoadException(@"Type lookup failure");

            var ruleset = Activator.CreateInstance(type) as Ruleset;

            if (ruleset == null)
                throw new RulesetLoadException(@"Instantiation failure");

            // overwrite the pre-populated RulesetInfo with a potentially database attached copy.
            // TODO: figure if we still want/need this after switching to realm.
            // ruleset.RulesetInfo = this;

            return ruleset;
        }
    }
}
