// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;

namespace osu.Game.Input.Bindings
{
    [Table("KeyBinding")]
    public class DatabasedKeyBinding : KeyBinding
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(RulesetInfo))]
        public int? RulesetInfoId { get; set; }
        public RulesetInfo RulesetInfo;

        public int? Variant { get; set; }

        [Column("Keys")]
        public string KeysString
        {
            get { return KeyCombination.ToString(); }
            private set { KeyCombination = value; }
        }

        [Column("Action")]
        public int IntAction
        {
            get { return (int)Action; }
            set { Action = value; }
        }
    }
}
