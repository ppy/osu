// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Edit
{
    public abstract partial class EditorRoundedScreenSettingsSection : CompositeDrawable
    {
        private const int header_height = 50;

        protected abstract string HeaderText { get; }

        protected FillFlowContainer Flow { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Padding = new MarginPadding { Horizontal = 20 },
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = HeaderText,
                        Font = new FontUsage(size: 25, weight: "bold")
                    }
                },
                new Container
                {
                    Y = header_height,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = Flow = new FillFlowContainer
                    {
                        Padding = new MarginPadding { Horizontal = 20 },
                        Spacing = new Vector2(10),
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    }
                }
            };
        }
    }
}
