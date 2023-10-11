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
            var bindings = getAllBindings();

            foreach (var defaultGroup in Defaults.GroupBy(d => d.Action))
            {
                int intKey = (int)defaultGroup.Key;

                var row = CreateKeyBindingRow(defaultGroup.Key, defaultGroup)
                    .With(row =>
                    {
                        row.BindingUpdated = onBindingUpdated;
                        row.BindingConflictResolved = reloadAllBindings;
                    });
                row.KeyBindings.AddRange(bindings.Where(b => b.ActionInt.Equals(intKey)));
                Add(row);
            }

            Add(new ResetButton
            {
                Action = () => Children.OfType<KeyBindingRow>().ForEach(k => k.RestoreDefaults())
            });
        }

        protected abstract IEnumerable<RealmKeyBinding> GetKeyBindings(Realm realm);

        private List<RealmKeyBinding> getAllBindings() => realm.Run(r => GetKeyBindings(r).Detach());

        protected virtual KeyBindingRow CreateKeyBindingRow(object action, IEnumerable<KeyBinding> defaults)
            => new KeyBindingRow(action)
            {
                AllowMainMouseButtons = false,
                Defaults = defaults.Select(d => d.KeyCombination),
            };

        private void reloadAllBindings()
        {
            var bindings = getAllBindings();

            foreach (var row in Children.OfType<KeyBindingRow>())
            {
                row.KeyBindings.Clear();
                row.KeyBindings.AddRange(bindings.Where(b => b.ActionInt.Equals((int)row.Action)));
            }
        }

        private void onBindingUpdated(KeyBindingRow sender, KeyBindingRow.KeyBindingUpdatedEventArgs args)
        {
            var bindings = getAllBindings();
            var existingBinding = args.KeyCombination.Equals(new KeyCombination(InputKey.None))
                ? null
                : bindings.FirstOrDefault(kb => kb.ID != args.KeyBindingID && kb.KeyCombination.Equals(args.KeyCombination));

            if (existingBinding != null)
            {
                // `RealmKeyBinding`'s  `Action` is just an int, always.
                // we need more than that for proper display, so leverage `Defaults` (which have the correct enum-typed object in `Action` inside).
                object existingAssignedAction = Defaults.First(binding => (int)binding.Action == existingBinding.ActionInt).Action;
                var bindingBeforeUpdate = bindings.Single(binding => binding.ID == args.KeyBindingID);

                sender.ShowBindingConflictPopover(
                    new KeyBindingConflictInfo(
                        new ConflictingKeyBinding(existingBinding.ID, existingAssignedAction, existingBinding.KeyCombination, new KeyCombination(InputKey.None)),
                        new ConflictingKeyBinding(bindingBeforeUpdate.ID, args.Action, args.KeyCombination, bindingBeforeUpdate.KeyCombination)));

                return;
            }

            realm.WriteAsync(r => r.Find<RealmKeyBinding>(args.KeyBindingID)!.KeyCombinationString = args.KeyCombination.ToString());

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
