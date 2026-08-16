// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardRankLabel : Container, IHasTooltip
    {
        private readonly bool darkText;
        private readonly OsuSpriteText text;

        public LeaderboardRankLabel(int? rank, bool sheared, bool darkText)
        {
            this.darkText = darkText;
            if (rank >= 1000)
                TooltipText = $"#{rank:N0}";

            Child = text = new OsuSpriteText
            {
                Shear = sheared ? -OsuGame.SHEAR : Vector2.Zero,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Style.Heading2,
                Text = rank?.FormatRank().Insert(0, "#") ?? "-",
                Shadow = !darkText,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            text.Colour = darkText ? colourProvider.Background3 : colourProvider.Content1;
        }

        public LocalisableString TooltipText { get; }
    }
}
