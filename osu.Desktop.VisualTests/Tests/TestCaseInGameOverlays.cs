// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseInGameOverlays : TestCase
    {
        public override string Description => @"Tests pause and fail overlays";

        private PauseOverlay pauseOverlay;
        private FailOverlay failOverlay;
        private int retryCount;

        public override void Reset()
        {
            base.Reset();

            pauseOverlay = new PauseOverlay
            {
                Depth = -1,
                OnResume = () => Logger.Log(@"Resume"),
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            };

            failOverlay = new FailOverlay
            {
                Depth = -1,
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            };

            Add(pauseOverlay);
            Add(failOverlay);

            AddStep(@"Pause", delegate {
                if(failOverlay.State == Visibility.Visible)
                {
                    failOverlay.Hide();
                }
                pauseOverlay.Show();
            });
            AddStep("Fail", delegate {
                if (pauseOverlay.State == Visibility.Visible)
                {
                    pauseOverlay.Hide();
                }
                failOverlay.Show();
            });
            AddStep("Add Retry", delegate
            {
                retryCount++;
                pauseOverlay.Retries = retryCount;
                failOverlay.Retries = retryCount;
            });

            retryCount = 0;
        }
    }
}
