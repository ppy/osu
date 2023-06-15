// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSongProgress : SkinnableHUDComponentTestScene
    {
        private GameplayClockContainer gameplayClockContainer = null!;

        private Box background = null!;

        private const double skip_target_time = -2000;

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            FrameStabilityContainer frameStabilityContainer;

            AddRange(new Drawable[]
            {
                background = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                gameplayClockContainer = new MasterGameplayClockContainer(Beatmap.Value, skip_target_time)
                {
                    Child = frameStabilityContainer = new FrameStabilityContainer
                    {
                        MaxCatchUpFrames = 1
                    }
                }
            });

            Dependencies.CacheAs<IGameplayClock>(gameplayClockContainer);
            Dependencies.CacheAs<IFrameStableClock>(frameStabilityContainer);
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("reset clock", () => gameplayClockContainer.Reset());
            AddStep("set hit objects", () => this.ChildrenOfType<SongProgress>().ForEach(progress => progress.Objects = Beatmap.Value.Beatmap.HitObjects));
            AddStep("hook seeking", () =>
            {
                applyToDefaultProgress(d => d.ChildrenOfType<DefaultSongProgressBar>().Single().OnSeek += t => gameplayClockContainer.Seek(t));
                applyToArgonProgress(d => d.ChildrenOfType<ArgonSongProgressBar>().Single().OnSeek += t => gameplayClockContainer.Seek(t));
            });
            AddStep("seek to intro", () => gameplayClockContainer.Seek(skip_target_time));
            AddStep("start", () => gameplayClockContainer.Start());
        }

        [Test]
        public void TestBasic()
        {
            AddToggleStep("toggle seeking", b =>
            {
                applyToDefaultProgress(s => s.Interactive.Value = b);
                applyToArgonProgress(s => s.Interactive.Value = b);
            });

            AddToggleStep("toggle graph", b =>
            {
                applyToDefaultProgress(s => s.ShowGraph.Value = b);
                applyToArgonProgress(s => s.ShowGraph.Value = b);
            });

            AddStep("set white background", () => background.FadeColour(Color4.White, 200, Easing.OutQuint));
            AddStep("randomise background colour", () => background.FadeColour(new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1), 200, Easing.OutQuint));

            AddStep("stop", gameplayClockContainer.Stop);
        }

        [Test]
        public void TestSeekToKnownTime()
        {
            AddStep("seek to known time", () => gameplayClockContainer.Seek(60000));
            AddWaitStep("wait some for seek", 15);
            AddStep("stop", () => gameplayClockContainer.Stop());
        }

        private void applyToArgonProgress(Action<ArgonSongProgress> action) =>
            this.ChildrenOfType<ArgonSongProgress>().ForEach(action);

        private void applyToDefaultProgress(Action<DefaultSongProgress> action) =>
            this.ChildrenOfType<DefaultSongProgress>().ForEach(action);

        protected override Drawable CreateDefaultImplementation() => new DefaultSongProgress();

        protected override Drawable CreateArgonImplementation() => new ArgonSongProgress();

        protected override Drawable CreateLegacyImplementation() => new LegacySongProgress();
    }
}
