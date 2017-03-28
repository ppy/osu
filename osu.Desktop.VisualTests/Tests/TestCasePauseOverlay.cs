// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens.Testing;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCasePauseOverlay : TestCase
    {
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
            });

            pauseOverlay.AddButton(@"Continue", Color4.Green, delegate { Logger.Log(@"Resume"); });
            pauseOverlay.AddButton(@"Retry", Color4.Yellow, delegate { Logger.Log(@"Retry"); });
            pauseOverlay.AddButton(@"Quit to Main Menu", new Color4(170, 27, 39, 255), delegate { Logger.Log(@"Quit"); });

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
