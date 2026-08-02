// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModMirror : OsuModTestScene
    {
        [Test]
        public void TestCorrectReflections([Values] OsuModMirror.MirrorType type, [Values] bool withStrictTracking) => CreateModTest(new ModTestData
        {
            Autoplay = true,
            CreateBeatmap = () => new OsuBeatmap
            {
                HitObjects =
                {
                    new Slider
                    {
                        Position = new Vector2(0),
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(100, 0))
                            }
                        },
                        TickDistanceMultiplier = 0.5,
                        RepeatCount = 1,
                    }
                }
            },
            Mods = withStrictTracking
                ? [new OsuModMirror { Reflection = { Value = type } }, new OsuModStrictTracking()]
                : [new OsuModMirror { Reflection = { Value = type } }],
            PassCondition = () =>
            {
                var slider = this.ChildrenOfType<DrawableSlider>().SingleOrDefault();
                var playfield = this.ChildrenOfType<OsuPlayfield>().Single();

                if (slider == null)
                    return false;

                return Precision.AlmostEquals(playfield.ToLocalSpace(slider.HeadCircle.ScreenSpaceDrawQuad.Centre), slider.HitObject.Position)
                       && Precision.AlmostEquals(playfield.ToLocalSpace(slider.TailCircle.ScreenSpaceDrawQuad.Centre), slider.HitObject.Position)
                       && Precision.AlmostEquals(playfield.ToLocalSpace(slider.NestedHitObjects.OfType<DrawableSliderRepeat>().Single().ScreenSpaceDrawQuad.Centre),
                           slider.HitObject.Position + slider.HitObject.Path.PositionAt(1))
                       && Precision.AlmostEquals(playfield.ToLocalSpace(slider.NestedHitObjects.OfType<DrawableSliderTick>().First().ScreenSpaceDrawQuad.Centre),
                           slider.HitObject.Position + slider.HitObject.Path.PositionAt(0.7f));
            }
        });
    }
}
