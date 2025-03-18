// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Containers.Draggable;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players.Components
{
    public partial class TournamentsTeamBlock : OsuDraggableItemContainer<TournamentUser>
    {

        public TournamentsTeamBlock() : base()
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.None;
            IsDroppedItemRetained = true;

            Size = new Vector2(250, 250);
            var temp = new Box() { RelativeSizeAxes = Axes.Both, Colour = new Colour4(56, 126, 35, 255) };
            AddInternal(temp);
            ChangeInternalChildDepth(temp, 10.0f);
        }

        protected override FillFlowContainer<DraggableItem<TournamentUser>> CreateListFillFlowContainer() => new FillFlowContainer<DraggableItem<TournamentUser>>()
        {
            RelativeSizeAxes = Axes.Both,
            Spacing = new Vector2(5),
        };

        protected override OsuDraggableItem<TournamentUser> CreateOsuDrawable(TournamentUser model)
        {
            return new TournamentsDraggablePlayer(model);
        }

        protected override ScrollContainer<Drawable> CreateScrollContainer()
        {
            return new OsuScrollContainer()
            {
                Children =
                [
                    new Box() { RelativeSizeAxes = Axes.Both, Colour = new Colour4(255, 255, 255, 128) },
                ]
            };
        }
    }
}
