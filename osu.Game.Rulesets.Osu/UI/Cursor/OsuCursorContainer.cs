// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class OsuCursorContainer : GameplayCursorContainer, IKeyBindingHandler<OsuAction>
    {
        protected override Drawable CreateCursor() => new OsuCursor();

        protected override Container<Drawable> Content => fadeContainer;

        private readonly Container<Drawable> fadeContainer;

        public OsuCursorContainer()
        {
            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new CursorTrail { Depth = 1 }
                }
            };
        }

        private int downCount;

        private void updateExpandedState()
        {
            if (downCount > 0)
                (ActiveCursor as OsuCursor)?.Expand();
            else
                (ActiveCursor as OsuCursor)?.Contract();
        }

        public bool OnPressed(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    downCount++;
                    updateExpandedState();
                    break;
            }

            return false;
        }

        public bool OnReleased(OsuAction action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    if (--downCount == 0)
                        updateExpandedState();
                    break;
            }

            return false;
        }

        public override bool HandlePositionalInput => true; // OverlayContainer will set this false when we go hidden, but we always want to receive input.

        protected override void PopIn()
        {
            fadeContainer.FadeTo(1, 300, Easing.OutQuint);
            ActiveCursor.ScaleTo(1, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            fadeContainer.FadeTo(0.05f, 450, Easing.OutQuint);
            ActiveCursor.ScaleTo(0.8f, 450, Easing.OutQuint);
        }
    }
}
