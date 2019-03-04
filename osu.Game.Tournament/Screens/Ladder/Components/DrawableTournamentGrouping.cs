// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableTournamentGrouping : CompositeDrawable
    {
        public DrawableTournamentGrouping(TournamentGrouping grouping, bool losers = false)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = grouping.Description.Value.ToUpper(),
                        Colour = Color4.Black,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                    new OsuSpriteText
                    {
                        Text = ((losers ? "Losers " : "") + grouping.Name).ToUpper(),
                        Font = "Exo2.0-Bold",
                        Colour = Color4.Black,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                }
            };
        }
    }
}
