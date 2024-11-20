// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Realms;

namespace osu.Game.Configuration
{
    [MapTo(@"RulesetSetting")]
    public partial class RealmRulesetSetting : IRealmObject
    {
        [Indexed]
        public string RulesetName { get; set; } = string.Empty;

        [Indexed]
        public int Variant { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public override string ToString() => $"{Key} => {Value}";
    }
}
