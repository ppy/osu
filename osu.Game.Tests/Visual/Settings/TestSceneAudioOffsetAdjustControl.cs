// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        [BackgroundDependencyLoader]
        private void load()
        {
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

        [Test]
        public void TestBehaviour()
        {
            AddStep("create control", () => Child = new AudioOffsetAdjustControl
            {
                Current = new BindableDouble
                {
                    MinValue = -500,
                    MaxValue = 500
                }
            });
            AddStep("set new score", () => statics.SetValue(Static.LastLocalUserScore, new ScoreInfo
            {
                HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(RNG.NextDouble(-100, 100)),
                BeatmapInfo = Beatmap.Value.BeatmapInfo,
            }));
            AddStep("clear history", () => tracker.ClearHistory());
        }
    }
}
