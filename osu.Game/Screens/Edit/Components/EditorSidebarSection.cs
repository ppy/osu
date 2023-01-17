// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    public partial class EditorSidebarSection : Container
    {
        protected override Container<Drawable> Content { get; }

        public EditorSidebarSection(LocalisableString sectionName)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new SectionHeader(sectionName),
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                }
            };
        }

        public partial class SectionHeader : CompositeDrawable
        {
            private readonly LocalisableString text;

            public SectionHeader(LocalisableString text)
            {
                this.text = text;

                Margin = new MarginPadding { Vertical = 10, Horizontal = 5 };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold))
                        {
                            Text = text,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        new Circle
                        {
                            Colour = colourProvider.Highlight1,
                            Size = new Vector2(28, 2),
                        }
                    }
                };
            }
        }
    }
}
