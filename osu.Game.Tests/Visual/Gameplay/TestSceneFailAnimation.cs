// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneFailAnimation : TestSceneAllRulesetPlayers
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = Array.Empty<Mod>();
            return new FailPlayer();
        }

        [Test]
        public void TestOsuWithoutRedTint()
        {
            AddStep("Disable red tint", () => Config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, false));
            TestOsu();
            AddStep("Enable red tint", () => Config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, true));
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("wait for fail", () => Player.GameplayState.HasFailed);
            AddUntilStep("wait for fail overlay", () => ((FailPlayer)Player).FailOverlay.State.Value == Visibility.Visible);

            // The pause screen and fail animation both ramp frequency.
            // This tests to ensure that it doesn't reset during that handoff.
            AddAssert("frequency only ever decreased", () => !((FailPlayer)Player).FrequencyIncreased);
        }

        private partial class FailPlayer : TestPlayer
        {
            public new FailOverlay FailOverlay => base.FailOverlay;

            public bool FrequencyIncreased { get; private set; }

            public FailPlayer()
                : base(false, false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                HealthProcessor.FailConditions += (_, _) => true;
            }

            private double lastFrequency = double.MaxValue;

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                // This must be done in UpdateAfterChildren to allow the gameplay clock to have updated before checking values.
                double freq = Beatmap.Value.Track.AggregateFrequency.Value;

                FrequencyIncreased |= freq > lastFrequency;

                lastFrequency = freq;
            }
        }
    }
}
