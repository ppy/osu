// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.BeatmapSet;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneLeaderboardModSelector : OsuTestScene
    {
        public TestSceneLeaderboardModSelector()
        {
            LeaderboardModSelector modSelector;
            FillFlowContainer<SpriteText> selectedMods;

            var ruleset = new Bindable<IRulesetInfo>();

            Add(selectedMods = new FillFlowContainer<SpriteText>
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

            modSelector.SelectedMods.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        args.NewItems.AsNonNull().Cast<Mod>().ForEach(mod => selectedMods.Add(new OsuSpriteText
                        {
                            Text = mod.Acronym,
                        }));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        args.OldItems.AsNonNull().Cast<Mod>().ForEach(mod =>
                        {
                            foreach (var selected in selectedMods)
                            {
                                if (selected.Text == mod.Acronym)
                                {
                                    selectedMods.Remove(selected);
                                    break;
                                }
                            }
                        });
                        break;
                }
            };

            AddStep("osu ruleset", () => ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("mania ruleset", () => ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddStep("taiko ruleset", () => ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("catch ruleset", () => ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("Deselect all", () => modSelector.DeselectAll());
            AddStep("null ruleset", () => ruleset.Value = null);
        }
    }
}
