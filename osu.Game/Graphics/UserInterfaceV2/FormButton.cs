// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormButton : CompositeDrawable
    {
        /// <summary>
        /// Caption describing this button, displayed on the left of it.
        /// </summary>
        public LocalisableString Caption { get; init; }

        public LocalisableString ButtonText { get; init; }

        public Action? Action { get; init; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = 9,
                        Right = 5,
                        Vertical = 5,
                    },
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Width = 0.45f,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = Caption,
                        },
                        new Button
                        {
                            Action = Action,
                            Text = ButtonText,
                            RelativeSizeAxes = ButtonText == default ? Axes.None : Axes.X,
                            Width = ButtonText == default ? 90 : 0.45f,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        }
                    },
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        private void updateState()
        {
            BorderThickness = IsHovered ? 2 : 0;

            if (IsHovered)
                BorderColour = colourProvider.Light4;
        }

        public partial class Button : OsuButton
        {
            private TrianglesV2? triangles { get; set; }

            protected override float HoverLayerFinalAlpha => 0;

            private Color4? triangleGradientSecondColour;

            public override Color4 BackgroundColour
            {
                get => base.BackgroundColour;
                set
                {
                    base.BackgroundColour = value;
                    triangleGradientSecondColour = BackgroundColour.Lighten(0.2f);
                    updateColours();
                }
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider overlayColourProvider)
            {
                DefaultBackgroundColour = overlayColourProvider.Colour3;
                triangleGradientSecondColour ??= overlayColourProvider.Colour1;

                if (Text == default)
                {
                    Add(new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ChevronRight,
                        Size = new Vector2(16),
                        Shadow = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Content.CornerRadius = 4;

                Add(triangles = new TrianglesV2
                {
                    Thickness = 0.02f,
                    SpawnRatio = 0.6f,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                });

                updateColours();
            }

            private void updateColours()
            {
                if (triangles == null)
                    return;

                Debug.Assert(triangleGradientSecondColour != null);

                triangles.Colour = ColourInfo.GradientVertical(triangleGradientSecondColour.Value, BackgroundColour);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Debug.Assert(triangleGradientSecondColour != null);

                Background.FadeColour(triangleGradientSecondColour.Value, 300, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeColour(BackgroundColour, 300, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
