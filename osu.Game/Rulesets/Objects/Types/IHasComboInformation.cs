// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that is part of a combo and has extended information about its position relative to other combo objects.
    /// </summary>
    public interface IHasComboInformation : IHasCombo
    {
        Bindable<int> IndexInCurrentComboBindable { get; }

        /// <summary>
        /// The offset of this hitobject in the current combo.
        /// </summary>
        int IndexInCurrentCombo { get; set; }

        Bindable<int> ComboIndexBindable { get; }

        /// <summary>
        /// The offset of this combo in relation to the beatmap.
        /// </summary>
        int ComboIndex { get; set; }

        Bindable<bool> LastInComboBindable { get; }

        /// <summary>
        /// Whether this is the last object in the current combo.
        /// </summary>
        bool LastInCombo { get; set; }
    }
}
