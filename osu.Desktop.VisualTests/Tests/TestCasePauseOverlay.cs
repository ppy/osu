using System;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Overlays.Pause;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Framework.GameModes.Testing;
namespace osu.Desktop.VisualTests.Tests
{
    class TestCasePauseOverlay : TestCase
    {
        public override string Name => @"PauseOverlay";

        public override string Description => @"Tests the pause overlay";

        private PauseOverlay pauseOverlay;

        public override void Reset()
        {
            base.Reset();

            Add(new Box
            {
                ColourInfo = ColourInfo.GradientVertical(Color4.Gray, Color4.WhiteSmoke),
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
            });
            Add(pauseOverlay = new PauseOverlay());

            pauseOverlay.ToggleVisibility();
        }
    }
}
