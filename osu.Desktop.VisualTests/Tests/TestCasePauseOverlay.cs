using System;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Game.Overlays.Pause;
using osu.Framework.Graphics.Containers;


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

            Children = new[] { pauseOverlay = new PauseOverlay() };
            pauseOverlay.ToggleVisibility();
        }
    }
}