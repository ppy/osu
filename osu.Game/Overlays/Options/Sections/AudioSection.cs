﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.Sections.Audio;

namespace osu.Game.Overlays.Options.Sections
{
    public class AudioSection : OptionsSection
    {
        public override string Header => "Audio";
        public override FontAwesome Icon => FontAwesome.fa_headphones;

        public AudioSection()
        {
            Children = new Drawable[]
            {
                new AudioDevicesOptions(),
                new VolumeOptions(),
                new OffsetOptions(),
                new MainMenuOptions(),
            };
        }
    }
}