// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public partial class OsuMarkdownSeparator : MarkdownSeparator
    {
        protected override Drawable CreateSeparator() => new Separator();

        private partial class Separator : Box
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.X;
                Height = 1;
                Colour = colourProvider.Background3;
            }
        }
    }
}
