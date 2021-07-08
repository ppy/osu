// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModDifficultyAdjustSettings : OsuManualInputManagerTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create difficulty adjust", () =>
            {
                var modDifficultyAdjust = new OsuModDifficultyAdjust();

                Child = new Container
                {
                    Size = new Vector2(300),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ChildrenEnumerable = modDifficultyAdjust.CreateSettingsControls(),
                        },
                    }
                };
            });

            setBeatmapWithDifficultyParameters(5);
            setBeatmapWithDifficultyParameters(8);
        }

        [Test]
        public void TestBasic()
        {
        }

        private void setBeatmapWithDifficultyParameters(float value)
        {
            AddStep($"set beatmap with all {value}", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap()
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = value,
                        CircleSize = value,
                        DrainRate = value,
                        ApproachRate = value,
                    }
                }
            }));
        }
    }
}
