using System;
using osu.Core.Screens.Evast;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using Symcol.osu.Core.KoziLord.EvastModded.MusicPlayers;

namespace Symcol.osu.Core.KoziLord
{
    public class KoziScreen : BeatmapScreen
    {
        public FillFlowContainer ColumnContainer;

        public Box ColumnBackground;

        public ColumnButton ColumnElement;

        public Container MainContainer;

        public KoziScreen()
        {
            Children = new Drawable[]
            {
                MainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 600,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        ColumnBackground = new Box
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Scale = new Vector2(0,1),
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.4f),
                        },
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                ColumnContainer = new FillFlowContainer
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Padding = new MarginPadding{Top = 20, Bottom = 20},
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(0,20),
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                       ColumnElement = new ColumnButton(@"FullScreen Player", () => Push(new FullscreenPlayer())),
                                       ColumnElement = new ColumnButton(@"Dummy button"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),
                                       ColumnElement = new ColumnButton(@"'Nother one"),


                                    }
                                }
                            }
                        },
                    },
                }
            };
    
        }
        public class ColumnButton : OsuClickableContainer
        {
            private readonly Box ItemBackground;
            private readonly OsuSpriteText ButtonTitle;

            public ColumnButton(string title, Action onPressed = null)
            {
                Action = onPressed;

                Alpha = 0;
                Scale = new Vector2(0.75f);
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                Height = 100;
                Width = 500;
                CornerRadius = 16;
                Masking = true;
                Children = new Drawable[]
                {
                   ItemBackground  = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.2f),
                        Alpha = 0.5f
                    },
                    ButtonTitle = new OsuSpriteText
                    {
                        TextSize = 26,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = title
                    }
                };
             
            }
            
            protected override bool OnHover(InputState state)
            {
                ItemBackground.FadeIn(50, Easing.Out);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                ItemBackground.FadeTo(0.5f, 150, Easing.Out);
                base.OnHoverLost(state);
            }
        }
        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            int delaySequence = 0;
            foreach (ColumnButton button in ColumnContainer)
            {
                button.Delay(50 * delaySequence)
                    .FadeInFromZero(600, Easing.Out)
                    .ScaleTo(1, 400, Easing.OutCubic);
                delaySequence++;
            }
            
            ColumnBackground.ScaleTo(new Vector2(1, 1), 600, Easing.OutQuart);
        }

        protected override bool OnExiting(Screen next)
         {
             MainContainer.FadeOut(200, Easing.In);
             ColumnBackground.ScaleTo(new Vector2(0, 1), 200, Easing.InCubic);

             return base.OnExiting(next);
         }
        //TODO: OnSuspending and OnResuming animations.
        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            ColumnContainer.ScaleTo(1.4f, 200, Easing.InCubic);
            ColumnBackground.Delay(100).ScaleTo(new Vector2(1.2f, 1), 200, Easing.InCubic);

            ColumnContainer.FadeOut(200, Easing.In);
            ColumnBackground.Delay(100).FadeOut(200, Easing.In);

        }
        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            ColumnContainer.ScaleTo(1, 300, Easing.OutCubic);
            ColumnBackground.Delay(100).ScaleTo(1, 300, Easing.OutCubic);

            ColumnContainer.FadeIn(300, Easing.Out);
            ColumnBackground.Delay(100).FadeIn(300, Easing.Out);
        }
    }
}
