// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        private readonly Bindable<Vector2> areaOffset = new Bindable<Vector2>();
        private readonly Bindable<Vector2> areaSize = new Bindable<Vector2>();

        private readonly BindableNumber<float> rotation = new BindableNumber<float>();

        private readonly IBindable<TabletInfo> tablet = new Bindable<TabletInfo>();

        private OsuSpriteText tabletName;

        private Box usableFill;
        private OsuSpriteText usableAreaText;

        public TabletAreaSelection(ITabletHandler handler)
        {
            this.handler = handler;

            Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            usableFill = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.6f,
                            },
                            new Box
                            {
                                Colour = Color4.White,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Height = 5,
                            },
                            new Box
                            {
                                Colour = Color4.White,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = 5,
                            },
                            usableAreaText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.White,
                                Font = OsuFont.Default.With(size: 12),
                                Y = 10
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            areaOffset.BindTo(handler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                usableAreaContainer.MoveTo(val.NewValue, 100, Easing.OutQuint)
                                   .OnComplete(_ => checkBounds()); // required as we are using SSDQ.
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(val.NewValue, 100, Easing.OutQuint)
                                   .OnComplete(_ => checkBounds()); // required as we are using SSDQ.

                int x = (int)val.NewValue.X;
                int y = (int)val.NewValue.Y;
                int commonDivider = greatestCommonDivider(x, y);

                usableAreaText.Text = $"{(float)x / commonDivider}:{(float)y / commonDivider}";
            }, true);

            rotation.BindTo(handler.Rotation);
            rotation.BindValueChanged(val =>
            {
                tabletContainer.RotateTo(-val.NewValue, 800, Easing.OutQuint);
                usableAreaContainer.RotateTo(val.NewValue, 100, Easing.OutQuint)
                                   .OnComplete(_ => checkBounds()); // required as we are using SSDQ.
            }, true);

            tablet.BindTo(handler.Tablet);
            tablet.BindValueChanged(_ => Scheduler.AddOnce(updateTabletDetails));

            updateTabletDetails();
            // initial animation should be instant.
            FinishTransforms(true);
        }

        private void updateTabletDetails()
        {
            tabletContainer.Size = tablet.Value?.Size ?? Vector2.Zero;
            tabletName.Text = tablet.Value?.Name ?? string.Empty;
            checkBounds();
        }

        private static int greatestCommonDivider(int a, int b)
        {
            while (b != 0)
            {
                int remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }

        [Resolved]
        private OsuColour colour { get; set; }

        private void checkBounds()
        {
            if (tablet.Value == null)
                return;

            var usableSsdq = usableAreaContainer.ScreenSpaceDrawQuad;

            bool isWithinBounds = tabletContainer.ScreenSpaceDrawQuad.Contains(usableSsdq.TopLeft + new Vector2(1)) &&
                                  tabletContainer.ScreenSpaceDrawQuad.Contains(usableSsdq.BottomRight - new Vector2(1));

            usableFill.FadeColour(isWithinBounds ? colour.Blue : colour.RedLight, 100);
        }

        protected override void Update()
        {
            base.Update();

            if (!(tablet.Value?.Size is Vector2 size))
                return;

            float maxDimension = size.LengthFast;

            float fitX = maxDimension / (DrawWidth - Padding.Left - Padding.Right);
            float fitY = maxDimension / DrawHeight;

            float adjust = MathF.Max(fitX, fitY);

            tabletContainer.Scale = new Vector2(1 / adjust);
        }
    }
}
