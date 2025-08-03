// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneStrainGraph : OsuTestScene
    {
        private GameplayClockContainer gameplayClockContainer = null!;

        private Box background = null!;
        private OsuSpriteText timeText = null!;
        private StrainGraph strainGraph;

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
                timeText = new OsuSpriteText
                {
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24)
                },
                gameplayClockContainer = new MasterGameplayClockContainer(Beatmap.Value, skip_target_time)
                {
                    Child = frameStabilityContainer = new FrameStabilityContainer
                    {
                        Child = new FakeLoad()
                    },
                },
            });

            Dependencies.CacheAs<IGameplayClock>(gameplayClockContainer);
            Dependencies.CacheAs<IFrameStableClock>(frameStabilityContainer);
        }

        private partial class FakeLoad : Drawable
        {
            protected override void Update()
            {
                base.Update();
                Thread.Sleep(1);
            }
        }

        protected override void Update()
        {
            base.Update();

            timeText.Text = $"Current time: {gameplayClockContainer.CurrentTime:F2}";
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("add graph", () =>
            {
                strainGraph?.Expire();

                Add(strainGraph = new StrainGraph
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            });

            AddStep("reset clock", () => gameplayClockContainer.Reset());
            AddStep("set hit objects", () => strainGraph.Objects = Beatmap.Value.Beatmap.HitObjects);
            AddStep("seek to intro", () => gameplayClockContainer.Seek(skip_target_time));
            AddStep("start", () => gameplayClockContainer.Start());
        }

        [Test]
        public void TestBasic()
        {
            AddSliderStep("width", 0, 800, 400, v => strainGraph?.ResizeWidthTo(v));
            AddSliderStep("height", 0, 800, 100, v => strainGraph?.ResizeHeightTo(v));

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
    }
}
