// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.TeamWin;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public partial class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        private LabelledSwitchButton warmupToggle = null!;

        private bool isChatShown = false;

        private TourneyButton chatToggle = null!;

        // private SettingsSlider<float> redMultiplier = null!;
        // private SettingsSlider<float> blueMultiplier = null!;

        private MatchIPCInfo ipc = null!;
        private bool chatEnforcing = false;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private Drawable chroma = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            this.ipc = ipc;

            AddRangeInternal(new Drawable[]
            {
                new TourneyVideo(BackgroundVideo.Gameplay, LadderInfo)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                header = new MatchHeader
                {
                    ShowLogo = false,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Y = 110,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new[]
                    {
                        chroma = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 512,
                            Children = new Drawable[]
                            {
                                new ChromaArea
                                {
                                    Name = "Left chroma",
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                new ChromaArea
                                {
                                    Name = "Right chroma",
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 0.5f,
                                }
                            }
                        },
                    }
                },
                scoreDisplay = new TournamentMatchScoreDisplay
                {
                    Y = -147,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        warmupToggle = new LabelledSwitchButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Label = "Warmup stage",
                            Current = warmup,
                        },
                        chatToggle = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle chat",
                            Action = () => {
                                chatEnforcing = true;
                                if (isChatShown)
                                {
                                    isChatShown = false;
                                    expand();
                                }
                                else
                                {
                                    isChatShown = true;
                                    contract();
                                }
                            }
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Chroma width",
                            Current = LadderInfo.ChromaKeyWidth,
                            KeyboardStep = 1,
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Players per team",
                            Current = LadderInfo.PlayersPerTeam,
                            KeyboardStep = 1,
                        },
                        
                        /* Experimental feature for Live score calculation! See https://github.com/CloneWith/osu/issues/2
                        
                        redMultiplier = new SettingsSlider<float>
                        {
                            LabelText = "Red score multiplier",
                            Current = LadderInfo.RedMultiplier,
                            KeyboardStep = 0.1f,
                        },
                        blueMultiplier = new SettingsSlider<float>
                        {
                            LabelText = "Blue score multiplier",
                            Current = LadderInfo.BlueMultiplier,
                            KeyboardStep = 0.1f,
                        },
                        */
                    }
                }
            });

            LadderInfo.ChromaKeyWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);

            warmup.BindValueChanged(w =>
            {
                header.ShowScores = !w.NewValue;
            }, true);
        }

        private void updateWarmup()
        {
            warmup.Value = warmupToggle.Current.Value;
            updateState();
            warmupToggle.Current.Value = warmup.Value;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            warmupToggle.Current.BindValueChanged(_ => updateWarmup(), true);

            State.BindTo(ipc.State);
            State.BindValueChanged(e => updateState(e), true);
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null)
                return;

            warmup.Value = match.NewValue.Team1Score.Value + match.NewValue.Team2Score.Value == 0;
            scheduledScreenChange?.Cancel();
        }

        private ScheduledDelegate? scheduledScreenChange;
        private ScheduledDelegate? scheduledContract;

        private TournamentMatchScoreDisplay scoreDisplay = null!;

        private TourneyState lastState;
        private MatchHeader header = null!;

        private void contract()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            SongBar.Expanded = false;
            scoreDisplay.FadeOut(100);
            sceneManager?.UpdateChatState(true);
        }

        private void expand()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            sceneManager?.UpdateChatState(false);

            using (BeginDelayedSequence(300))
            {
                scoreDisplay.FadeIn(100);
                SongBar.Expanded = true;
            }
        }

        private void updateState(ValueChangedEvent<TourneyState>? e = null)
        {
            var newState = e != null ? e.NewValue : State.Value;

            try
            {
                scheduledScreenChange?.Cancel();

                if (State.Value == TourneyState.Ranking)
                {
                    if (warmup.Value || CurrentMatch.Value == null) return;

                    if (ipc.Score1.Value > ipc.Score2.Value)
                        CurrentMatch.Value.Team1Score.Value++;
                    else
                        CurrentMatch.Value.Team2Score.Value++;
                }

                switch (State.Value)
                {
                    case TourneyState.Idle:
                        if (!chatEnforcing || lastState == TourneyState.Ranking)
                        {
                            chatEnforcing = false;
                            isChatShown = true;
                            contract();
                        }

                        if (LadderInfo.AutoProgressScreens.Value)
                        {
                            const float delay_before_progression = 4000;

                            // if we've returned to idle and the last screen was ranking
                            // we should automatically proceed after a short delay
                            if (lastState == TourneyState.Ranking && !warmup.Value)
                            {
                                if (CurrentMatch.Value?.Completed.Value == true)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(TeamWinScreen)); }, delay_before_progression);
                                else if (CurrentMatch.Value?.Completed.Value == false)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(MapPoolScreen)); }, delay_before_progression);
                            }
                        }

                        break;

                    case TourneyState.Ranking:
                        scheduledContract = Scheduler.AddDelayed(contract, 10000);
                        break;

                    default:
                        if (e == null || !chatEnforcing)
                        {
                            isChatShown = false;
                            expand();
                        }
                        break;
                }
            }
            finally
            {
                lastState = e != null ? e.NewValue : State.Value;
            }
        }

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }

        public override void Show()
        {
            updateState();
            base.Show();
        }

        private partial class ChromaArea : CompositeDrawable
        {
            [Resolved]
            private LadderInfo ladder { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                // chroma key area for stable gameplay
                Colour = new Color4(0, 255, 0, 255);

                ladder.PlayersPerTeam.BindValueChanged(performLayout, true);
            }

            private void performLayout(ValueChangedEvent<int> playerCount)
            {
                switch (playerCount.NewValue)
                {
                    case 3:
                        InternalChildren = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Height = 0.5f,
                            },
                        };
                        break;

                    default:
                        InternalChild = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        };
                        break;
                }
            }
        }
    }
}
