// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public partial class VideoIconPill : IconPill
    {
        public VideoIconPill()
            : base(FontAwesome.Solid.Film)
        {
        }

        public override LocalisableString TooltipText => BeatmapsetsStrings.ShowInfoVideo;
    }
}
