// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace osu.Game.Rulesets
{
    public class RulesetInfo : IEquatable<RulesetInfo>
    {
        public int? ID { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        private string instantiationInfo;

        public string InstantiationInfo
        {
            get => instantiationInfo;
            set => instantiationInfo = abbreviateInstantiationInfo(value);
        }

        private string abbreviateInstantiationInfo(string value)
        {
            // exclude version onwards, matching only on namespace and type.
            // this is mainly to allow for new versions of already loaded rulesets to "upgrade" from old.
            return string.Join(',', value.Split(',').Take(2));
        }

        [JsonIgnore]
        public bool Available { get; set; }

        // TODO: this should probably be moved to RulesetStore.
        public virtual Ruleset CreateInstance()
        {
            if (!Available) return null;

            var ruleset = (Ruleset)Activator.CreateInstance(Type.GetType(InstantiationInfo));

            // overwrite the pre-populated RulesetInfo with a potentially database attached copy.
            ruleset.RulesetInfo = this;

            return ruleset;
        }

        public bool Equals(RulesetInfo other) => other != null && ID == other.ID && Available == other.Available && Name == other.Name && InstantiationInfo == other.InstantiationInfo;

        public override bool Equals(object obj) => obj is RulesetInfo rulesetInfo && Equals(rulesetInfo);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ID.HasValue ? ID.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (InstantiationInfo != null ? InstantiationInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Available.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => Name ?? $"{Name} ({ShortName}) ID: {ID}";
    }
}
