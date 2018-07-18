// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Changelog.Header
{
    public class LineBadge : Circle
    {
        public float TransitionDuration = 400;
        public float UncollapsedHeight;
        public float CollapsedHeight;

        private bool isCollapsed;
        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                isCollapsed = value;
                this.ResizeHeightTo(value ? CollapsedHeight : UncollapsedHeight,
                    value ? TransitionDuration / 2f : TransitionDuration,
                    value ? Easing.Out : Easing.OutElastic);
            }
        }

        public LineBadge(bool startCollapsed = true, float collapsedHeight = 1, float uncollapsedHeight = 10)
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.Centre;
            CollapsedHeight = collapsedHeight;
            UncollapsedHeight = uncollapsedHeight;
            Height = startCollapsed ? CollapsedHeight : UncollapsedHeight;

            // this margin prevents jumps when changing text's font weight
            Margin = new MarginPadding
            {
                Left = 10,
                Right = 10,
            };
        }
    }
}
