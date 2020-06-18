// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneStoryboardSamples : OsuTestScene
    {
        [Test]
        public void TestRetrieveTopLevelSample()
        {
            ISkin skin = null;
            SampleChannel channel = null;

            AddStep("create skin", () => skin = new TestSkin("test-sample", Audio));
            AddStep("retrieve sample", () => channel = skin.GetSample(new SampleInfo("test-sample")));

            AddAssert("sample is non-null", () => channel != null);
        }

        [Test]
        public void TestRetrieveSampleInSubFolder()
        {
            ISkin skin = null;
            SampleChannel channel = null;

            AddStep("create skin", () => skin = new TestSkin("folder/test-sample", Audio));
            AddStep("retrieve sample", () => channel = skin.GetSample(new SampleInfo("folder/test-sample")));

            AddAssert("sample is non-null", () => channel != null);
        }

        [Test]
        public void TestSamplePlaybackAtZero()
        {
            GameplayClockContainer gameplayContainer = null;
            DrawableStoryboardSample sample = null;

            AddStep("create container", () =>
            {
                Add(gameplayContainer = new GameplayClockContainer(CreateWorkingBeatmap(new OsuRuleset().RulesetInfo), Array.Empty<Mod>(), 0));

                gameplayContainer.Add(sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start time", () => gameplayContainer.Start());

            AddUntilStep("sample playback succeeded", () => sample.LifetimeEnd < double.MaxValue);
        }

        [Test]
        public void TestSamplePlaybackWithRateMods()
        {
            GameplayClockContainer gameplayContainer = null;
            TestDrawableStoryboardSample sample = null;

            OsuModDoubleTime doubleTimeMod = null;

            AddStep("create container", () =>
            {
                var beatmap = Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

                Add(gameplayContainer = new GameplayClockContainer(beatmap, new[] { doubleTimeMod = new OsuModDoubleTime() }, 0));

                SelectedMods.Value = new[] { doubleTimeMod };
                Beatmap.Value = new TestCustomSkinWorkingBeatmap(beatmap.Beatmap, gameplayContainer.GameplayClock, Audio);
            });

            AddStep("create storyboard sample", () =>
            {
                gameplayContainer.Add(sample = new TestDrawableStoryboardSample(new StoryboardSampleInfo("test-sample", 1, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start", () => gameplayContainer.Start());

            AddAssert("sample playback rate matches mod rates", () => sample.TestChannel.AggregateFrequency.Value == doubleTimeMod.SpeedChange.Value);
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(string resourceName, AudioManager audioManager)
                : base(DefaultLegacySkin.Info, new TestResourceStore(resourceName), audioManager, "skin.ini")
            {
            }
        }

        private class TestResourceStore : IResourceStore<byte[]>
        {
            private readonly string resourceName;

            public TestResourceStore(string resourceName)
            {
                this.resourceName = resourceName;
            }

            public byte[] Get(string name) => name == resourceName ? TestResources.GetStore().Get("Resources/Samples/test-sample.mp3") : null;

            public Task<byte[]> GetAsync(string name) => name == resourceName ? TestResources.GetStore().GetAsync("Resources/Samples/test-sample.mp3") : null;

            public Stream GetStream(string name) => name == resourceName ? TestResources.GetStore().GetStream("Resources/Samples/test-sample.mp3") : null;

            public IEnumerable<string> GetAvailableResources() => new[] { resourceName };

            public void Dispose()
            {
            }
        }

        private class TestCustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly AudioManager audio;

            public TestCustomSkinWorkingBeatmap(IBeatmap beatmap, IFrameBasedClock referenceClock, AudioManager audio)
                : base(beatmap, null, referenceClock, audio)
            {
                this.audio = audio;
            }

            protected override ISkin GetSkin() => new TestSkin("test-sample", audio);
        }

        private class TestDrawableStoryboardSample : DrawableStoryboardSample
        {
            public TestDrawableStoryboardSample(StoryboardSampleInfo sampleInfo)
                : base(sampleInfo)
            {
            }

            public SampleChannel TestChannel => Channel;
        }
    }
}
