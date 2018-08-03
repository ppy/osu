// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class RoomAvailabilityTabControl : OsuTabControl<RoomAvailability>
    {
        protected override TabItem<RoomAvailability> CreateTabItem(RoomAvailability value) => new AvailabilityTabItem(value);

        private class AvailabilityTabItem : OsuTabItem
        {
            private readonly Box bg;

            private OsuColour colours;

            public AvailabilityTabItem(RoomAvailability value)
                : base(value)
            {
                AutoSizeAxes = Axes.None;
                CornerRadius = 7;
                Masking = true;
                Width = 110;

                Children = new Drawable[]
                {
                    bg = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Text = value.GetDescription(),
                        Colour = Color4.White,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = @"Exo2.0-Bold",
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                this.colours = colours;

                bg.Colour = Active.Value ? colours.Green : colours.Gray4;
            }

            protected override bool OnHover(InputState state)
            {
                bg.FadeTo(0.8f, 200);

                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                bg.FadeTo(1f, 200);

                base.OnHoverLost(state);
            }

            private void fadeActive()
            {
                bg.FadeColour(colours.Green, 150, Easing.InSine);
            }

            private void fadeInactive()
            {
                bg.FadeColour(colours.Gray4, 150, Easing.InSine);
            }

            protected override void OnActivated()
            {
                fadeActive();

                base.OnActivated();
            }

            protected override void OnDeactivated()
            {
                fadeInactive();

                base.OnDeactivated();
            }
        }
    }
}
