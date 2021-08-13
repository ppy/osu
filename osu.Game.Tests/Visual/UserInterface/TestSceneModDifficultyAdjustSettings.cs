// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModDifficultyAdjustSettings : OsuManualInputManagerTestScene
    {
        private OsuModDifficultyAdjust modDifficultyAdjust;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create control", () =>
            {
                modDifficultyAdjust = new OsuModDifficultyAdjust();

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
        }

        [Test]
        public void TestFollowsBeatmapDefaultsVisually()
        {
            setBeatmapWithDifficultyParameters(5);

            checkSliderAtValue("Circle Size", 5);
            checkBindableAtValue("Circle Size", null);

            setBeatmapWithDifficultyParameters(8);

            checkSliderAtValue("Circle Size", 8);
            checkBindableAtValue("Circle Size", null);
        }

        [Test]
        public void TestOutOfRangeValueStillApplied()
        {
            AddStep("set override cs to 11", () => modDifficultyAdjust.CircleSize.Value = 11);

            checkSliderAtValue("Circle Size", 11);
            checkBindableAtValue("Circle Size", 11);

            // this is a no-op, just showing that it won't reset the value during deserialisation.
            setExtendedLimits(false);

            checkSliderAtValue("Circle Size", 11);
            checkBindableAtValue("Circle Size", 11);

            // setting extended limits will reset the serialisation exception.
            // this should be fine as the goal is to allow, at most, the value of extended limits.
            setExtendedLimits(true);

            checkSliderAtValue("Circle Size", 11);
            checkBindableAtValue("Circle Size", 11);
        }

        [Test]
        public void TestExtendedLimits()
        {
            setSliderValue("Circle Size", 99);

            checkSliderAtValue("Circle Size", 10);
            checkBindableAtValue("Circle Size", 10);

            setExtendedLimits(true);

            checkSliderAtValue("Circle Size", 10);
            checkBindableAtValue("Circle Size", 10);

            setSliderValue("Circle Size", 99);

            checkSliderAtValue("Circle Size", 11);
            checkBindableAtValue("Circle Size", 11);

            setExtendedLimits(false);

            checkSliderAtValue("Circle Size", 10);
            checkBindableAtValue("Circle Size", 10);
        }

        [Test]
        public void TestUserOverrideMaintainedOnBeatmapChange()
        {
            setSliderValue("Circle Size", 9);

            setBeatmapWithDifficultyParameters(2);

            checkSliderAtValue("Circle Size", 9);
            checkBindableAtValue("Circle Size", 9);
        }

        [Test]
        public void TestResetToDefault()
        {
            setBeatmapWithDifficultyParameters(2);

            setSliderValue("Circle Size", 9);
            checkSliderAtValue("Circle Size", 9);
            checkBindableAtValue("Circle Size", 9);

            resetToDefault("Circle Size");
            checkSliderAtValue("Circle Size", 2);
            checkBindableAtValue("Circle Size", null);
        }

        [Test]
        public void TestUserOverrideMaintainedOnMatchingBeatmapValue()
        {
            setBeatmapWithDifficultyParameters(3);

            checkSliderAtValue("Circle Size", 3);
            checkBindableAtValue("Circle Size", null);

            // need to initially change it away from the current beatmap value to trigger an override.
            setSliderValue("Circle Size", 4);
            setSliderValue("Circle Size", 3);

            checkSliderAtValue("Circle Size", 3);
            checkBindableAtValue("Circle Size", 3);

            setBeatmapWithDifficultyParameters(4);

            checkSliderAtValue("Circle Size", 3);
            checkBindableAtValue("Circle Size", 3);
        }

        [Test]
        public void TestResetToDefaults()
        {
            setBeatmapWithDifficultyParameters(5);

            setSliderValue("Circle Size", 3);
            setExtendedLimits(true);

            checkSliderAtValue("Circle Size", 3);
            checkBindableAtValue("Circle Size", 3);

            AddStep("reset mod settings", () => modDifficultyAdjust.ResetSettingsToDefaults());

            checkSliderAtValue("Circle Size", 5);
            checkBindableAtValue("Circle Size", null);
        }

        private void resetToDefault(string name)
        {
            AddStep($"Reset {name} to default", () =>
                this.ChildrenOfType<DifficultyAdjustSettingsControl>().First(c => c.LabelText == name)
                    .Current.SetDefault());
        }

        private void setExtendedLimits(bool status) =>
            AddStep($"Set extended limits {status}", () => modDifficultyAdjust.ExtendedLimits.Value = status);

        private void setSliderValue(string name, float value)
        {
            AddStep($"Set {name} slider to {value}", () =>
                this.ChildrenOfType<DifficultyAdjustSettingsControl>().First(c => c.LabelText == name)
                    .ChildrenOfType<SettingsSlider<float>>().First().Current.Value = value);
        }

        private void checkBindableAtValue(string name, float? expectedValue)
        {
            AddAssert($"Bindable {name} is {(expectedValue?.ToString() ?? "null")}", () =>
                this.ChildrenOfType<DifficultyAdjustSettingsControl>().First(c => c.LabelText == name)
                    .Current.Value == expectedValue);
        }

        private void checkSliderAtValue(string name, float expectedValue)
        {
            AddAssert($"Slider {name} at {expectedValue}", () =>
                this.ChildrenOfType<DifficultyAdjustSettingsControl>().First(c => c.LabelText == name)
                    .ChildrenOfType<SettingsSlider<float>>().First().Current.Value == expectedValue);
        }

        private void setBeatmapWithDifficultyParameters(float value)
        {
            AddStep($"set beatmap with all {value}", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
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
