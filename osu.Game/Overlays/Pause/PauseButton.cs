using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Overlays.Pause
{
    public class PauseButton : ClickableContainer
    {
        private const float colourWidth = 0.8f;
        private const float colourExpandedWidth = 0.9f;
        private const float colourExpandTime = 500;
        private Vector2 colourShear = new Vector2(0.2f, 0);

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

        private Container backgroundContainer, colourContainer, glowContainer;

        private SpriteText spriteText;

        private bool didClick; // Used for making sure that the OnMouseDown animation can call instead of OnHoverLost's

        public override bool Contains(Vector2 screenSpacePos) => backgroundContainer.Contains(screenSpacePos);

        protected override bool OnMouseDown(Framework.Input.InputState state, MouseDownEventArgs args)
        {
            didClick = true;
            colourContainer.ResizeTo(new Vector2(1.5f, 1f), 200, EasingTypes.In);
            sampleClick?.Play();
            Action?.Invoke();

            Delay(200);
            Schedule(delegate {
                colourContainer.ResizeTo(new Vector2(colourWidth, 1f), 0, EasingTypes.None);
                spriteText.Spacing = Vector2.Zero;
                glowContainer.Alpha = 0;
            });

            return true;
        }

        protected override bool OnClick(Framework.Input.InputState state) => false;

        protected override bool OnHover(Framework.Input.InputState state)
        {
            colourContainer.ResizeTo(new Vector2(colourExpandedWidth, 1f), colourExpandTime, EasingTypes.OutElastic);
            spriteText.TransformSpacingTo(new Vector2(3f, 0f), colourExpandTime, EasingTypes.OutElastic);
            glowContainer.FadeTo(1f, colourExpandTime / 2, EasingTypes.Out);
            sampleHover?.Play();
            return true;
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            if (!didClick)
            {
                colourContainer.ResizeTo(new Vector2(colourWidth, 1f), colourExpandTime, EasingTypes.OutElastic);
                spriteText.TransformSpacingTo(Vector2.Zero, colourExpandTime, EasingTypes.OutElastic);
                glowContainer.FadeTo(0f, colourExpandTime / 2, EasingTypes.Out);
            }

            didClick = false;
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
                    // The red from the design isn't in the palette so it's used directly
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
                            ColourInfo = ColourInfo.GradientHorizontal(new Color4(buttonColour.R, buttonColour.G, buttonColour.B, 0f), buttonColour)
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
                            ColourInfo = ColourInfo.GradientHorizontal(buttonColour, new Color4(buttonColour.R, buttonColour.G, buttonColour.B, 0f))
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
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
                            Shear = colourShear,
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
                                    Shear = -colourShear
                                }
                            }
                        }
                    }
                },
                spriteText = new SpriteText
                {
                    Text = Text,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 28,
                    Font = "Exo2.0-Bold",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.1f),
                    Colour = Color4.White,
                }
            });
        }

        public PauseButton()
        {
            
        }
    }

    public enum PauseButtonType
    {
        Resume,
        Retry,
        Quit
    }
}
