// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.MatrixExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class TabletAreaSelection : CompositeDrawable
    {
        public bool IsWithinBounds { get; private set; }

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
                    usableAreaContainer = new UsableAreaContainer(handler)
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
                usableAreaContainer.MoveTo(val.NewValue, 100, Easing.OutQuint);
                checkBounds();
            }, true);

            areaSize.BindTo(handler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                usableAreaContainer.ResizeTo(val.NewValue, 100, Easing.OutQuint);

                int x = (int)Math.Round(val.NewValue.X);
                int y = (int)Math.Round(val.NewValue.Y);
                int commonDivider = greatestCommonDivider(x, y);

                usableAreaText.Text = $"{x / commonDivider}:{y / commonDivider}";
                checkBounds();
            }, true);

            rotation.BindTo(handler.Rotation);
            rotation.BindValueChanged(val =>
            {
                usableAreaContainer.RotateTo(val.NewValue, 100, Easing.OutQuint);
                tabletContainer.RotateTo(-val.NewValue, 800, Easing.OutQuint);

                checkBounds();
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

            // allow for some degree of floating point error, as we don't care about being perfect here.
            const float lenience = 0.5f;

            var tabletArea = new Quad(-lenience, -lenience, tablet.Value.Size.X + lenience * 2, tablet.Value.Size.Y + lenience * 2);

            var halfUsableArea = areaSize.Value / 2;
            var offset = areaOffset.Value;

            var usableAreaQuad = new Quad(
                new Vector2(-halfUsableArea.X, -halfUsableArea.Y),
                new Vector2(halfUsableArea.X, -halfUsableArea.Y),
                new Vector2(-halfUsableArea.X, halfUsableArea.Y),
                new Vector2(halfUsableArea.X, halfUsableArea.Y)
            );

            var matrix = Matrix3.Identity;

            MatrixExtensions.TranslateFromLeft(ref matrix, offset);
            MatrixExtensions.RotateFromLeft(ref matrix, MathUtils.DegreesToRadians(rotation.Value));

            usableAreaQuad *= matrix;

            IsWithinBounds =
                tabletArea.Contains(usableAreaQuad.TopLeft) &&
                tabletArea.Contains(usableAreaQuad.TopRight) &&
                tabletArea.Contains(usableAreaQuad.BottomLeft) &&
                tabletArea.Contains(usableAreaQuad.BottomRight);

            usableFill.FadeColour(IsWithinBounds ? colour.Blue : colour.RedLight, 100);
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

    public partial class UsableAreaContainer : Container
    {
        private readonly Bindable<Vector2> areaOffset;

        public UsableAreaContainer(ITabletHandler tabletHandler)
        {
            areaOffset = tabletHandler.AreaOffset.GetBoundCopy();
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            var newPos = Position + e.Delta;
            this.MoveTo(Vector2.Clamp(newPos, Vector2.Zero, Parent!.Size));
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            areaOffset.Value = Position;
            base.OnDragEnd(e);
        }
    }
}
