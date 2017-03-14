// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that is part of a combo.
    /// </summary>
    public interface IHasCombo
    {
        /// <summary>
        /// The colour of this HitObject in the combo.
        /// </summary>
        Color4 ComboColour { get; }

        /// <summary>
        /// Whether the HitObject starts a new combo.
        /// </summary>
        bool NewCombo { get; }

        /// <summary>
        /// The combo index.
        /// </summary>
        int ComboIndex { get; }
    }
}
