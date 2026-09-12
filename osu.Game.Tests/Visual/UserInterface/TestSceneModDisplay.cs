// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModDisplay : SkinnableHUDComponentTestScene
    {
        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("setup mods", () =>
            {
                SelectedMods.Value = new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime { SpeedChange = { Value = 2.0 } },
                    new OsuModDifficultyAdjust(),
                    new OsuModEasy(),
                };
            });

            base.SetUpSteps();
        }

        protected override Drawable CreateDefaultImplementation()
        {
            return new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                Children = new[]
                {
                    new ModDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.AlwaysContracted
                    },
                    new ModDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.AlwaysExpanded
                    },
                    new ModDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.ExpandOnHover
                    },
                }
            };
        }

        protected override Drawable CreateLegacyImplementation()
        {
            return new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                Children = new[]
                {
                    new ModDisplay(useSkinIcons: true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.AlwaysContracted
                    },
                    new ModDisplay(useSkinIcons: true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.AlwaysExpanded
                    },
                    new ModDisplay(useSkinIcons: true)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Current = SelectedMods,
                        ExpansionMode = ExpansionMode.ExpandOnHover
                    },
                }
            };
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
