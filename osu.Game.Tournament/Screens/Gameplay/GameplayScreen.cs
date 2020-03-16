// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.TeamWin;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public class GameplayScreen : BeatmapInfoScreen, IProvideVideo
    {
        private readonly BindableBool warmup = new BindableBool();

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        private OsuButton warmupButton;
        private MatchIPCInfo ipc;

        [Resolved(canBeNull: true)]
        private TournamentSceneManager sceneManager { get; set; }

        [Resolved]
        private TournamentMatchChatDisplay chat { get; set; }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, MatchIPCInfo ipc, Storage storage)
        {
            this.ipc = ipc;

            AddRangeInternal(new Drawable[]
            {
                new TourneyVideo("gameplay")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                header = new MatchHeader
                {
                    ShowLogo = false
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Y = 110,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            // chroma key area for stable gameplay
                            Name = "chroma",
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 512,
                            Colour = new Color4(0, 255, 0, 255),
                        },
                    }
                },
                scoreDisplay = new MatchScoreDisplay
                {
                    Y = -147,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        warmupButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle warmup",
                            Action = () => warmup.Toggle()
                        },
                        new TourneyButton
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

            currentMatch.BindValueChanged(m =>
            {
                warmup.Value = m.NewValue.Team1Score.Value + m.NewValue.Team2Score.Value == 0;
                scheduledOperation?.Cancel();
            });

            currentMatch.BindTo(ladder.CurrentMatch);

            warmup.BindValueChanged(w =>
            {
                warmupButton.Alpha = !w.NewValue ? 0.5f : 1;
                header.ShowScores = !w.NewValue;
            }, true);
        }

        private ScheduledDelegate scheduledOperation;
        private MatchScoreDisplay scoreDisplay;

        private TourneyState lastState;
        private MatchHeader header;

        private void stateChanged(ValueChangedEvent<TourneyState> state)
        {
            try
            {
                if (state.NewValue == TourneyState.Ranking)
                {
                    if (warmup.Value) return;

                    if (ipc.Score1.Value > ipc.Score2.Value)
                        currentMatch.Value.Team1Score.Value++;
                    else
                        currentMatch.Value.Team2Score.Value++;
                }

                scheduledOperation?.Cancel();

                void expand()
                {
                    chat?.Contract();

                    using (BeginDelayedSequence(300, true))
                    {
                        scoreDisplay.FadeIn(100);
                        SongBar.Expanded = true;
                    }
                }

                void contract()
                {
                    SongBar.Expanded = false;
                    scoreDisplay.FadeOut(100);
                    using (chat?.BeginDelayedSequence(500))
                        chat?.Expand();
                }

                switch (state.NewValue)
                {
                    case TourneyState.Idle:
                        contract();

                        const float delay_before_progression = 4000;

                        // if we've returned to idle and the last screen was ranking
                        // we should automatically proceed after a short delay
                        if (lastState == TourneyState.Ranking && !warmup.Value)
                        {
                            if (currentMatch.Value?.Completed.Value == true)
                                scheduledOperation = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(TeamWinScreen)); }, delay_before_progression);
                            else if (currentMatch.Value?.Completed.Value == false)
                                scheduledOperation = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(MapPoolScreen)); }, delay_before_progression);
                        }

                        break;

                    case TourneyState.Ranking:
                        scheduledOperation = Scheduler.AddDelayed(contract, 10000);
                        break;

                    default:
                        chat.Contract();
                        expand();
                        break;
                }
            }
            finally
            {
                lastState = state.NewValue;
            }
        }
    }
}
