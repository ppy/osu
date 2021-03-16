// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        private Container tabletContainer;
        private Container usableAreaContainer;

        private readonly Bindable<Size> areaOffset = new BindableSize();
        private readonly Bindable<Size> areaSize = new BindableSize();
        private readonly IBindable<Size> tabletSize = new BindableSize();

        private OsuSpriteText tabletName;

        public TabletAreaSelection(ITabletHandler handler)
        {
            this.handler = handler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding(5);

            InternalChild = tabletContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 5,
                BorderThickness = 2,
                BorderColour = colour.Gray3,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colour.Gray1,
                    },
                    usableAreaContainer = new Container
                    {
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.6f,
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
                    tabletName = new OsuSpriteText
                    {
                        Padding = new MarginPadding(3),
                        Font = OsuFont.Default.With(size: 8)
                    },
                }
            };

            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                usableAreaContainer.MoveTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
                checkBounds();
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(new Vector2(val.NewValue.Width, val.NewValue.Height), 100, Easing.OutQuint);
                checkBounds();
            }, true);

            tabletSize.BindTo(handler.TabletSize);
            tabletSize.BindValueChanged(val =>
            {
                tabletContainer.Size = new Vector2(val.NewValue.Width, val.NewValue.Height);
                tabletName.Text = handler.DeviceName;
                checkBounds();
            });
        }

        [Resolved]
        private OsuColour colour { get; set; }

        private void checkBounds()
        {
            Size areaExtent = areaOffset.Value + areaSize.Value;

            bool isWithinBounds = areaExtent.Width <= tabletSize.Value.Width
                                  && areaExtent.Height <= tabletSize.Value.Height;

            usableAreaContainer.FadeColour(isWithinBounds ? colour.Blue : colour.RedLight, 100);
        }

        protected override void Update()
        {
            base.Update();

            var size = tabletSize.Value;

            if (size == System.Drawing.Size.Empty)
                return;

            float fitX = size.Width / DrawWidth;
            float fitY = size.Height / DrawHeight;

            float adjust = MathF.Max(fitX, fitY);

            tabletContainer.Scale = new Vector2(1 / adjust);
        }
    }
}
