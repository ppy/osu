// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Localisation;
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

            FlowContent.Spacing = new Vector2(0, 3);
        }

        [BackgroundDependencyLoader]
        private void load(RealmContextFactory realmFactory)
        {
            int? rulesetId = Ruleset?.ID;

            List<RealmKeyBinding> bindings;

            using (var realm = realmFactory.CreateContext())
                bindings = realm.All<RealmKeyBinding>().Where(b => b.RulesetID == rulesetId && b.Variant == variant).Detach();

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

    public class ResetButton : DangerousSettingsButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = InputSettingsStrings.ResetSectionButton;
            RelativeSizeAxes = Axes.X;
            Width = 0.8f;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Margin = new MarginPadding { Top = 15 };
            Height = 30;

            Content.CornerRadius = 5;
        }

        // Empty FilterTerms so that the ResetButton is visible only when the whole subsection is visible.
        public override IEnumerable<string> FilterTerms => Enumerable.Empty<string>();
    }
}
