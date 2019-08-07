// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.BeatmapSet;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneLeaderboardModSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LeaderboardModSelector),
        };

        public TestSceneLeaderboardModSelector()
        {
            LeaderboardModSelector modSelector;
            FillFlowContainer selectedMods;
            Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

            Add(selectedMods = new FillFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            });

            Add(modSelector = new LeaderboardModSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Ruleset = { BindTarget = ruleset }
            });

            modSelector.SelectedMods.BindValueChanged(mods =>
            {
                selectedMods.Clear();

                foreach (var mod in mods.NewValue)
                    selectedMods.Add(new SpriteText
                    {
                        Text = mod.Acronym,
                    });
            });

            AddStep("osu mods", () => ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("mania mods", () => ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddStep("taiko mods", () => ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("catch mods", () => ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("Deselect all", () => modSelector.DeselectAll());
        }
    }
}
