// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public partial class RoomAvailabilityPicker : DisableableTabControl<RoomAvailability>
    {
        protected override TabItem<RoomAvailability> CreateTabItem(RoomAvailability value) => new RoomAvailabilityPickerItem(value);
        protected override Dropdown<RoomAvailability> CreateDropdown() => null;

        public RoomAvailabilityPicker()
        {
            RelativeSizeAxes = Axes.X;
            Height = 35;

            TabContainer.Spacing = new Vector2(10);

            AddItem(RoomAvailability.Public);
            AddItem(RoomAvailability.FriendsOnly);
            AddItem(RoomAvailability.InviteOnly);
        }

        private partial class RoomAvailabilityPickerItem : DisableableTabItem
        {
            private const float transition_duration = 200;

            private readonly Box hover, selection;

            public RoomAvailabilityPickerItem(RoomAvailability value)
                : base(value)
            {
                RelativeSizeAxes = Axes.Y;
                Width = 102;
                Masking = true;
                CornerRadius = 5;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex(@"3d3943"),
                    },
                    selection = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                    hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = 0,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        Text = value.GetDescription(),
                    },
                    new HoverSounds(),
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                selection.Colour = colours.GreenLight;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hover.FadeTo(0.05f, transition_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hover.FadeOut(transition_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override void OnActivated()
            {
                selection.FadeIn(transition_duration, Easing.OutQuint);
            }

            protected override void OnDeactivated()
            {
                selection.FadeOut(transition_duration, Easing.OutQuint);
            }
        }
    }
}
