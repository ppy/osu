// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays.Volume;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseVolumePieces : TestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(VolumeMeter), typeof(MuteButton) };

        protected override void LoadComplete()
        {
            VolumeMeter meter;
            MuteButton mute;
            LoadComponentAsync(meter = new VolumeMeter("MASTER", 125, Color4.Blue), Add);
            LoadComponentAsync(mute = new MuteButton
            {
                Margin = new MarginPadding { Top = 200 }
            }, Add);

            AddSliderStep("master volume", 0, 10, 0, i => meter.Bindable.Value = i * 0.1);
            AddToggleStep("mute", b => mute.Current.Value = b);
        }
    }
}
