// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Scoring.Drawables
{
    /// <summary>
    /// A placeholder used in PP columns for scores that do not award PP.
    /// </summary>
    public partial class UnrankedPerformancePointsPlaceholder : SpriteText, IHasTooltip
    {
        public LocalisableString TooltipText => "pp is not awarded for this score"; // todo: replace with localised string ScoresStrings.StatusNoPp.

        public UnrankedPerformancePointsPlaceholder()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Text = "-";
        }
    }
}
