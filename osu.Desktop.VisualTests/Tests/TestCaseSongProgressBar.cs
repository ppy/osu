using System;
using osu.Framework.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests
{
    public class TestCaseSongProgressBar : TestCase
    {
        public override string Name => @"SongProgressBar";

        public override string Description => @"Tests the song progress bar";

        public override void Reset()
        {
            base.Reset();

            Add(new SongProgressBar
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X
            });
        }
    }
}
