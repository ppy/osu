using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
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
