// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.KeyBinding
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
        private void load(KeyBindingStore store)
        {
            var bindings = store.Query(Ruleset?.ID, variant);

            foreach (var defaultGroup in Defaults.GroupBy(d => d.Action))
            {
                // one row per valid action.
                Add(new RestorableKeyBindingRow(defaultGroup.Key, bindings, Ruleset, defaultGroup.Select(d => d.KeyCombination)));
            }

            Add(new ResetButton
            {
                Action = () => Children.OfType<RestorableKeyBindingRow>().ForEach(k => k.KeyBindingRow.RestoreDefaults())
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
