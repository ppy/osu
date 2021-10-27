// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;

namespace osu.Game.Rulesets
{
    [ExcludeFromDynamicCompile]
    public class RulesetInfo : IEquatable<RulesetInfo>, IRulesetInfo
    {
        public int? ID { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string InstantiationInfo { get; set; }

        [JsonIgnore]
        public bool Available { get; set; }

        // TODO: this should probably be moved to RulesetStore.
        public virtual Ruleset CreateInstance()
        {
            if (!Available) return null;

            var ruleset = (Ruleset)Activator.CreateInstance(Type.GetType(InstantiationInfo).AsNonNull());

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
                int hashCode = ID.HasValue ? ID.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (InstantiationInfo != null ? InstantiationInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Available.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => Name ?? $"{Name} ({ShortName}) ID: {ID}";

        #region Implementation of IHasOnlineID

        public int OnlineID => ID ?? -1;

        #endregion
    }
}
