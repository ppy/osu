// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Settings
{
    public class SettingsSidebar : ExpandingButtonContainer
    {
        public const float DEFAULT_WIDTH = 70;
        public const int EXPANDED_WIDTH = 200;

        public SettingsSidebar()
            : base(DEFAULT_WIDTH, EXPANDED_WIDTH)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AddInternal(new Box
            {
                Colour = colourProvider.Background5,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });
        }
    }
}
