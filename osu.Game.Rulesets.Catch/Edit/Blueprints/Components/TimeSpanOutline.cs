// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class TimeSpanOutline : CompositeDrawable
    {
        private const float border_width = 4;

        private bool isEmpty = true;

        public TimeSpanOutline()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            RelativeSizeAxes = Axes.X;

            Masking = true;
            BorderThickness = border_width;

            // a box is needed to make edge visible
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Transparent
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            BorderColour = osuColour.Yellow;
        }

        public void UpdateFrom(ScrollingHitObjectContainer hitObjectContainer, BananaShower hitObject)
        {
            float startY = hitObjectContainer.PositionAtTime(hitObject.StartTime);
            float endY = hitObjectContainer.PositionAtTime(hitObject.EndTime);

            Y = Math.Max(startY, endY);
            float height = Math.Abs(startY - endY);

            bool wasEmpty = isEmpty;
            isEmpty = height == 0;
            if (wasEmpty != isEmpty)
                this.FadeTo(isEmpty ? 0.5f : 1f, 150);

            Height = Math.Max(height, border_width);
        }
    }
}
