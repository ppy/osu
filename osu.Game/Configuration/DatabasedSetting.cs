// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.Database;

namespace osu.Game.Configuration
{
    [Table("Settings")]
    public class DatabasedSetting : Setting, IHasPrimaryKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? RulesetID { get; set; }

        [Column("Key")]
        public int IntKey
        {
            get => (int)Key;
            private set => Key = value;
        }

        [Column("Value")]
        public string StringValue
        {
            get => Value.ToString();
            set => Value = value;
        }
    }
}
