// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using OpenTK;
using osu.Game.Graphics;

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
        }

        [BackgroundDependencyLoader]
        private void load(KeyBindingStore store)
        {
            var bindings = store.Query(Ruleset?.ID, variant);

            foreach (var defaultGroup in Defaults.GroupBy(d => d.Action))
            {
                int intKey = (int)defaultGroup.Key;

                // one row per valid action.
                Add(new KeyBindingRow(defaultGroup.Key, bindings.Where(b => ((int)b.Action).Equals(intKey)))
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

    internal class ResetButton : OsuButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Text = "Reset";
            RelativeSizeAxes = Axes.X;
            Margin = new MarginPadding { Top = 5 };
            Height = 20;

            Content.CornerRadius = 5;

            BackgroundColour = colours.PinkDark;
            Triangles.ColourDark = colours.PinkDarker;
            Triangles.ColourLight = colours.Pink;
        }
    }
}
