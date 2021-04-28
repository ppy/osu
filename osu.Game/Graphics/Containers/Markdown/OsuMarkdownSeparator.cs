// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownSeparator : MarkdownSeparator
    {
        private Drawable separator;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            separator.Colour = colourProvider.Background3;
        }

        protected override Drawable CreateSeparator()
        {
            return separator = base.CreateSeparator();
        }
    }
}
