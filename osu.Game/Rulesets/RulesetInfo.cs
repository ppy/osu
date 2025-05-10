// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Game.Rulesets.Difficulty;
using Realms;

namespace osu.Game.Rulesets
{
    [MapTo("Ruleset")]
    public class RulesetInfo : RealmObject, IEquatable<RulesetInfo>, IComparable<RulesetInfo>, IRulesetInfo
    {
        [PrimaryKey]
        public string ShortName { get; set; } = string.Empty;

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public string Name { get; set; } = string.Empty;

        public string InstantiationInfo { get; set; } = string.Empty;

        /// <summary>
        /// Stores the last applied <see cref="DifficultyCalculator.Version"/>
        /// </summary>
        public int LastAppliedDifficultyVersion { get; set; }

        public RulesetInfo(string shortName, string name, string instantiationInfo, int onlineID)
        {
            ShortName = shortName;
            Name = name;
            InstantiationInfo = instantiationInfo;
            OnlineID = onlineID;
        }

        [UsedImplicitly]
        public RulesetInfo()
        {
        }

        public bool Available { get; set; }

        public bool Equals(RulesetInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ShortName == other.ShortName;
        }

        public bool Equals(IRulesetInfo? other) => other is RulesetInfo r && Equals(r);

        public int CompareTo(RulesetInfo? other)
        {
            if (OnlineID >= 0 && other?.OnlineID >= 0)
                return OnlineID.CompareTo(other.OnlineID);

            // Official rulesets are always given precedence for the time being.
            if (OnlineID >= 0)
                return -1;
            if (other?.OnlineID >= 0)
                return 1;

            return string.Compare(ShortName, other?.ShortName, StringComparison.Ordinal);
        }

        public int CompareTo(IRulesetInfo? other)
        {
            if (!(other is RulesetInfo ruleset))
                throw new ArgumentException($@"Object is not of type {nameof(RulesetInfo)}.", nameof(other));

            return CompareTo(ruleset);
        }

        public override int GetHashCode()
        {
            // Importantly, ignore the underlying realm hash code, as it will usually not match.
            var hashCode = new HashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hashCode.Add(ShortName);
            return hashCode.ToHashCode();
        }

        public override string ToString() => Name;

        public RulesetInfo Clone() => new RulesetInfo
        {
            OnlineID = OnlineID,
            Name = Name,
            ShortName = ShortName,
            InstantiationInfo = InstantiationInfo,
            Available = Available,
            LastAppliedDifficultyVersion = LastAppliedDifficultyVersion,
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
