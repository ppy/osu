// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogSupporterPromo : CompositeDrawable
    {
        public ChangelogSupporterPromo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Horizontal = 50,
            };
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.3f),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                        },
                    }
                },
            };
        }
    }
}
