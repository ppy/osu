// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditor : FocusedOverlayContainer
    {
        public const double TRANSITION_DURATION = 500;

        private readonly Drawable target;

        protected override bool StartHidden => true;

        public SkinEditor(Drawable target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SkinBlueprintContainer(target),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Show();
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override void PopIn()
        {
            this.FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
