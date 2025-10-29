// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// A "global" footer staple element in matchmaking which shows the current progression of the room, from start to finish.
    /// </summary>
    public partial class StageDisplay : CompositeDrawable
    {
        public const float HEIGHT = 96;

        // TODO: get this from somewhere?
        private const int round_count = 5;

        private OsuScrollContainer scroll = null!;
        private FillFlowContainer flow = null!;

        private CurrentRoundDisplay roundDisplay = null!;

        public StageDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Dark6,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = HEIGHT,
                    Children = new Drawable[]
                    {
                        scroll = new StageScrollContainer
                        {
                            ScrollbarVisible = false,
                            ClampExtension = 0,
                            RelativeSizeAxes = Axes.X,
                            Height = HEIGHT,
                            Child = flow = new FillFlowContainer
                            {
                                Padding = new MarginPadding { Horizontal = 2000 },
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Direction = FillDirection.Horizontal,
                            },
                        },
                        new TimerText
                        {
                            Y = -38,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new StatusText
                        {
                            Y = 32,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new Box
                        {
                            Colour = ColourInfo.GradientHorizontal(
                                colourProvider.Dark4,
                                colourProvider.Dark5.Opacity(0)
                            ),
                            RelativeSizeAxes = Axes.Y,
                            Width = 240,
                        },
                        roundDisplay = new CurrentRoundDisplay
                        {
                            X = 12,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    }
                },
            };

            flow.Add(new StageSegment(null, MatchmakingStage.WaitingForClientsJoin, "Waiting for other users"));

            for (int i = 1; i <= round_count; i++)
            {
                flow.Add(new StageSegment(i, MatchmakingStage.RoundWarmupTime, "Next Round"));
                flow.Add(new StageSegment(i, MatchmakingStage.UserBeatmapSelect, "Beatmap Selection"));
                flow.Add(new StageSegment(i, MatchmakingStage.GameplayWarmupTime, "Get Ready"));
                flow.Add(new StageSegment(i, MatchmakingStage.ResultsDisplaying, "Results"));
            }

            flow.Add(new StageSegment(round_count, MatchmakingStage.Ended, "Match End"));
        }

        protected override void Update()
        {
            base.Update();
            var bubble = flow.OfType<StageSegment>().FirstOrDefault(b => b.Active);

            if (bubble != null)
            {
                scroll.ScrollTo(flow.Padding.Left + bubble.X + bubble.Progress * bubble.DrawWidth - scroll.DrawWidth / 2);
                roundDisplay.Round = bubble.Round;
            }
        }

        private partial class StageScrollContainer : OsuScrollContainer
        {
            public override bool HandlePositionalInput => false;
            public override bool HandleNonPositionalInput => false;

            public StageScrollContainer()
                : base(Direction.Horizontal)
            {
            }
        }

        private partial class CurrentRoundDisplay : CompositeDrawable
        {
            private OsuSpriteText text = null!;

            private Circle innerCircle = null!;
            private CircularProgress progress = null!;

            private Sample? swishSample;
            private Sample? swooshSample;
            private Sample? roundUpSample;
            private SampleChannel? swishChannel;
            private SampleChannel? swooshChannel;
            private SampleChannel? roundUpChannel;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours, AudioManager audio)
            {
                Size = new Vector2(76);

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Colour = ColourInfo.GradientVertical(
                            colours.Dark2,
                            colours.Dark4
                        ),
                        RelativeSizeAxes = Axes.Both,
                    },
                    progress = new CircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = ColourInfo.GradientVertical(
                            colours.Light1,
                            colours.Dark2
                        ),
                        InnerRadius = 0.1f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    innerCircle = new Circle
                    {
                        Alpha = 0.2f,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = ColourInfo.GradientVertical(
                            colours.Dark1,
                            colours.Dark2
                        ),
                        Scale = new Vector2(0.9f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Y = 10,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.Style.Caption2,
                        Text = "Round",
                    },
                    text = new OsuSpriteText
                    {
                        Font = OsuFont.Style.Heading1,
                        Position = new Vector2(-8, -3),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "1"
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.Style.Heading2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = 4,
                        Text = "/"
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.Style.Heading1,
                        Position = new Vector2(10, 11),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = $"{round_count}"
                    },
                };

                swishSample = audio.Samples.Get(@"UI/overlay-pop-in");
                swooshSample = audio.Samples.Get(@"UI/overlay-big-pop-out");
                roundUpSample = audio.Samples.Get(@"Multiplayer/Matchmaking/round-up");
            }

            private int round;

            public int? Round
            {
                set
                {
                    value ??= 1;

                    if (round == value)
                        return;

                    round = value.Value;

                    this.ScaleTo(6, 1000, Easing.OutPow10)
                        .MoveToY(-300, 1000, Easing.OutPow10)
                        .Then()
                        .MoveToY(0, 500, Easing.InQuart)
                        .ScaleTo(1, 500, Easing.InQuart);

                    swishChannel = swishSample?.GetChannel();

                    if (swishChannel != null)
                    {
                        swishChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH;
                        swishChannel?.Play();
                    }

                    Scheduler.AddDelayed(() =>
                    {
                        swooshChannel = swooshSample?.GetChannel();

                        if (swooshChannel == null) return;

                        swooshChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH;
                        swooshChannel?.Play();
                    }, 1250);

                    Scheduler.AddDelayed(() =>
                    {
                        progress.ProgressTo((float)round / round_count, 500, Easing.InOutQuart);

                        Scheduler.AddDelayed(() =>
                        {
                            roundUpChannel = roundUpSample?.GetChannel();

                            if (roundUpChannel != null)
                            {
                                roundUpChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH;
                                roundUpChannel.Frequency.Value = 1f + round * 0.05f;
                                roundUpChannel?.Play();
                            }

                            innerCircle
                                .FadeTo(1, 250, Easing.OutQuint)
                                .Then()
                                .FadeTo(0.2f, 5000, Easing.OutQuint);

                            text.Text = $"{round}";
                        }, 150);
                    }, 250);
                }
            }
        }
    }
}
