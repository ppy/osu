// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModMuted : OsuModTestScene
    {
        /// <summary>
        /// Ensures that a final volume combo of 0 (i.e. "always muted" mode) constantly plays metronome and completely mutes track.
        /// </summary>
        [TestCase(0.0, 1.0)]
        public void TestZeroFinalCombo(double expectedTrackVolume, double expectedMetronomeVolume) => CreateModTest(new ModTestData
        {
            Mod = new OsuModMuted
            {
                MuteComboCount = { Value = 0 },
            },
            PassCondition = () => Beatmap.Value.Track.AggregateVolume.Value == expectedTrackVolume &&
                                  Player.ChildrenOfType<Metronome>().SingleOrDefault()?.AggregateVolume.Value == expectedMetronomeVolume,
        });
    }
}
