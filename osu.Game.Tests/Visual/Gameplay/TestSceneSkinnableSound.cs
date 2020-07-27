// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinnableSound : OsuTestScene
    {
        [Cached]
        private GameplayClock gameplayClock = new GameplayClock(new FramedClock());

        private SkinnableSound skinnableSounds;

        [SetUp]
        public void SetUp()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Clock = gameplayClock,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        skinnableSounds = new SkinnableSound(new SampleInfo("normal-sliderslide"))
                        {
                            Looping = true
                        }
                    }
                },
            };
        }

        [Test]
        public void TestStoppedSoundDoesntResumeAfterPause()
        {
            DrawableSample sample = null;
            AddStep("start sample", () =>
            {
                skinnableSounds.Play();
                sample = skinnableSounds.ChildrenOfType<DrawableSample>().First();
            });

            AddUntilStep("wait for sample to start playing", () => sample.Playing);

            AddStep("stop sample", () => skinnableSounds.Stop());

            AddUntilStep("wait for sample to stop playing", () => !sample.Playing);

            AddStep("pause gameplay clock", () => gameplayClock.IsPaused.Value = true);
            AddStep("resume gameplay clock", () => gameplayClock.IsPaused.Value = false);

            AddWaitStep("wait a bit", 5);
            AddAssert("sample not playing", () => !sample.Playing);
        }

        [Test]
        public void TestLoopingSoundResumesAfterPause()
        {
            DrawableSample sample = null;
            AddStep("start sample", () =>
            {
                skinnableSounds.Play();
                sample = skinnableSounds.ChildrenOfType<DrawableSample>().First();
            });

            AddUntilStep("wait for sample to start playing", () => sample.Playing);

            AddStep("pause gameplay clock", () => gameplayClock.IsPaused.Value = true);
            AddUntilStep("wait for sample to stop playing", () => !sample.Playing);

            AddStep("resume gameplay clock", () => gameplayClock.IsPaused.Value = false);
            AddUntilStep("wait for sample to start playing", () => sample.Playing);
        }
    }
}
