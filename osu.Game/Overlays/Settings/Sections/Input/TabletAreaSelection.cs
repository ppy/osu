// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Handlers.Tablet;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TabletAreaSelection : CompositeDrawable
    {
        private readonly ITabletHandler handler;

        private readonly Container tabletContainer;
        private readonly Container usableAreaContainer;

        private readonly Bindable<Size> areaOffset = new BindableSize();
        private readonly Bindable<Size> areaSize = new BindableSize();
        private readonly Bindable<Size> tabletSize = new BindableSize();

        public TabletAreaSelection(ITabletHandler handler)
        {
            this.handler = handler;

            Padding = new MarginPadding(5);

            InternalChildren = new Drawable[]
            {
                tabletContainer = new Container
                {
                    Masking = true,
                    CornerRadius = 5,
                    BorderThickness = 2,
                    BorderColour = Color4.Black,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                        usableAreaContainer = new Container
                        {
                            Masking = true,
                            CornerRadius = 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Yellow,
                                },
                                new OsuSpriteText
                                {
                                    Text = "usable area",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.Black,
                                    Font = OsuFont.Default.With(size: 12)
                                }
                            }
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                usableAreaContainer.MoveTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
            }, true);

            ((IBindable<Size>)tabletSize).BindTo(handler.TabletSize);
            tabletSize.BindValueChanged(val =>
            {
                tabletContainer.ResizeTo(new Vector2(tabletSize.Value.Width, tabletSize.Value.Height), 100, Easing.OutQuint);
            }, true);
        }
    }
}
