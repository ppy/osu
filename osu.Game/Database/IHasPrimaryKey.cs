// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace osu.Game.Database
{
    public interface IHasPrimaryKey
    {
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        int ID { get; set; }

        bool IsManaged { get; }
    }
}
