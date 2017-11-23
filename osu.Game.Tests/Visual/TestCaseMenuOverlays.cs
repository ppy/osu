// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("player pause/fail screens")]
    internal class TestCaseMenuOverlays : OsuTestCase
    {
        public TestCaseMenuOverlays()
        {
            FailOverlay failOverlay;
            PauseContainer.PauseOverlay pauseOverlay;

            var retryCount = 0;

            Add(pauseOverlay = new PauseContainer.PauseOverlay
            {
                OnResume = () => Logger.Log(@"Resume"),
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            });
            Add(failOverlay = new FailOverlay
            {
                OnRetry = () => Logger.Log(@"Retry"),
                OnQuit = () => Logger.Log(@"Quit"),
            });

            AddStep(@"Pause", delegate
            {
                if (failOverlay.State == Visibility.Visible)
                {
                    failOverlay.Hide();
                }
                pauseOverlay.Show();
            });
            AddStep("Fail", delegate
            {
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
        }
    }
}
