// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
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
    public partial class TestSceneStoryboardSamples : OsuTestScene, IStorageResourceProvider
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private GameHost host { get; set; }

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

                Add(gameplayContainer = new MasterGameplayClockContainer(working, 0)
                {
                    Child = new FrameStabilityContainer
                    {
                        Child = sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                    }
                });
            });

            AddStep("reset clock", () => gameplayContainer.Reset(startClock: true));

            AddUntilStep("sample played", () => sample.RequestedPlaying);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);
        }

        /// <summary>
        /// Sample at 0ms, start time at 1000ms (so the sample should not be played).
        /// </summary>
        [Test]
        public void TestSampleHasLifetimeEndWithInitialClockTime()
        {
            MasterGameplayClockContainer gameplayContainer = null;
            DrawableStoryboardSample sample = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

                const double start_time = 1000;

                Add(gameplayContainer = new MasterGameplayClockContainer(working, start_time)
                {
                    Child = new FrameStabilityContainer
                    {
                        Child = sample = new DrawableStoryboardSample(new StoryboardSampleInfo(string.Empty, 0, 1))
                    }
                });

                gameplayContainer.Reset(start_time);
            });

            AddStep("start time", () => gameplayContainer.Start());

            AddUntilStep("sample not played", () => !sample.RequestedPlaying);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);
        }

        [Test]
        public void TestSamplePlaybackWithBeatmapHitsoundsOff()
        {
            GameplayClockContainer gameplayContainer = null;
            DrawableStoryboardSample sample = null;

            AddStep("disable beatmap hitsounds", () => config.SetValue(OsuSetting.BeatmapHitsounds, false));

            AddStep("setup storyboard sample", () =>
            {
                Beatmap.Value = new TestCustomSkinWorkingBeatmap(new OsuRuleset().RulesetInfo, this);

                var beatmapSkinSourceContainer = new BeatmapSkinProvidingContainer(Beatmap.Value.Skin);

                Add(gameplayContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
                {
                    Child = beatmapSkinSourceContainer
                });

                beatmapSkinSourceContainer.Add(sample = new DrawableStoryboardSample(new StoryboardSampleInfo("test-sample", 1, 1))
                {
                    Clock = gameplayContainer
                });
            });

            AddStep("reset clock", () => gameplayContainer.Reset(startClock: true));

            AddUntilStep("sample played", () => sample.IsPlayed);
            AddUntilStep("sample has lifetime end", () => sample.LifetimeEnd < double.MaxValue);

            AddStep("restore default", () => config.GetBindable<bool>(OsuSetting.BeatmapHitsounds).SetDefault());
        }

        private class TestSkin : LegacySkin
        {
            public TestSkin(string resourceName, IStorageResourceProvider resources)
                : base(DefaultLegacySkin.CreateInfo(), resources, new TestResourceStore(resourceName))
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

        #region IResourceStorageProvider

        public IRenderer Renderer => host.Renderer;
        public AudioManager AudioManager => Audio;
        public IResourceStore<byte[]> Files => null;
        public new IResourceStore<byte[]> Resources => base.Resources;
        public RealmAccess RealmAccess => null;
        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => null;

        #endregion
    }
}
