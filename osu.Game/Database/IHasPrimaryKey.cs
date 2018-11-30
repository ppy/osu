// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace osu.Game.Database
{
    public interface IHasPrimaryKey
    {
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        int ID { get; set; }
    }
}
