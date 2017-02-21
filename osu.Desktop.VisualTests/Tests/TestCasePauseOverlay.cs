// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private int retryCount;

        public override void Reset()
        {
            base.Reset();

            Add(pauseOverlay = new PauseOverlay
            {
                Depth = -1,
                OnResume = () => Logger.Log(@"Resume"),
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit")
            });
            AddButton("Pause", pauseOverlay.Show);
            AddButton("Add Retry", delegate
            {
                retryCount++;
                pauseOverlay.Retries = retryCount;
            });

            retryCount = 0;
        }
    }
}
