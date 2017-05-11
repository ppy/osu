// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Audio
{
    public class SampleInfoList : List<SampleInfo>
    {
        public SampleInfoList()
        {
        }

        public SampleInfoList(IEnumerable<SampleInfo> elements) : base(elements)
        {
        }
    }
}