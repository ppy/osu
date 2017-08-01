// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using SQLite.Net.Attributes;

namespace osu.Game.IO
{
    public class FileInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed(Unique = true)]
        public string Hash { get; set; }

        public string StoragePath => Path.Combine(Hash.Remove(1), Hash.Remove(2), Hash);

        [Indexed]
        public int ReferenceCount { get; set; }
    }
}
