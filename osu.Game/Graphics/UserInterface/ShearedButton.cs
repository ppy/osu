// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedButton : OsuClickableContainer
    {
        public const float HEIGHT = 50;
        public const float CORNER_RADIUS = 7;
        public const float BORDER_THICKNESS = 2;

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public float TextSize
        {
            get => text.Font.Size;
            set => text.Font = OsuFont.TorusAlternate.With(size: value);
        }

        public Colour4 DarkerColour
        {
            set
            {
                darkerColour = value;
                Scheduler.AddOnce(updateState);
            }
        }

        public Colour4 LighterColour
        {
            set
            {
                lighterColour = value;
                Scheduler.AddOnce(updateState);
            }
        }

        public Colour4 TextColour
        {
            set
            {
                textColour = value;
                Scheduler.AddOnce(updateState);
            }
        }

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        private readonly Box background;
        private readonly OsuSpriteText text;

        private const float shear = 0.2f;

        private Colour4? darkerColour;
        private Colour4? lighterColour;
        private Colour4? textColour;

        private readonly Container backgroundLayer;
        private readonly Box flashLayer;

        /// <summary>
        /// Creates a new <see cref="ShearedToggleButton"/>
        /// </summary>
        /// <param name="width">
        /// The width of the button.
        /// <list type="bullet">
        /// <item>If a non-<see langword="null"/> value is provided, this button will have a fixed width equal to the provided value.</item>
        /// <item>If a <see langword="null"/> value is provided (or the argument is omitted entirely), the button will autosize in width to fit the text.</item>
        /// </list>
        /// </param>
        public ShearedButton(float? width = null)
        {
            Height = HEIGHT;
            Padding = new MarginPadding { Horizontal = shear * 50 };

            Content.CornerRadius = CORNER_RADIUS;
            Content.Shear = new Vector2(shear, 0);
            Content.Masking = true;
            Content.Anchor = Content.Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                backgroundLayer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    CornerRadius = CORNER_RADIUS,
                    Masking = true,
                    BorderThickness = BORDER_THICKNESS,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.TorusAlternate.With(size: 17),
                            Shear = new Vector2(-shear, 0)
                        },
                    }
                },
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.9f),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            };

            if (width != null)
            {
                Width = width.Value;
                backgroundLayer.RelativeSizeAxes = Axes.Both;
            }
            else
            {
                AutoSizeAxes = Axes.X;
                backgroundLayer.AutoSizeAxes = Axes.X;
                text.Margin = new MarginPadding { Horizontal = 15 };
            }
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverClickSounds(sampleSet) { Enabled = { BindTarget = Enabled } };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(_ => Scheduler.AddOnce(updateState));

            updateState();
            FinishTransforms(true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
                flashLayer.FadeOutFromOne(800, Easing.OutQuint);

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Scheduler.AddOnce(updateState);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Scheduler.AddOnce(updateState);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        private void updateState()
        {
            var colourDark = darkerColour ?? ColourProvider.Background3;
            var colourLight = lighterColour ?? ColourProvider.Background1;
            var colourText = textColour ?? ColourProvider.Content1;

            if (!Enabled.Value)
            {
                colourDark = colourDark.Darken(1f);
                colourLight = colourLight.Darken(1f);
            }
            else if (IsHovered)
            {
                colourDark = colourDark.Lighten(0.2f);
                colourLight = colourLight.Lighten(0.2f);
            }

            background.FadeColour(colourDark, 150, Easing.OutQuint);
            backgroundLayer.TransformTo(nameof(BorderColour), ColourInfo.GradientVertical(colourDark, colourLight), 150, Easing.OutQuint);

            if (!Enabled.Value)
                colourText = colourText.Opacity(0.6f);

            text.FadeColour(colourText, 150, Easing.OutQuint);
        }
    }
}
