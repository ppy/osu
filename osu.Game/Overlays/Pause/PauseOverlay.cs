using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Overlays.Pause
{
    public class PauseOverlay : OverlayContainer
    {
        private int fadeDuration = 100;

        public Action OnResume;
        public Action OnRetry;
        public Action OnQuit;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.75f,
                },
                new SpriteText
                {
                    Text = @"paused",
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, -175),
                    Font = @"Exo2.0-Medium",
                    Spacing = new Vector2(5, 0),
                    TextSize = 30,
                    Colour = colours.Yellow,
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                },
                new SpriteText
                {
                    Text = @"you're not going to do what i think you're going to do, ain't ya?",
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.Centre,
                    Width = 100,
                    Position = new Vector2(0, -125),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                },
                new SpriteText
                {
                    Text = @"You've retried 0 times in this session",
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.Centre,
                    Width = 100,
                    Position = new Vector2(0, 175),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    TextSize = 18,
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, 25),
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = new Color4(0, 0, 0, 150),
                        Radius = 50,
                        Offset = new Vector2(0, 0),
                    },

                    Children = new Drawable[]
                    {
                        new PauseButton
                        {
                            Type = PauseButtonType.Resume,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = (delegate
                            {
                                Hide();
                                OnResume?.Invoke();
                            }),
                        },
                        new PauseButton
                        {
                            Type = PauseButtonType.Retry,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = (delegate
                            {
                                Hide();
                                OnRetry?.Invoke();
                            }),
                        },
                        new PauseButton
                        {
                            Type = PauseButtonType.Quit,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = (delegate
                            {
                                Hide();
                                OnQuit?.Invoke();
                            }),
                        },
                    }
                },
            };
        }

        protected override void PopIn()
        {
            FadeTo(1, fadeDuration, EasingTypes.In);
        }

        protected override void PopOut()
        {
            FadeTo(0, fadeDuration, EasingTypes.In);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    Hide();
                    OnResume?.Invoke();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        public PauseOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            AutoSizeAxes = Axes.Both;
            Depth = -1;
        }
    }
}
