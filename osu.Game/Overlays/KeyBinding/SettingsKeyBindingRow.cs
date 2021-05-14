// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class SettingsKeyBindingRow : Container, IFilterable
    {
        private readonly IGrouping<object, Framework.Input.Bindings.KeyBinding> defaultGroup;
        private readonly IEnumerable<Framework.Input.Bindings.KeyBinding> bindings;
        public readonly KeyBindingRow KeyBindingRow;

        private bool matchingFilter;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                matchingFilter = value;
                this.FadeTo(!matchingFilter ? 0 : 1);
            }
        }

        public bool FilteringActive { get; set; }

        public IEnumerable<string> FilterTerms => bindings.Select(b => b.KeyCombination.ReadableString()).Prepend(defaultGroup.Key.ToString());

        public SettingsKeyBindingRow(
            IGrouping<object, Framework.Input.Bindings.KeyBinding> defaultGroup,
            IEnumerable<Framework.Input.Bindings.KeyBinding> bindings,
            RulesetInfo ruleset)
        {
            this.defaultGroup = defaultGroup;
            this.bindings = bindings;

            KeyBindingRow = new KeyBindingRow(defaultGroup.Key, bindings.Where(b => ((int)b.Action).Equals((int)defaultGroup.Key)))
            {
                AllowMainMouseButtons = ruleset != null,
                Defaults = defaultGroup.Select(d => d.KeyCombination)
            };

            RestoreDefaultValueButton<bool> restoreDefaultButton;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                restoreDefaultButton = new RestoreDefaultValueButton<bool>(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                    Child = KeyBindingRow
                },
            };

            restoreDefaultButton.Bindable = KeyBindingRow.Current;
        }
    }
}