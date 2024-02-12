// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Scoring.Drawables
{
    /// <summary>
    /// A placeholder used in PP columns for scores that do not award PP due to a reason specified by <see cref="TooltipText"/>.
    /// </summary>
    public partial class UnrankedPerformancePointsPlaceholder : SpriteText, IHasTooltip
    {
        public LocalisableString TooltipText { get; }

        public UnrankedPerformancePointsPlaceholder(LocalisableString tooltipText)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Text = "-";
            TooltipText = tooltipText;
        }
    }
}
