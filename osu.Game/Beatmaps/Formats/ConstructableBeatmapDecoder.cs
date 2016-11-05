//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Beatmaps.Formats
{
    public class ConstructableBeatmapDecoder : BeatmapDecoder
    {
        protected override Beatmap ParseFile(TextReader stream)
        {
            throw new NotImplementedException();
        }
    }
}
