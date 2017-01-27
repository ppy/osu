using System;
using OpenTK.Graphics;
using osu.Framework.Logging;
using osu.Framework.Graphics;
using osu.Game.Overlays.Pause;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.UserInterface;

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

            Add(new Button
            {
                Text = @"Pause",
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Width = 100,
                Height = 50,
                Colour = Color4.Black,
                Action = (() => pauseOverlay.Pause())
            });

            pauseOverlay.OnPause += (() => Logger.Log(@"Pause"));
            pauseOverlay.OnResume += (() => Logger.Log(@"Resume"));
            pauseOverlay.OnRetry += (() => Logger.Log(@"Retry"));
            pauseOverlay.OnQuit += (() => Logger.Log(@"Quit"));
        }
    }
}
