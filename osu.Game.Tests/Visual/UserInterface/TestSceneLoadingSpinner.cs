// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLoadingSpinner : OsuGridTestScene
    {
        public TestSceneLoadingSpinner()
            : base(2, 3)
        {
            LoadingSpinner loading;

            Cell(0).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingSpinner()
            });

            loading.Show();

            Cell(1).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingSpinner(true)
            });

            loading.Show();

            Cell(2).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Gray,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingSpinner()
            });

            loading.Show();

            Cell(3).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingSpinner(false, true)
            });

            loading.Show();

            Cell(4).AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                loading = new LoadingSpinner(true, true)
            });
            loading.Show();

            Cell(5).AddRange(new Drawable[]
            {
                loading = new LoadingSpinner()
            });

            Scheduler.AddDelayed(() => loading.ToggleVisibility(), 200, true);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                Beatmap.Value.Track.Start();
            });
        }

        [Test]
        public void TestBeatSyncDifferentBPM()
        {
            AddStep("set 60 BPM", () =>
                Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.BeatLength = 1000));
            AddWaitStep("wait for beats", 16);

            AddStep("set 100 BPM", () =>
                Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.BeatLength = 600));
            AddWaitStep("wait for beats", 16);

            AddStep("set 150 BPM", () =>
                Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.BeatLength = 400));
            AddWaitStep("wait for beats", 16);

            AddStep("set 200 BPM", () =>
                Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.BeatLength = 300));
            AddWaitStep("wait for beats", 16);
        }
    }
}
