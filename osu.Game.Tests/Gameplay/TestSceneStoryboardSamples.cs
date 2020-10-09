// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets;
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
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gameplayContainer = new GameplayClockContainer(working, 0));

                gameplayContainer.Add(sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start time", () => gameplayContainer.Start());

            AddUntilStep("sample playback succeeded", () => sample.LifetimeEnd < double.MaxValue);
        }

        [TestCase(typeof(OsuModDoubleTime), 1.5)]
        [TestCase(typeof(OsuModHalfTime), 0.75)]
        [TestCase(typeof(ModWindUp), 1.5)]
        [TestCase(typeof(ModWindDown), 0.75)]
        [TestCase(typeof(OsuModDoubleTime), 2)]
        [TestCase(typeof(OsuModHalfTime), 0.5)]
        [TestCase(typeof(ModWindUp), 2)]
        [TestCase(typeof(ModWindDown), 0.5)]
        public void TestSamplePlaybackWithRateMods(Type expectedMod, double expectedRate)
        {
            GameplayClockContainer gameplayContainer = null;
            TestDrawableStoryboardSample sample = null;

            Mod testedMod = Activator.CreateInstance(expectedMod) as Mod;

            switch (testedMod)
            {
                case ModRateAdjust m:
                    m.SpeedChange.Value = expectedRate;
                    break;

                case ModTimeRamp m:
                    m.InitialRate.Value = m.FinalRate.Value = expectedRate;
                    break;
            }

            AddStep("setup storyboard sample", () =>
            {
                Beatmap.Value = new TestCustomSkinWorkingBeatmap(new OsuRuleset().RulesetInfo, Audio);
                SelectedMods.Value = new[] { testedMod };

                var beatmapSkinSourceContainer = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

                Add(gameplayContainer = new GameplayClockContainer(Beatmap.Value, 0)
                {
                    Child = beatmapSkinSourceContainer
                });

                beatmapSkinSourceContainer.Add(sample = new TestDrawableStoryboardSample(new StoryboardSampleInfo("test-sample", 1, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start", () => gameplayContainer.Start());

            AddAssert("sample playback rate matches mod rates", () => sample.ChildrenOfType<DrawableSample>().First().AggregateFrequency.Value == expectedRate);
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

            public TestCustomSkinWorkingBeatmap(RulesetInfo ruleset, AudioManager audio)
                : base(ruleset, null, audio)
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
        }
    }
}
