// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Realms;

namespace osu.Game.Database
{
    public interface IHasGuidPrimaryKey
    {
        [JsonIgnore]
        [Ignored]
        public Guid Guid
        {
            get => new Guid(ID);
            set => ID = value.ToString();
        }

        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        string ID { get; set; }
    }
}
