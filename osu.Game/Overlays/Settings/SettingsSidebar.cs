// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsSidebar : ExpandingContainer
    {
        public const float DEFAULT_WIDTH = 70;
        public const int EXPANDED_WIDTH = 170;

        protected override bool ExpandOnHover => false;

        public SettingsSidebar()
            : base(DEFAULT_WIDTH, EXPANDED_WIDTH)
        {
            Expanded.Value = true;
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
