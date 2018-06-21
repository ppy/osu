// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Overlays.Volume;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseVolumePieces : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(VolumeMeter), typeof(MuteButton) };

        protected override void LoadComplete()
        {
            VolumeMeter meter;
            MuteButton mute;
            Add(meter = new VolumeMeter("MASTER", 125, Color4.Blue) { Position = new Vector2(10) });
            AddSliderStep("master volume", 0, 10, 0, i => meter.Bindable.Value = i * 0.1);

            Add(new VolumeMeter("BIG", 250, Color4.Red)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(10),
            });

            Add(mute = new MuteButton
            {
                Margin = new MarginPadding { Top = 200 }
            });

            AddToggleStep("mute", b => mute.Current.Value = b);
        }
    }
}
