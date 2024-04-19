// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModDifficultyAdjustSettings : OsuManualInputManagerTestScene
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
            AddStep("set override cs to 12", () => modDifficultyAdjust.CircleSize.Value = 12);

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);

            // this is a no-op, just showing that it won't reset the value during deserialisation.
            setExtendedLimits(false);

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);

            // setting extended limits will reset the serialisation exception.
            // this should be fine as the goal is to allow, at most, the value of extended limits.
            setExtendedLimits(true);

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);
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

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);

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
        public void TestExtendedLimitsRetainedAfterBoundCopyCreation()
        {
            setExtendedLimits(true);
            setSliderValue("Circle Size", 12);

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);

            AddStep("create bound copy", () => _ = modDifficultyAdjust.CircleSize.GetBoundCopy());

            checkSliderAtValue("Circle Size", 12);
            checkBindableAtValue("Circle Size", 12);
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

        [Test]
        public void TestModSettingChangeTracker()
        {
            ModSettingChangeTracker tracker = null;
            Queue<Mod> settingsChangedQueue = null;

            setBeatmapWithDifficultyParameters(5);

            AddStep("add mod settings change tracker", () =>
            {
                settingsChangedQueue = new Queue<Mod>();

                tracker = new ModSettingChangeTracker(modDifficultyAdjust.Yield())
                {
                    SettingChanged = settingsChangedQueue.Enqueue
                };
            });

            AddAssert("no settings changed", () => settingsChangedQueue.Count == 0);

            setSliderValue("Circle Size", 3);

            settingsChangedFired();

            setSliderValue("Circle Size", 5);
            checkBindableAtValue("Circle Size", 5);

            settingsChangedFired();

            AddStep("reset mod settings", () => modDifficultyAdjust.CircleSize.SetDefault());
            checkBindableAtValue("Circle Size", null);

            settingsChangedFired();

            setExtendedLimits(true);

            settingsChangedFired();

            AddStep("dispose tracker", () =>
            {
                tracker.Dispose();
                tracker = null;
            });

            void settingsChangedFired()
            {
                AddAssert("setting changed event fired", () =>
                {
                    settingsChangedQueue.Dequeue();
                    return settingsChangedQueue.Count == 0;
                });
            }
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
                    .ChildrenOfType<RoundedSliderBar<float>>().First().Current.Value = value);
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
                    .ChildrenOfType<RoundedSliderBar<float>>().First().Current.Value == expectedValue);
        }

        private void setBeatmapWithDifficultyParameters(float value)
        {
            AddStep($"set beatmap with all {value}", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
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
