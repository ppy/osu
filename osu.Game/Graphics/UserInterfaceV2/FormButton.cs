// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
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

        /// <summary>
        /// Sets text inside the button.
        /// </summary>
        public LocalisableString ButtonText { get; init; }

        /// <summary>
        /// Sets a custom button icon. Not shown when <see cref="ButtonText"/> is set.
        /// </summary>
        public IconUsage ButtonIcon { get; init; } = FontAwesome.Solid.ChevronRight;

        private readonly Color4? backgroundColour;

        /// <summary>
        /// Sets a custom background colour for the button.
        /// </summary>
        public Color4? BackgroundColour
        {
            get => backgroundColour;
            init
            {
                backgroundColour = value;

                if (IsLoaded)
                    updateState();
            }
        }

        /// <summary>
        /// The action to invoke when the button is clicked.
        /// </summary>
        public Action? Action { get; set; }

        /// <summary>
        /// Whether the button is enabled.
        /// </summary>
        public readonly BindableBool Enabled = new BindableBool(true);

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Container content = null!;
        private Box background = null!;
        private OsuTextFlowContainer text = null!;
        private Button button = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5,
                CornerExponent = 2.5f,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new TrianglesV2
                    {
                        SpawnRatio = 0.5f,
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colourProvider.Background4, colourProvider.Background5),
                    },
                    new HoverClickSounds(HoverSampleSet.Button)
                    {
                        Enabled = { BindTarget = Enabled },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Left = 9,
                            Right = 5,
                            Vertical = 5,
                        },
                        Children = new Drawable[]
                        {
                            text = new OsuTextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = Caption,
                            },
                            button = new Button
                            {
                                Action = () => Action?.Invoke(),
                                Text = ButtonText,
                                Icon = ButtonIcon,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Enabled = { BindTarget = Enabled },
                            }
                        },
                    },
                }
            };

            if (ButtonText == default)
            {
                text.Padding = new MarginPadding { Right = 100 };
                button.Width = 90;
            }
            else
            {
                text.Width = 0.55f;
                text.Padding = new MarginPadding { Right = 10 };
                button.RelativeSizeAxes = Axes.X;
                button.Width = 0.45f;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateState(), true);
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

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
            {
                background.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);
                button.TriggerClick();
            }

            return true;
        }

        private void updateState()
        {
            text.Colour = Enabled.Value ? colourProvider.Content1 : colourProvider.Background1;

            background.FadeColour(IsHovered
                ? ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4)
                : colourProvider.Background5, 200, Easing.OutQuint);

            content.BorderThickness = IsHovered ? 2 : 0;

            if (BackgroundColour != null)
            {
                button.BackgroundColour = BackgroundColour.Value;
                content.BorderColour = Enabled.Value ? BackgroundColour.Value : Interpolation.ValueAt(0.75, BackgroundColour.Value, colourProvider.Dark1, 0, 1);
            }
            else
                content.BorderColour = Enabled.Value ? colourProvider.Light4 : colourProvider.Dark1;
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

            public IconUsage Icon { get; init; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider overlayColourProvider)
            {
                DefaultBackgroundColour = overlayColourProvider.Colour3;
                triangleGradientSecondColour ??= DefaultBackgroundColour.Lighten(0.2f);

                if (Text == default)
                {
                    Add(new SpriteIcon
                    {
                        Icon = Icon,
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
