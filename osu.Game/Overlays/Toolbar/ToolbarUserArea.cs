// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using OpenTK;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarUserArea : Container
    {
        public LoginOverlay LoginOverlay;
        private readonly ToolbarUserButton button;

        public override RectangleF BoundingBox => button.BoundingBox;

        public ToolbarUserArea()
        {
            AlwaysReceiveInput = true;

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Children = new Drawable[] {
                button = new ToolbarUserButton
                {
                    Action = () => LoginOverlay.ToggleVisibility(),
                },
                LoginOverlay = new LoginOverlay
                {
                    BypassAutoSizeAxes = Axes.Both,
                    Position = new Vector2(0, 1),
                    RelativePositionAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };
        }
    }
}