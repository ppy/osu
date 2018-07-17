// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Changelog.Header
{
    public class LineBadge : Circle
    {
        private const float transition_duration = 100;
        private const float uncollapsed_height = 10;
        public float UncollapsedHeight => uncollapsed_height;
        public float TransitionDuration => transition_duration;
        private bool isCollapsed;
        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                this.ResizeHeightTo(value ? 1 : 10, transition_duration);
            }
        }

        public LineBadge(bool startCollapsed = false)
        {
            IsCollapsed = startCollapsed;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.Centre;
            Margin = new MarginPadding()
            {
                Left = 10,
                Right = 10,
            };
        }
    }
}
