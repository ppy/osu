// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public abstract class KeyBindingsSubsection : SettingsSubsection
    {
        protected IEnumerable<Framework.Input.Bindings.KeyBinding> Defaults;

        protected RulesetInfo Ruleset;

        private readonly int? variant;

        protected KeyBindingsSubsection(int? variant)
        {
            this.variant = variant;

            FlowContent.Spacing = new Vector2(0, 1);
            FlowContent.Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS };
        }

        [BackgroundDependencyLoader]
        private void load(RealmContextFactory realmFactory)
        {
            var rulesetId = Ruleset?.ID;

            List<RealmKeyBinding> bindings;

            using (var usage = realmFactory.GetForRead())
                bindings = usage.Realm.All<RealmKeyBinding>().Where(b => b.RulesetID == rulesetId && b.Variant == variant).Detach();

            foreach (var defaultGroup in Defaults.GroupBy(d => d.Action))
            {
                int intKey = (int)defaultGroup.Key;

                // one row per valid action.
                Add(new KeyBindingRow(defaultGroup.Key, bindings.Where(b => b.ActionInt.Equals(intKey)).ToList())
                {
                    AllowMainMouseButtons = Ruleset != null,
                    Defaults = defaultGroup.Select(d => d.KeyCombination)
                });
            }

            Add(new ResetButton
            {
                Action = () => Children.OfType<KeyBindingRow>().ForEach(k => k.RestoreDefaults())
            });
        }
    }

    public class ResetButton : DangerousTriangleButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Reset all bindings in section";
            RelativeSizeAxes = Axes.X;
            Width = 0.5f;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Margin = new MarginPadding { Top = 15 };
            Height = 30;

            Content.CornerRadius = 5;
        }
    }
}
