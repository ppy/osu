// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays.Volume;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseVolumePieces : TestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(VolumeMeter), typeof(MuteButton) };

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            VolumeMeter meter;
            Add(meter = new VolumeMeter("MASTER", 125, Color4.Blue));
            Add(new MuteButton
            {
                Margin = new MarginPadding { Top = 200 }
            });

            meter.Bindable.BindTo(audio.Volume);

        }
    }
}
