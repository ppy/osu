// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Multi.Components
{
    public class OverlinedParticipants : OverlinedDisplay
    {
        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        public OverlinedParticipants(Direction direction)
            : base("Participants")
        {
            OsuScrollContainer scroll;
            ParticipantsList list;

            Content.Add(scroll = new OsuScrollContainer(direction)
            {
                Child = list = new ParticipantsList()
            });

            switch (direction)
            {
                case Direction.Horizontal:
                    scroll.RelativeSizeAxes = Axes.X;
                    scroll.Height = ParticipantsList.TILE_SIZE + OsuScrollContainer.SCROLL_BAR_HEIGHT + OsuScrollContainer.SCROLL_BAR_PADDING * 2;
                    list.AutoSizeAxes = Axes.Both;
                    break;

                case Direction.Vertical:
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
            MaxParticipants.BindValueChanged(_ => setParticipantCount());

            setParticipantCount();
        }

        private void setParticipantCount() => Details = MaxParticipants.Value != null ? $"{ParticipantCount.Value}/{MaxParticipants.Value}" : ParticipantCount.Value.ToString();
    }
}
