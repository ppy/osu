// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;
using osu.Game.Users;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamPlayerList : CompositeDrawable
    {
        private FillFlowContainer playerContainer = null!;
        public DrawableTeamPlayerList(TournamentTeam? team)
        {
            AutoSizeAxes = Axes.Both;

            var players = team?.Players ?? new BindableList<TournamentUser>();

            InternalChildren = new Drawable[]
            {
                playerContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 300,
                    // Height = 300,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                },
            };

            foreach (TournamentUser p in players)
            {
                playerContainer.Add(new UserListPanel(p.ToAPIUser())
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = 50,
                    Scale = new Vector2(1f),
                });
            }
        }
    }
}
