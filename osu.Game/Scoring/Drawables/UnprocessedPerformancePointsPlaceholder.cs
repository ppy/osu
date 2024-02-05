// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Scoring.Drawables
{
    /// <summary>
    /// A placeholder used in PP columns for scores with unprocessed PP value.
    /// </summary>
    public partial class UnprocessedPerformancePointsPlaceholder : SpriteIcon, IHasTooltip
    {
        public LocalisableString TooltipText => ScoresStrings.StatusProcessing;

        public UnprocessedPerformancePointsPlaceholder()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Icon = FontAwesome.Solid.Sync;
        }
    }
}
