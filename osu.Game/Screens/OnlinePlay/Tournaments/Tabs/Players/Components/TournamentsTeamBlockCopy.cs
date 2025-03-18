// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers.Draggable;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players.Components
{
    public partial class TournamentsTeamBlockCopy : TournamentsTeamBlock
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            IsSharedItemRetained = true;
            IsDroppedItemRetained = true;
            Size = new Vector2(1);
        }

        protected override OsuDraggableItem<TournamentUser> CreateOsuDrawable(TournamentUser model)
        {
            return new TournamentsDraggablePlayerCopy(model);
        }
    }
}
