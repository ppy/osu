// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class KeyBindingRow : IHasPopover
    {
        private KeyBindingConflictInfo? pendingKeyBindingConflict;

        public Popover GetPopover()
        {
            Debug.Assert(pendingKeyBindingConflict != null);
            return new KeyBindingConflictPopover(pendingKeyBindingConflict)
            {
                BindingConflictResolved = () => BindingUpdated?.Invoke(this, new KeyBindingUpdatedEventArgs(bindingConflictResolved: true, canAdvanceToNextBinding: false))
            };
        }

        private void showBindingConflictPopover(KeyBindingConflictInfo conflictInfo)
        {
            pendingKeyBindingConflict = conflictInfo;
            this.ShowPopover();
        }

        /// <summary>
        /// Contains information about the key binding conflict to be resolved.
        /// </summary>
        public class KeyBindingConflictInfo
        {
            public ConflictingKeyBinding Existing { get; }
            public ConflictingKeyBinding New { get; }

            /// <summary>
            /// Contains information about the key binding conflict to be resolved.
            /// </summary>
            public KeyBindingConflictInfo(ConflictingKeyBinding existingBinding, ConflictingKeyBinding newBinding)
            {
                Existing = existingBinding;
                New = newBinding;
            }
        }

        public class ConflictingKeyBinding
        {
            public Guid ID { get; }
            public object Action { get; }
            public KeyCombination CombinationWhenChosen { get; }
            public KeyCombination CombinationWhenNotChosen { get; }

            public ConflictingKeyBinding(Guid id, object action, KeyCombination combinationWhenChosen, KeyCombination combinationWhenNotChosen)
            {
                ID = id;
                Action = action;
                CombinationWhenChosen = combinationWhenChosen;
                CombinationWhenNotChosen = combinationWhenNotChosen;
            }
        }

        public class KeyBindingUpdatedEventArgs
        {
            public bool BindingConflictResolved { get; }
            public bool CanAdvanceToNextBinding { get; }

            public KeyBindingUpdatedEventArgs(bool bindingConflictResolved, bool canAdvanceToNextBinding)
            {
                BindingConflictResolved = bindingConflictResolved;
                CanAdvanceToNextBinding = canAdvanceToNextBinding;
            }
        }
    }
}
