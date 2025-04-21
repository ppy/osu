// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// this file is meant for testing purposes when BindableColour4 is not availiable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum BackgroundColour
    {
        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Red))]
        Red,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Orange))]
        Orange,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Yellow))]
        Yellow,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Lime))]
        Lime,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Green))]
        Green,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Cyan))]
        Cyan,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.LightBlue))]
        LightBlue,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Blue))]
        Blue,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Purple))]
        Purple,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Magenta))]
        Magenta,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Pink))]
        Pink,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.White))]
        White,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.LightGrey))]
        LightGrey,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Grey))]
        Grey,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Black))]
        Black,

        [LocalisableDescription(typeof(ColourStrings), nameof(ColourStrings.Brown))]
        Brown,
    }
}
