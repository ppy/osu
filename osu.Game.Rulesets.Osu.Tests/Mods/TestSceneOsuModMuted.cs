// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModMuted : OsuModTestScene
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
                                  Player.ChildrenOfType<MetronomeBeat>().SingleOrDefault()?.AggregateVolume.Value == 1.0,
        });

        /// <summary>
        /// Ensures that copying from a normal mod with 0 final combo while originally inversed does not yield incorrect results.
        /// </summary>
        [Test]
        public void TestModCopy()
        {
            OsuModMuted muted = null!;

            AddStep("create inversed mod", () => muted = new OsuModMuted
            {
                MuteComboCount = { Value = 100 },
                InverseMuting = { Value = true },
            });

            AddStep("copy from normal", () => muted.CopyFrom(new OsuModMuted
            {
                MuteComboCount = { Value = 0 },
                InverseMuting = { Value = false },
            }));

            AddAssert("mute combo count copied", () => muted.MuteComboCount.Value, () => Is.EqualTo(0));
            AddAssert("inverse muting copied", () => muted.InverseMuting.Value, () => Is.False);
        }
    }
}
