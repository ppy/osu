// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.UI
{
    public enum PlayfieldBorderStyle
    {
        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.None))]
        None,

        [LocalisableDescription(typeof(PlayfieldBorderStyleStrings), nameof(PlayfieldBorderStyleStrings.Corners))]
        Corners,

        [LocalisableDescription(typeof(PlayfieldBorderStyleStrings), nameof(PlayfieldBorderStyleStrings.Full))]
        Full
    }
}
