// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A simple rounded expandable line. Set its <see cref="Anchor"/>
    /// property to the center of the edge it's meant stick with. By default,
    /// takes up the full parent's axis defined by <see cref="IsHorizontal"/>.
    /// </summary>
    public class LineBadge : Circle
    {
        public float UncollapsedSize;
        public float CollapsedSize;

        public bool IsCollapsed { get; private set; }
        private bool isHorizontal;

        /// <summary>
        /// Automatically sets the RelativeSizeAxes and switches X and Y components when changed.
        /// </summary>
        public bool IsHorizontal
        {
            get { return isHorizontal; }
            set
            {
                if (value == isHorizontal)
                    return;
                if (IsLoaded)
                {
                    FinishTransforms();
                    var height = Height;
                    var width = Width;
                    RelativeSizeAxes = value ? Axes.X : Axes.Y;
                    Width = height;
                    Height = width;
                }
                else
                    RelativeSizeAxes = value ? Axes.X : Axes.Y;
                isHorizontal = value;
            }
        }

        /// <param name="startCollapsed">Whether to initialize with the
        /// <see cref="CollapsedSize"/> or the <see cref="UncollapsedSize"/>.</param>
        public LineBadge(bool startCollapsed = true)
        {
            IsCollapsed = startCollapsed;
            RelativeSizeAxes = Axes.X;
            isHorizontal = true;
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            if (isHorizontal)
                Height = IsCollapsed ? CollapsedSize : UncollapsedSize;
            else
                Width = IsCollapsed ? CollapsedSize : UncollapsedSize;
            base.LoadComplete();
        }

        public void Collapse(float transitionDuration = 400, Easing easing = Easing.Out)
        {
            IsCollapsed = true;
            if (IsHorizontal)
                this.ResizeHeightTo(CollapsedSize, transitionDuration, easing);
            else
                this.ResizeWidthTo(CollapsedSize, transitionDuration, easing);
        }

        public void Uncollapse(float transitionDuration = 400, Easing easing = Easing.OutElastic)
        {
            IsCollapsed = false;
            if (IsHorizontal)
                this.ResizeHeightTo(UncollapsedSize, transitionDuration, easing);
            else
                this.ResizeWidthTo(UncollapsedSize, transitionDuration, easing);
        }
    }
}
