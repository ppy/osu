// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class ExpandedContentScrollContainer : OsuScrollContainer
    {
        public const float HEIGHT = 200;

        protected override ScrollbarContainer CreateScrollbar(Direction direction) => new ExpandedContentScrollbar(direction);

        protected override void Update()
        {
            base.Update();

            Height = Math.Min(Content.DrawHeight, HEIGHT);
            ScrollbarVisible = allowScroll;
        }

        private bool allowScroll => !Precision.AlmostEquals(DrawSize, Content.DrawSize);

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!allowScroll)
                return false;

            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            if (!allowScroll)
                return;

            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (!allowScroll)
                return;

            base.OnDragEnd(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (!allowScroll)
                return false;

            return base.OnScroll(e);
        }

        private class ExpandedContentScrollbar : OsuScrollbar
        {
            public ExpandedContentScrollbar(Direction scrollDir)
                : base(scrollDir)
            {
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                // do not handle hover, as handling hover would make the beatmap card's expanded content not-hovered
                // and therefore cause it to hide when trying to drag the scroll bar.
                // see: `BeatmapCardContent.dropdownContent` and its `Unhovered` handler.
                return false;
            }
        }
    }
}
