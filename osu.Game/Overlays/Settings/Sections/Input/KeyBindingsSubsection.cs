// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osuTK;
using Realms;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public abstract partial class KeyBindingsSubsection : SettingsSubsection
    {
        /// <summary>
        /// After a successful binding, automatically select the next binding row to make quickly
        /// binding a large set of keys easier on the user.
        /// </summary>
        protected virtual bool AutoAdvanceTarget => false;

        protected IEnumerable<KeyBinding> Defaults { get; init; } = Array.Empty<KeyBinding>();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        protected KeyBindingsSubsection()
        {
            FlowContent.Spacing = new Vector2(0, 3);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var bindings = realm.Run(r => GetKeyBindings(r).Detach());

            foreach (var defaultGroup in Defaults.GroupBy(d => d.Action))
            {
                int intKey = (int)defaultGroup.Key;

                // one row per valid action.
                var row = CreateKeyBindingRow(defaultGroup.Key, defaultGroup)
                    .With(row => row.BindingUpdated = onBindingUpdated);
                row.KeyBindings.AddRange(bindings.Where(b => b.ActionInt.Equals(intKey)));
                Add(row);
            }

            Add(new ResetButton
            {
                Action = () => Children.OfType<KeyBindingRow>().ForEach(k => k.RestoreDefaults())
            });
        }

        protected abstract IEnumerable<RealmKeyBinding> GetKeyBindings(Realm realm);

        protected virtual KeyBindingRow CreateKeyBindingRow(object action, IEnumerable<KeyBinding> defaults)
            => new KeyBindingRow(action)
            {
                AllowMainMouseButtons = false,
                Defaults = defaults.Select(d => d.KeyCombination),
            };

        private void onBindingUpdated(KeyBindingRow sender, KeyBindingRow.KeyBindingUpdatedEventArgs args)
        {
            realm.WriteAsync(r => r.Find<RealmKeyBinding>(args.KeyBindingID)!.KeyCombinationString = args.KeyCombinationString);

            if (AutoAdvanceTarget)
            {
                var next = Children.SkipWhile(c => c != sender).Skip(1).FirstOrDefault();
                if (next != null)
                    GetContainingInputManager().ChangeFocus(next);
            }
        }
    }

    public partial class ResetButton : DangerousSettingsButton
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
        public override IEnumerable<LocalisableString> FilterTerms => Enumerable.Empty<LocalisableString>();
    }
}
