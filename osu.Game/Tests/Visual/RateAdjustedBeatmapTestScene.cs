// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// Test case which adjusts the beatmap's rate to match any speed adjustments in visual tests.
    /// </summary>
    public abstract class RateAdjustedBeatmapTestScene : ScreenTestScene
    {
        protected override void Update()
        {
            base.Update();

            // note that this will override any mod rate application
            MusicController.CurrentTrack.AsNonNull().Tempo.Value = Clock.Rate;
        }
    }
}
