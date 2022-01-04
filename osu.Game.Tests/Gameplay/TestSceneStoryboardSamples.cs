// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneStoryboardSamples : OsuTestScene, IStorageResourceProvider
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestRetrieveTopLevelSample()
        {
            ISkin skin = null;
            ISample channel = null;

            AddStep("create skin", () => skin = new TestSkin("test-sample", this));
            AddStep("retrieve sample", () => channel = skin.GetSample(new SampleInfo("test-sample")));

            AddAssert("sample is non-null", () => channel != null);
        }

        [Test]
        public void TestRetrieveSampleInSubFolder()
        {
            ISkin skin = null;
            ISample channel = null;

            AddStep("create skin", () => skin = new TestSkin("folder/test-sample", this));
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

                Add(gameplayContainer = new MasterGameplayClockContainer(working, 0)
                {
                    IsPaused = { Value = true },
                    Child = new FrameStabilityContainer
                    {
                        Child = sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                    }
                });
            });

            AddStep("reset clock", () => gameplayContainer.Start());

            AddUntilStep("sample played", () => sample.RequestedPlaying);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);
        }

        [Test]
        public void TestSampleHasLifetimeEndWithInitialClockTime()
        {
            GameplayClockContainer gameplayContainer = null;
            DrawableStoryboardSample sample = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gameplayContainer = new MasterGameplayClockContainer(working, 1000, true)
                {
                    IsPaused = { Value = true },
                    Child = new FrameStabilityContainer
                    {
                        Child = sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                    }
                });
            });

            AddStep("start time", () => gameplayContainer.Start());

            AddUntilStep("sample not played", () => !sample.RequestedPlaying);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);
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
            StoryboardSampleInfo sampleInfo = null;
            TestDrawableStoryboardSample sample = null;

            Mod testedMod = Activator.CreateInstance(expectedMod) as Mod;

            switch (testedMod)
            {
                case ModRateAdjust m:
                    m.SpeedChange.Value = expectedRate;
                    break;

                case ModTimeRamp m:
                    m.FinalRate.Value = m.InitialRate.Value = expectedRate;
                    break;
            }

            AddStep("setup storyboard sample", () =>
            {
                Beatmap.Value = new TestCustomSkinWorkingBeatmap(new OsuRuleset().RulesetInfo, this);
                SelectedMods.Value = new[] { testedMod };

                var beatmapSkinSourceContainer = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

                Add(gameplayContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
                {
                    Child = beatmapSkinSourceContainer
                });

                beatmapSkinSourceContainer.Add(sample = new TestDrawableStoryboardSample(sampleInfo = new StoryboardSampleInfo("test-sample", 1, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start", () => gameplayContainer.Start());

            AddAssert("sample playback rate matches mod rates", () =>
                testedMod != null && Precision.AlmostEquals(
                    sample.ChildrenOfType<DrawableSample>().First().AggregateFrequency.Value,
                    ((IApplicableToRate)testedMod).ApplyToRate(sampleInfo.StartTime)));
        }

        [Test]
        public void TestSamplePlaybackWithBeatmapHitsoundsOff()
        {
            GameplayClockContainer gameplayContainer = null;
            TestDrawableStoryboardSample sample = null;

            AddStep("disable beatmap hitsounds", () => config.SetValue(OsuSetting.BeatmapHitsounds, false));

            AddStep("setup storyboard sample", () =>
            {
                Beatmap.Value = new TestCustomSkinWorkingBeatmap(new OsuRuleset().RulesetInfo, this);

                var beatmapSkinSourceContainer = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

                Add(gameplayContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
                {
                    Child = beatmapSkinSourceContainer
                });

                beatmapSkinSourceContainer.Add(sample = new TestDrawableStoryboardSample(new StoryboardSampleInfo("test-sample", 1, 1))
                {
                    Clock = gameplayContainer.GameplayClock
                });
            });

            AddStep("start", () => gameplayContainer.Start());

            AddUntilStep("sample played", () => sample.IsPlayed);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);

            AddStep("restore default", () => config.GetBindable<bool>(OsuSetting.BeatmapHitsounds).SetDefault());
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(string resourceName, IStorageResourceProvider resources)
                : base(DefaultLegacySkin.CreateInfo(), new TestResourceStore(resourceName), resources, "skin.ini")
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

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
                => name == resourceName ? TestResources.GetStore().GetAsync("Resources/Samples/test-sample.mp3", cancellationToken) : null;

            public Stream GetStream(string name) => name == resourceName ? TestResources.GetStore().GetStream("Resources/Samples/test-sample.mp3") : null;

            public IEnumerable<string> GetAvailableResources() => new[] { resourceName };

            public void Dispose()
            {
            }
        }

        private class TestCustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly IStorageResourceProvider resources;

            public TestCustomSkinWorkingBeatmap(RulesetInfo ruleset, IStorageResourceProvider resources)
                : base(ruleset, null, resources.AudioManager)
            {
                this.resources = resources;
            }

            protected internal override ISkin GetSkin() => new TestSkin("test-sample", resources);
        }

        private class TestDrawableStoryboardSample : DrawableStoryboardSample
        {
            public TestDrawableStoryboardSample(StoryboardSampleInfo sampleInfo)
                : base(sampleInfo)
            {
            }
        }

        #region IResourceStorageProvider

        public AudioManager AudioManager => Audio;
        public IResourceStore<byte[]> Files => null;
        public new IResourceStore<byte[]> Resources => base.Resources;
        public RealmContextFactory RealmContextFactory => null;
        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => null;

        #endregion
    }
}
