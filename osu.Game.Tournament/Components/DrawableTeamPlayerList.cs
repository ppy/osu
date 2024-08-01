// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamPlayerList : FillFlowContainer
    {
        private int totalHeight;
        private const int entryheight = 50;
        private const int spacing = 10;
        public DrawableTeamPlayerList(TournamentTeam? team)
        {
            var players = team?.Players ?? new BindableList<TournamentUser>();

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.None;
            Width = 300;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(spacing);
            totalHeight = players.Count * (entryheight + spacing) + spacing;
            // We need to provide all children upon definition of a widget,
            // Since it's impossible to change its height after that.
            ChildrenEnumerable = players.Select(createCard);
        }
        private UserListPanel createCard(TournamentUser user) => new UserListPanel(user.ToAPIUser())
        {
            RelativeSizeAxes = Axes.None,
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Width = 300,
            Height = entryheight,
            Scale = new Vector2(1f),
        };

        public int GetHeight() => totalHeight;
    }
}
