// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamWithPlayers : CompositeDrawable
    {
        public DrawableTeamWithPlayers(TournamentTeam? team, TeamColour colour)
        {
            AutoSizeAxes = Axes.Both;

            var players = team?.Players ?? new BindableList<TournamentUser>();

            // split the players into two even columns, favouring the first column if odd.
            int split = (int)Math.Ceiling(players.Count / 2f);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(30),
                    Children = new Drawable[]
                    {
                        new DrawableTeamTitleWithHeader(team, colour),
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = 10 },
                            Spacing = new Vector2(30),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    AutoSizeAxes = Axes.Both,
                                    ChildrenEnumerable = players.Take(split).Select(createPlayerText),
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    AutoSizeAxes = Axes.Both,
                                    ChildrenEnumerable = players.Skip(split).Select(createPlayerText),
                                },
                            }
                        },
                    }
                },
            };

            TournamentSpriteText createPlayerText(TournamentUser p) =>
                new TournamentSpriteText
                {
                    Text = p.Username,
                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                    Colour = Color4.White,
                };
        }
    }
}
