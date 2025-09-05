// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Results;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.RoundResults;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens
{
    public partial class MatchmakingScreenStack : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private ScreenStack screenStack = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding(10);

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.AutoSize) },
                Content = new Drawable[][]
                {
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.Absolute, 20), new Dimension(GridSizeMode.AutoSize) },
                            Padding = new MarginPadding { Bottom = 20 },
                            Content = new Drawable?[][]
                            {
                                [
                                    screenStack = new ScreenStack(),
                                    null,
                                    new PlayerPanelList
                                    {
                                        Horizontal = true,
                                        RelativeSizeAxes = Axes.Y,
                                        Width = 250,
                                        Scale = new Vector2(0.8f),
                                    }
                                ]
                            }
                        }
                    ],
                    [
                        new StageDisplay
                        {
                            RelativeSizeAxes = Axes.X
                        }
                    ]
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            screenStack.Push(new IdleScreen());

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            onMatchRoomStateChanged(client.Room!.MatchState);
        }

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            switch (matchmakingState.Stage)
            {
                case MatchmakingStage.WaitingForClientsJoin:
                case MatchmakingStage.RoundWarmupTime:
                    while (screenStack.CurrentScreen is not IdleScreen)
                        screenStack.Exit();
                    break;

                case MatchmakingStage.UserBeatmapSelect:
                    screenStack.Push(new PickScreen());
                    break;

                case MatchmakingStage.ServerBeatmapFinalised:
                    Debug.Assert(screenStack.CurrentScreen is PickScreen);
                    ((PickScreen)screenStack.CurrentScreen).RollFinalBeatmap(matchmakingState.CandidateItems, matchmakingState.CandidateItem);
                    break;

                case MatchmakingStage.ResultsDisplaying:
                    screenStack.Push(new RoundResultsScreen());
                    break;

                case MatchmakingStage.Ended:
                    screenStack.Push(new ResultsScreen());
                    break;
            }
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
        }
    }
}
