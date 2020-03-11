// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public class RoundDisplay : CompositeDrawable
    {
        public RoundDisplay(TournamentMatch match)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new DrawableTournamentTitleText(),
                        new TournamentSpriteText
                        {
                            Text = match.Round.Value?.Name.Value ?? "Unknown Round",
                            Font = OsuFont.Torus.With(size: 26, weight: FontWeight.SemiBold)
                        },
                    }
                }
            };
        }
    }
}
