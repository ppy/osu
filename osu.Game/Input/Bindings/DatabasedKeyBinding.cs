// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
using osu.Framework.Input.Bindings;
using osu.Game.Database;

namespace osu.Game.Input.Bindings
{
    [Table("KeyBinding")]
    public class DatabasedKeyBinding : KeyBinding, IHasPrimaryKey
    {
        public int ID { get; set; }

        public int? RulesetID { get; set; }

        public int? Variant { get; set; }

        [Column("Keys")]
        public string KeysString
        {
            get => KeyCombination.ToString();
            private set => KeyCombination = value;
        }

        [Column("Action")]
        public int IntAction
        {
            get => (int)Action;
            set => Action = value;
        }
    }
}
