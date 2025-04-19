// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers.Draggable;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players.Components
{
    /// <summary>
    /// This is a temp class for testing.
    /// </summary>
    public partial class TournamentsDraggablePlayerCopy : OsuDraggableItem<TournamentUser>
    {
        public TournamentsDraggablePlayerCopy(TournamentUser item)
            : base(item)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.None;
            Size = new Vector2(80, 50);
            AddInternal(new Box { RelativeSizeAxes = Axes.Both, Colour = new Colour4(0, 255, 255, 255) });
            AddInternal(new OsuSpriteText { RelativeSizeAxes = Axes.Both, Text = Model.OnlineID.ToString(), Colour = new Colour4(34, 34, 34, 255) });
        }
    }
}
