//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using System;
using osu.Game.Overlays.Pause;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Play
{
    class FailDialog : OsuGameMode
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private static readonly Vector2 BACKGROUND_BLUR = new Vector2(20);

        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        private OsuColour colour;

        public FailDialog()
        {
            colour = new OsuColour();

            Add(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = background_alpha,
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirection.VerticalOnly,
                    Spacing = new Vector2(0f, 50f),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0f, 20f),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = @"failed",
                                    Font = @"Exo2.0-Medium",
                                    Spacing = new Vector2(5, 0),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextSize = 30,
                                    Colour = colour.Yellow,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                },
                                new SpriteText
                                {
                                    Text = @"you're failed",
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                }
                            }
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.6f),
                                Radius = 50
                            },
                            Children = new Drawable[]
                            {
                                new RetryButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = button_height,
                                    Action = Retry
                                },
                                new QuitButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = button_height,
                                    Action = Exit
                                }
                            }
                        }
                    }
                }
            });
        }

        public void Retry()
        {
            Player player = new Player();

            player.Preload(Game, delegate
            {
                if (!Push(player))
                {
                    player = null;
                    //error occured?
                }
            });
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            Background.Schedule(() => (Background as BackgroundModeBeatmap)?.BlurTo(BACKGROUND_BLUR, 1000));
        }

        protected override bool OnExiting(GameMode next)
        {
            Background.Schedule(() => Background.FadeColour(Color4.White, 500));
            return base.OnExiting(next);
        }
    }
}
