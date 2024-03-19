// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplaySamplePlayback : PlayerTestScene
    {
        protected override bool AllowBackwardsSeeks => true;

        private bool seek;

        [Test]
        public void TestAllSamplesStopDuringSeek()
        {
            DrawableSlider? slider = null;
            PoolableSkinnableSample[] samples = null!;
            ISamplePlaybackDisabler sampleDisabler = null!;

            AddUntilStep("get variables", () =>
            {
                sampleDisabler = Player;
                slider = Player.ChildrenOfType<DrawableSlider>().MinBy(s => s.HitObject.StartTime);
                samples = slider.ChildrenOfType<PoolableSkinnableSample>().ToArray();

                return slider != null;
            });

            AddUntilStep("wait for slider sliding then seek", () =>
            {
                if (slider?.Tracking.Value != true)
                    return false;

                if (!samples.Any(s => s.Playing))
                    return false;

                seek = true;
                return true;
            });

            AddAssert("sample playback disabled", () => sampleDisabler.SamplePlaybackDisabled.Value);

            // because we are in frame stable context, it's quite likely that not all samples are "played" at this point.
            // the important thing is that at least one started, and that sample has since stopped.
            AddAssert("all looping samples stopped immediately", () => allStopped(allLoopingSounds));
            AddUntilStep("all samples stopped eventually", () => allStopped(allSounds));

            AddAssert("sample playback still disabled", () => sampleDisabler.SamplePlaybackDisabled.Value);

            AddStep("stop seeking", () => seek = false);

            AddUntilStep("seek finished, sample playback enabled", () => !sampleDisabler.SamplePlaybackDisabled.Value);
            AddUntilStep("any sample is playing", () => Player.ChildrenOfType<PausableSkinnableSound>().Any(s => s.IsPlaying));
        }

        protected override void Update()
        {
            base.Update();

            if (seek)
            {
                // Frame stable playback is too fast to catch up these days.
                //
                // We want to keep seeking while asserting various test conditions, so
                // continue to seek until we unset the flag.
                var gameplayClockContainer = Player.ChildrenOfType<GameplayClockContainer>().First();
                gameplayClockContainer.Seek(gameplayClockContainer.CurrentTime > 30000 ? 0 : 60000);
            }
        }

        private IEnumerable<PausableSkinnableSound> allSounds => Player.ChildrenOfType<PausableSkinnableSound>();
        private IEnumerable<PausableSkinnableSound> allLoopingSounds => allSounds.Where(sound => sound.Looping);

        private bool allStopped(IEnumerable<PausableSkinnableSound> sounds) => sounds.All(sound => !sound.IsPlaying);

        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
