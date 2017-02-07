// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Sprites;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Colour;

namespace osu.Desktop.VisualTests
{
    public class TestCaseSongProgressBar : TestCase
    {
        public override string Name => @"SongProgressBar";

        public override string Description => @"Tests the song progress bar";

        public override void Reset()
        {
            base.Reset();

            Add(new Box
            {
                ColourInfo = ColourInfo.GradientVertical(Color4.WhiteSmoke, Color4.Gray),
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
            });
            Add(new SongProgressBar
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X
            });
        }
    }
}
