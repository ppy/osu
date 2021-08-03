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
        [Test]
        public void TestZeroFinalCombo() => CreateModTest(new ModTestData
        {
            Mod = new OsuModMuted
            {
                MuteComboCount = { Value = 0 },
            },
            PassCondition = () => Beatmap.Value.Track.AggregateVolume.Value == 0.0 &&
                                  Player.ChildrenOfType<Metronome>().SingleOrDefault()?.AggregateVolume.Value == 1.0,
        });
    }
}
