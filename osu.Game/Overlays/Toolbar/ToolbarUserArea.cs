// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarUserArea : Container
    {
        public LoginOverlay LoginOverlay;
        private ToolbarUserButton button;

        public override RectangleF BoundingBox => button.BoundingBox;

        [BackgroundDependencyLoader]
        private void load()
        {
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
