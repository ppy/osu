// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class TeamDisplay : DrawableTournamentTeam
    {
        public TeamDisplay(TournamentTeam team, Color4 colour, bool flip)
            : base(team)
        {
            RelativeSizeAxes = Axes.Both;

            var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

            Anchor = Origin = anchor;

            Flag.Anchor = Flag.Origin = anchor;
            Flag.RelativeSizeAxes = Axes.None;
            Flag.Size = new Vector2(60, 40);
            Flag.Margin = new MarginPadding(20);

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Flag,
                    new TournamentSpriteText
                    {
                        Text = team?.FullName.Value.ToUpper() ?? "???",
                        X = (flip ? -1 : 1) * 90,
                        Y = -10,
                        Colour = colour,
                        Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 20),
                        Origin = anchor,
                        Anchor = anchor,
                    },
                }
            };
        }
    }
}
