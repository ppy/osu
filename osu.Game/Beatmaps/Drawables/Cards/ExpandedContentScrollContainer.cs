// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class ExpandedContentScrollContainer : OsuScrollContainer
    {
        public const float HEIGHT = 200;

        public ExpandedContentScrollContainer()
        {
            ScrollbarVisible = false;
        }

        protected override void Update()
        {
            base.Update();

            Height = Math.Min(Content.DrawHeight, HEIGHT);
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
    }
}
