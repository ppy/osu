// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using SQLite.Net.Attributes;

namespace osu.Game.Database
{
    public class StoreVersion
    {
        [PrimaryKey]
        public string StoreName { get; set; }

        public int Version { get; set; }
    }
}
