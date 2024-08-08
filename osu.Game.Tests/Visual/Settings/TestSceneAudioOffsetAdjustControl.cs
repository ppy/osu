// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings.Sections.Audio;
using osu.Game.Scoring;
using osu.Game.Tests.Visual.Ranking;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneAudioOffsetAdjustControl : OsuTestScene
    {
        [Resolved]
        private SessionStatics statics { get; set; } = null!;

        [Cached]
        private SessionAverageHitErrorTracker tracker = new SessionAverageHitErrorTracker();

        private Container content = null!;
        protected override Container Content => content;

        private OsuConfigManager localConfig = null!;
        private AudioOffsetAdjustControl adjustControl = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            localConfig = new OsuConfigManager(LocalStorage);
            Dependencies.CacheAs(localConfig);

            base.Content.AddRange(new Drawable[]
            {
                tracker,
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 400,
                    AutoSizeAxes = Axes.Y
                }
            });
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = adjustControl = new AudioOffsetAdjustControl
            {
                Current = localConfig.GetBindable<double>(OsuSetting.AudioOffset),
            };

            localConfig.SetValue(OsuSetting.AudioOffset, 0.0);
            tracker.ClearHistory();
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("set new score", () => statics.SetValue(Static.LastLocalUserScore, new ScoreInfo
            {
                HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(RNG.NextDouble(-100, 100)),
                BeatmapInfo = Beatmap.Value.BeatmapInfo,
            }));
            AddStep("clear history", () => tracker.ClearHistory());
        }

        [Test]
        public void TestBehaviour()
        {
            AddStep("set score with -20ms", () => setScore(-20));
            AddAssert("suggested global offset is 20ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(20));
            AddStep("clear history", () => tracker.ClearHistory());

            AddStep("set score with 40ms", () => setScore(40));
            AddAssert("suggested global offset is -40ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(-40));
            AddStep("clear history", () => tracker.ClearHistory());
        }

        [Test]
        public void TestNonZeroGlobalOffset()
        {
            AddStep("set global offset to -20ms", () => localConfig.SetValue(OsuSetting.AudioOffset, -20.0));
            AddStep("set score with -20ms", () => setScore(-20));
            AddAssert("suggested global offset is 0ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(0));
            AddStep("clear history", () => tracker.ClearHistory());

            AddStep("set global offset to 20ms", () => localConfig.SetValue(OsuSetting.AudioOffset, 20.0));
            AddStep("set score with 40ms", () => setScore(40));
            AddAssert("suggested global offset is -20ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(-20));
            AddStep("clear history", () => tracker.ClearHistory());
        }

        [Test]
        public void TestMultiplePlays()
        {
            AddStep("set score with -20ms", () => setScore(-20));
            AddStep("set score with -10ms", () => setScore(-10));
            AddAssert("suggested global offset is 15ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(15));
            AddStep("clear history", () => tracker.ClearHistory());

            AddStep("set score with -20ms", () => setScore(-20));
            AddStep("set global offset to 30ms", () => localConfig.SetValue(OsuSetting.AudioOffset, 30.0));
            AddStep("set score with 10ms", () => setScore(10));
            AddAssert("suggested global offset is 20ms", () => adjustControl.SuggestedOffset.Value, () => Is.EqualTo(20));
            AddStep("clear history", () => tracker.ClearHistory());
        }

        private void setScore(double averageHitError)
        {
            statics.SetValue(Static.LastLocalUserScore, new ScoreInfo
            {
                HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(averageHitError),
                BeatmapInfo = Beatmap.Value.BeatmapInfo,
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            if (localConfig.IsNotNull())
                localConfig.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
