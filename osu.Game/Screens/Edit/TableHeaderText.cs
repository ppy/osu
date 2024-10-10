// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit
{
    public partial class TableHeaderText : OsuSpriteText
    {
        public TableHeaderText(LocalisableString text)
        {
            Text = text.ToUpper();
            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold);
        }
    }
}
