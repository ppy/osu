// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Storyboards;
using osuTK;
using static osu.Game.Tests.Visual.OsuTestScene.ClockBackedTestWorkingBeatmap;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSliderSnaking : TestSceneOsuPlayer
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        private TrackVirtualManual track;

        protected override bool Autoplay => autoplay;
        private bool autoplay;

        private readonly Bindable<bool> snakingIn = new Bindable<bool>();
        private readonly Bindable<bool> snakingOut = new Bindable<bool>();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);
            track = (TrackVirtualManual)working.Track;
            return working;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetConfigCache configCache)
        {
            var config = (OsuRulesetConfigManager)configCache.GetConfigFor(Ruleset.Value.CreateInstance());
            config.BindWith(OsuRulesetSetting.SnakingInSliders, snakingIn);
            config.BindWith(OsuRulesetSetting.SnakingOutSliders, snakingOut);
        }

        private DrawableSlider slider;
        private DrawableSliderRepeat repeat;
        private Vector2 vector;

        [SetUpSteps]
        public override void SetUpSteps() { }

        [Test]
        public void TestSnaking()
        {
            AddStep("have autoplay", () => autoplay = true);
            base.SetUpSteps();
            AddUntilStep("wait for track to start running", () => track.IsRunning);

            AddStep("retrieve 1st slider", () => slider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.First());
            testLinear(true);
            testLinear(false);
            AddStep("retrieve 2nd slider", () => slider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.Skip(1).First());
            testRepeating(true);
            testRepeating(false);
            AddStep("retrieve 3rd slider", () => slider = (DrawableSlider)Player.DrawableRuleset.Playfield.AllHitObjects.Skip(2).First());
            testDoubleRepeating(true);
            testDoubleRepeating(false);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestArrowStays(bool isHit)
        {
            var isSame = isHit ? "is same" : "decreased";
            var enable = isHit ? "enable" : "disable";

            AddStep($"{enable} autoplay", () => autoplay = isHit);
            setSnaking(true);
            base.SetUpSteps();

            addSeekStep(13500);
            AddStep("retrieve 2nd slider repeat", () =>
            {
                var drawable = Player.DrawableRuleset.Playfield.AllHitObjects.Skip(1).First();
                repeat = drawable.ChildrenOfType<Container<DrawableSliderRepeat>>().First().Children.First();
            });
            AddStep("Save repeat vector", () => vector = repeat.Position);
            addSeekStep(13700);
            AddAssert($"Repeat vector {isSame}", () => isHit ? Precision.AlmostEquals(vector.X, repeat.X, 1) && Precision.AlmostEquals(vector.Y, repeat.Y, 1) : repeat.X < vector.X && repeat.Y < vector.Y);
        }

        private void testLinear(bool snaking)
        {
            var increased = snaking ? "increased" : "is same";

            setSnaking(snaking);
            addSeekStep(1800);
            AddStep("Save end vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.Last();
            });
            addSeekStep(1900);
            AddAssert($"End vector {increased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var last = body.CurrentCurve.Last();
                return snaking ? last.X > vector.X && last.Y > vector.Y : last == vector;
            });
            addSeekStep(3100);
            AddStep("Save start vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.First();
            });
            addSeekStep(3200);
            AddAssert($"Start vector {increased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var first = body.CurrentCurve.First();
                return snaking ? first.X > vector.X && first.Y > vector.Y : first == vector;
            });
        }

        private void testRepeating(bool snaking)
        {
            var increased = snaking ? "increased" : "is same";
            var decreased = snaking ? "decreased" : "is same";

            setSnaking(snaking);
            addSeekStep(8800);
            AddStep("Save end vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.Last();
            });
            addSeekStep(8900);
            AddAssert($"End vector {increased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var last = body.CurrentCurve.Last();
                return snaking ? last.X > vector.X && last.Y > vector.Y : last == vector;
            });
            addSeekStep(10100);
            AddStep("Save start vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.First();
            });
            addSeekStep(10200);
            AddAssert("Start vector is same", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var first = body.CurrentCurve.First();
                return first == vector;
            });
            addSeekStep(13700);
            AddStep("Save end vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.Last();
            });
            addSeekStep(13800);
            AddAssert($"End vector {decreased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var last = body.CurrentCurve.Last();
                return snaking ? last.X < vector.X && last.Y < vector.Y : last == vector;
            });
        }

        private void testDoubleRepeating(bool snaking)
        {
            var increased = snaking ? "increased" : "is same";

            setSnaking(snaking);
            addSeekStep(18800);
            AddStep("Save end vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.Last();
            });
            addSeekStep(18900);
            AddAssert($"End vector {increased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var last = body.CurrentCurve.Last();
                return snaking ? last.X > vector.X && last.Y > vector.Y : last == vector;
            });
            addSeekStep(20100);
            AddStep("Save start vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.First();
            });
            addSeekStep(20200);
            AddAssert("Start vector is same", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var first = body.CurrentCurve.First();
                return first == vector;
            });
            addSeekStep(23700);
            AddStep("Save end vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.Last();
            });
            addSeekStep(23800);
            AddAssert("End vector is same", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var last = body.CurrentCurve.Last();
                return last == vector;
            });
            addSeekStep(27300);
            AddStep("Save start vector", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                vector = body.CurrentCurve.First();
            });
            addSeekStep(27400);
            AddAssert($"Start vector {increased}", () =>
            {
                var body = (PlaySliderBody)slider.Body.Drawable;
                var first = body.CurrentCurve.First();
                return snaking ? first.X > vector.X && first.Y > vector.Y : first == vector;
            });
        }

        private void setSnaking(bool value)
        {
            var text = value ? "Enable" : "Disable";
            AddStep($"{text} snaking", () =>
            {
                snakingIn.Value = value;
                snakingOut.Value = value;
            });
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => track.Seek(time));

            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Slider
                {
                    StartTime = 3000,
                    Position = new Vector2(100, 100),
                    Path = new SliderPath(PathType.PerfectCurve, new[]
                    {
                        Vector2.Zero,
                        new Vector2(300, 200)
                    }),
                },
                new Slider
                {
                    StartTime = 10000,
                    Position = new Vector2(100, 100),
                    Path = new SliderPath(PathType.PerfectCurve, new[]
                    {
                        Vector2.Zero,
                        new Vector2(300, 200)
                    }),
                    RepeatCount = 1,
                },

                new Slider
                {
                    StartTime = 20000,
                    Position = new Vector2(100, 100),
                    Path = new SliderPath(PathType.PerfectCurve, new[]
                    {
                        Vector2.Zero,
                        new Vector2(300, 200)
                    }),
                    RepeatCount = 2,
                },

                new HitCircle
                {
                    StartTime = 99999,
                }
            }
        };
    }
}
