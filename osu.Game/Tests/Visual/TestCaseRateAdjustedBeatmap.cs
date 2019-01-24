// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Tests.Visual
{
    public class TestCaseRateAdjustedBeatmap : ScreenTestCase
    {
        protected override void Update()
        {
            base.Update();

            // note that this will override any mod rate application
            Beatmap.Value.Track.Rate = Clock.Rate;
        }
    }
}
