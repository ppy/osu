// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Bindings;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    /// <summary>
    /// Contains information about the key binding conflict to be resolved.
    /// </summary>
    public record KeyBindingConflictInfo(ConflictingKeyBinding Existing, ConflictingKeyBinding New);

    public record ConflictingKeyBinding(Guid ID, object Action, KeyCombination CombinationWhenChosen, KeyCombination CombinationWhenNotChosen);
}
