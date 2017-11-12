// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;


namespace osu.Game.Overlays.Notifications
{
    public class ProgressCompletionNotification : SimpleNotification
    {
        public ProgressCompletionNotification()
        {
            Icon = FontAwesome.fa_check;

            Content.Add(subTextDrawable = new TextFlowContainer(t => t.TextSize = 16)
            {
                Colour = OsuColour.Gray(128),
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
            });
        }

        public string SubText
        {
            set { Schedule(() => subTextDrawable.Text = value); }
        }

        private readonly TextFlowContainer subTextDrawable;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IconBackgound.Colour = ColourInfo.GradientVertical(colours.GreenDark, colours.GreenLight);
        }
    }
}
