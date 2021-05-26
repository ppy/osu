// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osuTK.Graphics;

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

        /// <summary>
        /// Whether the HitObject starts a new combo.
        /// </summary>
        new bool NewCombo { get; set; }

        Bindable<bool> LastInComboBindable { get; }

        /// <summary>
        /// Whether this is the last object in the current combo.
        /// </summary>
        bool LastInCombo { get; set; }

        /// <summary>
        /// Retrieves the colour of the combo described by this <see cref="IHasComboInformation"/> object from a set of possible combo colours.
        /// Defaults to using <see cref="ComboIndex"/> to decide the colour.
        /// </summary>
        /// <param name="comboColours">A list of possible combo colours provided by the beatmap or skin.</param>
        /// <returns>The colour of the combo described by this <see cref="IHasComboInformation"/> object.</returns>
        Color4 GetComboColour([NotNull] IReadOnlyList<Color4> comboColours) => comboColours.Count > 0 ? comboColours[ComboIndex % comboColours.Count] : Color4.White;
    }
}
