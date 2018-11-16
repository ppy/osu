// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        private TriangleButton warmupButton;
        private MatchIPCInfo ipc;

        private readonly Color4 red = new Color4(186, 0, 18, 255);
        private readonly Color4 blue = new Color4(17, 136, 170, 255);

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures, MatchIPCInfo ipc)
        {
            this.ipc = ipc;
            AddRange(new Drawable[]
            {
                new MatchHeader(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    Name = "top bar red",
                                    RelativeSizeAxes = Axes.X,
                                    Height = 10,
                                    Width = 0.5f,
                                    Colour = red,
                                },
                                new Circle
                                {
                                    Name = "top bar blue",
                                    RelativeSizeAxes = Axes.X,
                                    Height = 10,
                                    Width = 0.5f,
                                    Colour = blue,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                            }
                        },
                        new Box
                        {
                            // chroma key area for stable gameplay
                            Name = "chroma",
                            RelativeSizeAxes = Axes.X,
                            Height = 480,
                            Colour = new Color4(0, 255, 0, 255),
                        },
                    }
                },
                chat = new MatchChatDisplay
                {
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.45f, 120),
                    Margin = new MarginPadding(10),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        warmupButton = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle warmup",
                            Action = () => warmup.Toggle()
                        },
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle chat",
                            Action = () => { State.Value = State.Value == TourneyState.Idle ? TourneyState.Playing : TourneyState.Idle; }
                        }
                    }
                }
            });

            State.BindTo(ipc.State);
            State.BindValueChanged(stateChanged, true);

            currentMatch.BindValueChanged(m => warmup.Value = m.Team1Score + m.Team2Score == 0);
            currentMatch.BindTo(ladder.CurrentMatch);

            warmup.BindValueChanged(w => warmupButton.Alpha = !w ? 0.5f : 1, true);
        }

        private ScheduledDelegate scheduledBarContract;
        private MatchChatDisplay chat;

        private void stateChanged(TourneyState state)
        {
            if (state == TourneyState.Ranking)
            {
                if (warmup.Value) return;

                if (ipc.Score1 > ipc.Score2)
                    currentMatch.Value.Team1Score.Value++;
                else
                    currentMatch.Value.Team2Score.Value++;
            }

            scheduledBarContract?.Cancel();

            switch (state)
            {
                case TourneyState.Idle:
                    // show chat
                    SongBar.Expanded = false;
                    using (chat.BeginDelayedSequence(500))
                    {
                        chat.FadeIn(300);
                        chat.MoveToY(100).MoveToY(0, 500, Easing.OutQuint);
                    }

                    break;
                case TourneyState.Ranking:
                    scheduledBarContract = Scheduler.AddDelayed(() => SongBar.Expanded = false, 15000);
                    break;
                default:
                    chat.FadeOut(200);
                    chat.MoveToY(100, 500, Easing.In);
                    using (SongBar.BeginDelayedSequence(300, true))
                        SongBar.Expanded = true;
                    break;
            }
        }
    }
}
