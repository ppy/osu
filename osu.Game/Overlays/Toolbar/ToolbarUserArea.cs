//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using OpenTK;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarUserArea : Container
    {
        private LoginOverlay loginOverlay;
        private ToolbarUserButton button;

        public override RectangleF BoundingBox => button.BoundingBox;

        public override bool Contains(Vector2 screenSpacePos) => true;

        public override Vector2 Size => button.Size;

        public ToolbarUserArea()
        {
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[] {
                button = new ToolbarUserButton
                {
                    Action = toggle,
                },
                loginOverlay = new LoginOverlay
                {
                    Position = new Vector2(0, 1),
                    RelativePositionAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };
        }

        private void toggle()
        {
            loginOverlay.ToggleVisibility();
        }
    }
}