// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Verify
{
    internal class VisibilitySettings : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new LabelledSwitchButton
                        {
                            Label = "Show problems",
                            Current = new Bindable<bool>(true)
                        },
                        new LabelledSwitchButton
                        {
                            Label = "Show warnings",
                            Current = new Bindable<bool>(true)
                        },
                        new LabelledSwitchButton
                        {
                            Label = "Show negligibles"
                        }
                    }
                },
            };
        }
    }
}
