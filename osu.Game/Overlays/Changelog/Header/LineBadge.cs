// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Changelog.Header
{
    public class LineBadge : Circle
    {
        private const float transition_duration = 100;
        private const float uncollapsed_height = 10;

        public float TransitionDuration => transition_duration;
        public float UncollapsedHeight => uncollapsed_height;
        protected bool isCollapsed;
        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                this.ResizeHeightTo(value ? 1 : uncollapsed_height, transition_duration);
            }
        }

        public LineBadge()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.Centre;

            // this margin prevents jumps when changing text's font weight
            Margin = new MarginPadding()
            {
                Left = 10,
                Right = 10,
            };
        }
    }
}
