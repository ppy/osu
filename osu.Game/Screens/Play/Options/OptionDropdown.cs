// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.Options
{
    public class OptionDropdown : OverlayContainer
    {
        private const float transition_duration = 600;

        private FillFlowContainer content;

        public OptionDropdown()
        {
            Children = new Drawable[]
            {
                content = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Padding = new MarginPadding(15),
                    Spacing = new Vector2(0, 10),
                }
            };
        }

        public new void Add(Drawable drawable)
        {
            content.Add(drawable);
        }

        protected override void PopIn()
        {
            ResizeTo(new Vector2(1, content.Height), transition_duration, EasingTypes.OutQuint);
            FadeIn(transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            ResizeTo(new Vector2(1, 0), transition_duration, EasingTypes.OutQuint);
            FadeOut(transition_duration);
        }
    }
}
