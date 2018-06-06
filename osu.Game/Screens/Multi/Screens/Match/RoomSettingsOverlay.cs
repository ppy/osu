// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class RoomSettingsOverlay : OsuFocusedOverlayContainer
    {
        private const float transition_duration = 500;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public RoomSettingsOverlay()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"28242d"),
                    },
                },
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            Content.MoveToY(0, transition_duration, Easing.OutSine);
        }

        protected override void PopOut()
        {
            base.PopOut();

            Content.MoveToY(-1, transition_duration, Easing.InSine);
        }
    }
}
