// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using OpenTK;
using osu.Framework.Input.States;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match.Settings
{
    public class RoomAvailabilityPicker : TabControl<RoomAvailability>
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

        private class RoomAvailabilityPickerItem : TabItem<RoomAvailability>
        {
            private const float transition_duration = 200;

            private readonly Box hover, selection;

            public RoomAvailabilityPickerItem(RoomAvailability value) : base(value)
            {
                RelativeSizeAxes = Axes.Y;
                Width = 120;
                Masking = true;
                CornerRadius = 5;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"3d3943"),
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
                        Font = @"Exo2.0-Bold",
                        Text = value.GetDescription(),
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                selection.Colour = colours.GreenLight;
            }

            protected override bool OnHover(InputState state)
            {
                hover.FadeTo(0.05f, transition_duration, Easing.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                hover.FadeOut(transition_duration, Easing.OutQuint);
                base.OnHoverLost(state);
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
