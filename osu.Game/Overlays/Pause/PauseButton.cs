using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Overlays.Pause
{
    public class PauseButton : ClickableContainer
    {
        private float height = 70;
        private float colourWidth = 0.8f;
        private float colourExpandedWidth = 0.9f;
        private float colourExpandTime = 500;
        private float shear = 0.2f;
        private float glowGradientEndAlpha = 0f;
        private double pressExpandTime = 100;

        private Color4 buttonColour;
        private Color4 backgroundColour = OsuColour.Gray(34);

        private AudioSample sampleClick;
        private AudioSample sampleHover;

        public PauseButtonType Type;

        public string Text
        {
            get
            {
                switch (Type)
                {
                    case PauseButtonType.Resume:
                        return "Continue";

                    case PauseButtonType.Retry:
                        return "Retry";

                    case PauseButtonType.Quit:
                        return "Quit to Main Menu";

                    default:
                        return "Unknown";
                }
            }
        }

        private Container backgroundContainer;
        private Container colourContainer;
        private Container glowContainer;

        public override bool Contains(Vector2 screenSpacePos) => backgroundContainer.Contains(screenSpacePos);

        protected override bool OnMouseDown(Framework.Input.InputState state, MouseDownEventArgs args)
        {
            colourContainer.ResizeTo(new Vector2(1.1f, 1f), pressExpandTime, EasingTypes.In);
            sampleClick?.Play();
            Action?.Invoke();
            return true;
        }

        protected override bool OnHover(Framework.Input.InputState state)
        {
            colourContainer.ResizeTo(new Vector2(colourExpandedWidth, 1f), colourExpandTime, EasingTypes.OutElastic);
            glowContainer.FadeTo(1f, colourExpandTime / 2, EasingTypes.Out);
            sampleHover?.Play();
            return true;
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            colourContainer.ResizeTo(new Vector2(colourWidth, 1f), colourExpandTime, EasingTypes.OutElastic);
            glowContainer.FadeTo(0f, colourExpandTime / 2, EasingTypes.Out);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            switch (Type)
            {
                case PauseButtonType.Resume:
                    buttonColour = colours.Green;
                    sampleClick = audio.Sample.Get(@"Menu/menuback");
                    break;

                case PauseButtonType.Retry:
                    buttonColour = colours.YellowDark;
                    sampleClick = audio.Sample.Get(@"Menu/menu-play-click");
                    break;

                case PauseButtonType.Quit:
                    // For whatever reason the red from the mockup is not in the osu! palette
                    buttonColour = new Color4(170, 27, 39, 255);
                    sampleClick = audio.Sample.Get(@"Menu/menuback");
                    break;
            }

            sampleHover = audio.Sample.Get(@"Menu/menuclick");

            Add(new Drawable[]
            {
                backgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour
                        }
                    }
                },
                glowContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Width = 0.125f,
                            ColourInfo = ColourInfo.GradientHorizontal(new Color4(buttonColour.R, buttonColour.G, buttonColour.B, glowGradientEndAlpha), buttonColour)
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.75f,
                            Colour = buttonColour
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Width = 0.125f,
                            ColourInfo = ColourInfo.GradientHorizontal(buttonColour, new Color4(buttonColour.R, buttonColour.G, buttonColour.B, glowGradientEndAlpha))
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        colourContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = colourWidth,
                            Masking = true,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 5
                            },
                            Colour = buttonColour,
                            Shear = new Vector2(shear, 0),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    EdgeSmoothness = new Vector2(2, 0),
                                    RelativeSizeAxes = Axes.Both
                                },
                                new Triangles
                                {
                                    BlendingMode = BlendingMode.Additive,
                                    RelativeSizeAxes = Axes.Both,
                                    TriangleScale = 4,
                                    Alpha = 0.05f,
                                    Shear = new Vector2(-shear, 0)
                                }
                            }
                        }
                    }
                },
                new SpriteText
                {
                    Text = Text,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 25,
                    Font = "Exo2.0-Bold",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.1f),
                    Colour = Color4.White
                }
            });
        }

        public PauseButton()
        {
            Height = height;
            RelativeSizeAxes = Axes.X;
        }
    }

    public enum PauseButtonType
    {
        Resume,
        Retry,
        Quit
    }
}
