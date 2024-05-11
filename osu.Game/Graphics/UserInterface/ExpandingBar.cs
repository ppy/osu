// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A rounded bar which can be expanded or collapsed.
    /// Generally used for tabs or breadcrumbs.
    /// </summary>
    public partial class ExpandingBar : Circle
    {
        private bool expanded = true;

        public bool Expanded
        {
            get => expanded;
            set
            {
                if (value == expanded)
                    return;

                expanded = value;
                updateState();
            }
        }

        private float expandedSize = 4;

        public float ExpandedSize
        {
            get => expandedSize;
            set
            {
                if (value == expandedSize)
                    return;

                expandedSize = value;
                updateState();
            }
        }

        private float collapsedSize = 2;

        public float CollapsedSize
        {
            get => collapsedSize;
            set
            {
                if (value == collapsedSize)
                    return;

                collapsedSize = value;
                updateState();
            }
        }

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                base.RelativeSizeAxes = Axes.None;
                Size = Vector2.Zero;

                base.RelativeSizeAxes = value;
                updateState();
            }
        }

        public ExpandingBar()
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        public void Collapse() => Expanded = false;

        public void Expand() => Expanded = true;

        private void updateState()
        {
            float newSize = expanded ? ExpandedSize : CollapsedSize;
            Easing easingType = expanded ? Easing.OutElastic : Easing.Out;

            if (RelativeSizeAxes == Axes.X)
                this.ResizeHeightTo(newSize, 400, easingType);
            else
                this.ResizeWidthTo(newSize, 400, easingType);

            this.FadeTo(expanded ? 1 : 0.5f, 100, Easing.OutQuint);
        }
    }
}
