// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;
using Realms;

#nullable enable

namespace osu.Game.Configuration
{
    [MapTo(@"RulesetSetting")]
    public class RealmRulesetSetting : RealmObject, IHasGuidPrimaryKey
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Indexed]
        public int RulesetID { get; set; }

        [Indexed]
        public int Variant { get; set; }

        public string Key { get; set; } = string.Empty;

        [MapTo(nameof(Value))]
        public string ValueString { get; set; } = string.Empty;

        public object Value
        {
            get => ValueString;
            set => ValueString = value.ToString();
        }

        public override string ToString() => $"{Key}=>{Value}";
    }
}
