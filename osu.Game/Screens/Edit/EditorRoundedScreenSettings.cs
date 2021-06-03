// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit
{
    public abstract class EditorRoundedScreenSettings : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = CreateSections()
                    },
                }
            };
        }

        protected abstract IReadOnlyList<Drawable> CreateSections();
    }
}
