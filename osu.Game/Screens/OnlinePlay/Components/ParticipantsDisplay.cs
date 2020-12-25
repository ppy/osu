// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class ParticipantsDisplay : OnlinePlayComposite
    {
        public Bindable<string> Details = new Bindable<string>();

        public ParticipantsDisplay(Direction direction)
        {
            OsuScrollContainer scroll;
            ParticipantsList list;

            AddInternal(scroll = new OsuScrollContainer(direction)
            {
                Child = list = new ParticipantsList()
            });

            switch (direction)
            {
                case Direction.Horizontal:
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;

                    scroll.RelativeSizeAxes = Axes.X;
                    scroll.Height = ParticipantsList.TILE_SIZE + OsuScrollContainer.SCROLL_BAR_HEIGHT + OsuScrollContainer.SCROLL_BAR_PADDING * 2;

                    list.RelativeSizeAxes = Axes.Y;
                    list.AutoSizeAxes = Axes.X;
                    break;

                case Direction.Vertical:
                    RelativeSizeAxes = Axes.Both;

                    scroll.RelativeSizeAxes = Axes.Both;

                    list.RelativeSizeAxes = Axes.X;
                    list.AutoSizeAxes = Axes.Y;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ParticipantCount.BindValueChanged(_ => setParticipantCount());
            MaxParticipants.BindValueChanged(_ => setParticipantCount(), true);
        }

        private void setParticipantCount() =>
            Details.Value = MaxParticipants.Value != null ? $"{ParticipantCount.Value}/{MaxParticipants.Value}" : ParticipantCount.Value.ToString();
    }
}
