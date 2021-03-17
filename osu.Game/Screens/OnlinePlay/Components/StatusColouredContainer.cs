// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class StatusColouredContainer : Container
    {
        private readonly double transitionDuration;

        [Resolved(typeof(Room), nameof(Room.Status))]
        private Bindable<RoomStatus> status { get; set; }

        [Resolved(typeof(Room), nameof(Room.Category))]
        private Bindable<RoomCategory> category { get; set; }

        public StatusColouredContainer(double transitionDuration = 100)
        {
            this.transitionDuration = transitionDuration;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            status.BindValueChanged(s =>
            {
                this.FadeColour(category.Value == RoomCategory.Spotlight ? colours.Pink : s.NewValue.GetAppropriateColour(colours)
                    , transitionDuration);
            }, true);
        }
    }
}
