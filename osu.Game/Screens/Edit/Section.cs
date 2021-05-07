// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit
{
    internal abstract class Section : CompositeDrawable
    {
        protected Container Content;
        protected OsuCheckbox Checkbox;

        protected FillFlowContainer Flow { get; private set; }

        private const float header_height = 50;

        /// <summary>
        /// The name of this section, as seen from the user interface.
        /// </summary>
        protected abstract string SectionName { get; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeDuration = 200;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeAxes = Axes.Y;

            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        Checkbox = new OsuCheckbox
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            LabelText = SectionName
                        }
                    }
                },
                Content = new Container
                {
                    Y = header_height,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.Background3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        Flow = new FillFlowContainer
                        {
                            Padding = new MarginPadding(20),
                            Spacing = new Vector2(20),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Checkbox.Current.BindValueChanged(selected =>
            {
                Content.BypassAutoSizeAxes = selected.NewValue ? Axes.None : Axes.Y;
            }, true);
        }
    }
}
