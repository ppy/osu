﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.Database;

namespace osu.Game.Configuration
{
    [Table("Settings")]
    public class DatabasedSetting : IHasPrimaryKey
    {
        public int ID { get; set; }

        public int? RulesetID { get; set; }

        public int? Variant { get; set; }

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

        public object Key;
        public object Value;

        public DatabasedSetting(object key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Constructor for derived classes that may require serialisation.
        /// </summary>
        public DatabasedSetting()
        {
        }

        public override string ToString() => $"{Key}=>{Value}";
    }
}
