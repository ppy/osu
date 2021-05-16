// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class RestorableKeyBindingRow : Container, IFilterable
    {
        private readonly object key;
        private readonly ICollection<Input.Bindings.DatabasedKeyBinding> bindings;
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

        public IEnumerable<string> FilterTerms => bindings.Select(b => b.KeyCombination.ReadableString()).Prepend(key.ToString());

        public RestorableKeyBindingRow(
            object key,
            ICollection<Input.Bindings.DatabasedKeyBinding> bindings,
            RulesetInfo ruleset,
            IEnumerable<KeyCombination> defaults)
        {
            this.key = key;
            this.bindings = bindings;

            RestoreDefaultValueButton<bool> restoreDefaultButton;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            KeyBindingRow = new KeyBindingRow(key, bindings.Where(b => ((int)b.Action).Equals((int)key)))
            {
                AllowMainMouseButtons = ruleset != null,
                Defaults = defaults
            };

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

            restoreDefaultButton.Current = KeyBindingRow.Current;
        }
    }
}