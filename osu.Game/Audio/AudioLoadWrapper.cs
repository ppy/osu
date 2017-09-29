// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Audio
{
    public class AudioLoadWrapper : Drawable
    {
        private readonly string preview;

        public Track Preview;

        public AudioLoadWrapper(string preview)
        {
            this.preview = preview;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            if (!string.IsNullOrEmpty(preview))
            {
                Preview = audio.Track.Get(preview);
                Preview.Volume.Value = 0.5;
            }
        }
    }
}
