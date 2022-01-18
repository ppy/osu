// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using osu.Framework.Testing;

namespace osu.Game.Rulesets
{
    [ExcludeFromDynamicCompile]
    [Table(@"RulesetInfo")]
    public sealed class EFRulesetInfo : IEquatable<EFRulesetInfo>, IRulesetInfo
    {
        public int? ID { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string InstantiationInfo { get; set; }

        [JsonIgnore]
        public bool Available { get; set; }

        // TODO: this should probably be moved to RulesetStore.
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
            // ruleset.RulesetInfo = this;

            return ruleset;
        }

        public bool Equals(EFRulesetInfo other) => other != null && ID == other.ID && Available == other.Available && Name == other.Name && InstantiationInfo == other.InstantiationInfo;

        public override bool Equals(object obj) => obj is EFRulesetInfo rulesetInfo && Equals(rulesetInfo);

        public bool Equals(IRulesetInfo other) => other is RulesetInfo b && Equals(b);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ID.HasValue ? ID.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (InstantiationInfo != null ? InstantiationInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Available.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => Name ?? $"{Name} ({ShortName}) ID: {ID}";

        #region Implementation of IHasOnlineID

        [NotMapped]
        public int OnlineID
        {
            get => ID ?? -1;
            set => ID = value >= 0 ? value : (int?)null;
        }

        #endregion
    }
}
