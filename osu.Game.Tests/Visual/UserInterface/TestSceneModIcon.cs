// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModIcon : OsuTestScene
    {
        [Test]
        public void TestShowAllMods()
        {
            AddStep("create mod icons", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Full,
                    ChildrenEnumerable = Ruleset.Value.CreateInstance().CreateAllMods().Select(m => new ModIcon(m)),
                };
            });

            AddStep("toggle selected", () =>
            {
                foreach (var icon in this.ChildrenOfType<ModIcon>())
                    icon.Selected.Toggle();
            });
        }

        [Test]
        public void TestShowRateAdjusts()
        {
            AddStep("create mod icons", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Full,
                    ChildrenEnumerable = Ruleset.Value.CreateInstance().CreateAllMods()
                                                .OfType<ModRateAdjust>()
                                                .SelectMany(m =>
                                                {
                                                    List<ModIcon> icons = new List<ModIcon> { new ModIcon(m) };

                                                    for (double i = m.SpeedChange.MinValue; i < m.SpeedChange.MaxValue; i += m.SpeedChange.Precision * 10)
                                                    {
                                                        m = (ModRateAdjust)m.DeepClone();
                                                        m.SpeedChange.Value = i;
                                                        icons.Add(new ModIcon(m));
                                                    }

                                                    return icons;
                                                }),
                };
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
            ModIcon icon = null!;

            AddStep("create mod icon", () => Child = icon = new ModIcon(new OsuModDoubleTime()));
            AddStep("change mod", () => icon.Mod = new OsuModEasy());
        }

        [Test]
        public void TestInterfaceModType()
        {
            ModIcon icon = null!;

            var ruleset = new OsuRuleset();

            AddStep("create mod icon", () => Child = icon = new ModIcon(ruleset.AllMods.First(m => m.Acronym == "DT")));
            AddStep("change mod", () => icon.Mod = ruleset.AllMods.First(m => m.Acronym == "EZ"));
        }
    }
}
