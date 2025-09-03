// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModIcon : OsuManualInputManagerTestScene
    {
        private FillFlowContainer spreadOutFlow = null!;
        private ModDisplay modDisplay = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create flows", () =>
            {
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.5f),
                        new Dimension(GridSizeMode.Relative, 0.5f),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            modDisplay = new ModDisplay
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        },
                        new Drawable[]
                        {
                            spreadOutFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                            }
                        }
                    }
                };
            });
        }

        private void addRange(IEnumerable<IMod> mods)
        {
            spreadOutFlow.AddRange(mods.Select(m => new ModIcon(m)));
            modDisplay.Current.Value = modDisplay.Current.Value.Concat(mods.OfType<Mod>()).ToList();
        }

        [Test]
        public void TestShowAllMods()
        {
            createModIconsForRuleset(0);
            createModIconsForRuleset(1);
            createModIconsForRuleset(2);
            createModIconsForRuleset(3);

            AddStep("toggle selected", () =>
            {
                foreach (var icon in this.ChildrenOfType<ModIcon>())
                    icon.Selected.Toggle();
            });
        }

        private void createModIconsForRuleset(int rulesetId)
        {
            AddStep($"create mod icons for ruleset {rulesetId}", () =>
            {
                spreadOutFlow.Clear();
                modDisplay.Current.Value = [];

                addRange(rulesetStore.GetRuleset(rulesetId)!.CreateInstance().CreateAllMods().Select(m =>
                {
                    if (m is OsuModFlashlight fl)
                        fl.FollowDelay.Value = 1245;

                    if (m is OsuModDaycore dc)
                        dc.SpeedChange.Value = 0.74f;

                    if (m is OsuModDifficultyAdjust da)
                        da.CircleSize.Value = 8.2f;

                    if (m is ModAdaptiveSpeed ad)
                        ad.AdjustPitch.Value = false;

                    return m;
                }));
            });
        }

        [Test]
        public void TestShowRateAdjusts()
        {
            AddStep("create mod icons", () =>
            {
                var rateAdjustMods = Ruleset.Value.CreateInstance().CreateAllMods()
                                            .OfType<ModRateAdjust>();

                addRange(rateAdjustMods.SelectMany(m =>
                {
                    List<Mod> mods = new List<Mod> { m };

                    for (double i = m.SpeedChange.MinValue; i < m.SpeedChange.MaxValue; i += m.SpeedChange.Precision * 10)
                    {
                        m = (ModRateAdjust)m.DeepClone();
                        m.SpeedChange.Value = i;
                        mods.Add(m);
                    }

                    return mods;
                }));
            });

            AddStep("adjust rates", () =>
            {
                foreach (var icon in this.ChildrenOfType<ModIcon>())
                {
                    if (icon.Mod is ModRateAdjust rateAdjust)
                    {
                        rateAdjust.SpeedChange.Value = RNG.NextDouble() > 0.9
                            ? rateAdjust.SpeedChange.Default
                            : RNG.NextDouble(rateAdjust.SpeedChange.MinValue, rateAdjust.SpeedChange.MaxValue);
                    }
                }
            });
        }

        [Test]
        public void TestChangeModType()
        {
            AddStep("create mod icon", () => addRange([new OsuModDoubleTime()]));
            AddStep("change mod", () =>
            {
                foreach (var modIcon in this.ChildrenOfType<ModIcon>())
                    modIcon.Mod = new OsuModEasy();
            });
        }

        [Test]
        public void TestInterfaceModType()
        {
            var ruleset = new OsuRuleset();

            AddStep("create mod icon", () => addRange([ruleset.AllMods.First(m => m.Acronym == "DT")]));
            AddStep("change mod", () =>
            {
                foreach (var modIcon in this.ChildrenOfType<ModIcon>())
                    modIcon.Mod = ruleset.AllMods.First(m => m.Acronym == "EZ");
            });
        }

        [Test]
        public void TestDifficultyAdjust()
        {
            AddStep("create icons", () =>
            {
                addRange([
                    new OsuModDifficultyAdjust
                    {
                        CircleSize = { Value = 8 }
                    },
                    new OsuModDifficultyAdjust
                    {
                        CircleSize = { Value = 5.5f }
                    },
                    new OsuModDifficultyAdjust
                    {
                        CircleSize = { Value = 8 },
                        ApproachRate = { Value = 8 },
                        OverallDifficulty = { Value = 8 },
                        DrainRate = { Value = 8 },
                    }
                ]);
            });
        }

        [Test]
        public void TestTooltip()
        {
            OsuModDoubleTime mod = null!;

            AddStep("create icon", () => addRange([mod = new OsuModDoubleTime()]));
            AddStep("hover", () => InputManager.MoveMouseTo(this.ChildrenOfType<ModIcon>().First()));
            AddUntilStep("tooltip displayed", () => getTooltip()?.IsPresent, () => Is.True);
            AddAssert("tooltip text = \"Double Time\"", getTooltipText, () => Is.EqualTo("Double Time"));
            AddAssert("tooltip settings empty", () => getTooltipSettingsLabels().Concat(getTooltipSettingsValues()), () => Is.Empty);

            AddStep("change settings", () => mod.SpeedChange.Value = 1.75f);
            AddAssert("tooltip text = \"Double Time\"", getTooltipText, () => Is.EqualTo("Double Time"));
            AddAssert("tooltip settings updated",
                () => getTooltipSettingsLabels().Concat(getTooltipSettingsValues()),
                () => Is.EquivalentTo(new[] { "Speed ", "change", "1.75x" }));

            AddStep("change settings", () => mod.SpeedChange.Value = 1.25f);
            AddAssert("tooltip text = \"Double Time\"", getTooltipText, () => Is.EqualTo("Double Time"));
            AddAssert("tooltip settings updated",
                () => getTooltipSettingsLabels().Concat(getTooltipSettingsValues()),
                () => Is.EquivalentTo(new[] { "Speed ", "change", "1.25x" }));

            AddStep("rest settings", () => mod.SpeedChange.SetDefault());
            AddAssert("tooltip text = \"Double Time\"", getTooltipText, () => Is.EqualTo("Double Time"));
            AddAssert("tooltip settings empty", () => getTooltipSettingsLabels().Concat(getTooltipSettingsValues()), () => Is.Empty);

            ModTooltip? getTooltip() => this.ChildrenOfType<ModTooltip>().SingleOrDefault();

            // we could also just expose those directly from ModTooltip, but this works.
            string getTooltipText() => getTooltip().ChildrenOfType<SpriteText>().First().Text.ToString();
            IEnumerable<string> getTooltipSettingsLabels() => getTooltip().ChildrenOfType<TextFlowContainer>().First().ChildrenOfType<SpriteText>().Select(t => t.Text.ToString());
            IEnumerable<string> getTooltipSettingsValues() => getTooltip().ChildrenOfType<TextFlowContainer>().Last().ChildrenOfType<SpriteText>().Select(t => t.Text.ToString());
        }
    }
}
