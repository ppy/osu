// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets.Mods;
using System.Linq;
using NUnit.Framework;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public abstract class SliderTestBase : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderBall),
            typeof(SliderBody),
            typeof(SliderTick),
            typeof(DrawableSlider),
            typeof(DrawableSliderTick),
            typeof(DrawableRepeatPoint),
            typeof(DrawableOsuHitObject)
        };

        protected int DepthIndex;

        protected abstract List<Mod> Mods { get; set; }
        protected Slider CreateSlider(float circleSize = 2, float distance = 400, int repeats = 0, double speedMultiplier = 2, int stackHeight = 0, bool addToContent = true)
        {
            var slider = new Slider
            {
                StartTime = addToContent ? Time.Current + 1000 : 1500,
                Position = addToContent ? new Vector2(-(distance / 2), 0) : new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(distance, 0),
                }, distance),
                RepeatCount = repeats,
                NodeSamples = CreateEmptySamples(repeats),
                StackHeight = stackHeight
            };

            if (addToContent)
                AddSlider(slider, circleSize, speedMultiplier);

            return slider;
        }

        protected List<List<SampleInfo>> CreateEmptySamples(int repeats)
        {
            var repeatSamples = new List<List<SampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<SampleInfo>());
            return repeatSamples;
        }

        protected void AddSlider(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty { CircleSize = circleSize, SliderTickRate = 3 });

            var drawable = new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Depth = DepthIndex++
            };

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            drawable.OnNewResult += onNewResult;
            Add(drawable);
        }

        private float judgementOffsetDirection = 1;
        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            var osuObject = judgedObject as DrawableOsuHitObject;
            if (osuObject == null)
                return;

            OsuSpriteText text;
            Add(text = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = result.IsHit ? "Hit!" : "Miss!",
                Colour = result.IsHit ? Color4.Green : Color4.Red,
                TextSize = 30,
                Position = osuObject.HitObject.StackedEndPosition + judgementOffsetDirection * new Vector2(0, 45)
            });

            text.Delay(150)
                .Then().FadeOut(200)
                .Then().Expire();

            judgementOffsetDirection *= -1;
        }
    }
}
