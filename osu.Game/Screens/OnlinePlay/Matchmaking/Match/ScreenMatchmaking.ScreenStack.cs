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
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.RoundResults;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.RoundWarmup;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class ScreenMatchmaking
    {
        public partial class ScreenStack : CompositeDrawable
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private Framework.Screens.ScreenStack screenStack = null!;
            private PlayerPanelOverlay playersList = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(6)
                        {
                            Bottom = StageDisplay.HEIGHT + 6,
                        },
                        Children = new Drawable[]
                        {
                            screenStack = new Framework.Screens.ScreenStack(),
                        }
                    },
                    playersList = new PlayerPanelOverlay
                    {
                        DisplayArea = this
                    },
                    new StageDisplay
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                screenStack.ScreenPushed += onScreenPushed;
                screenStack.ScreenExited += onScreenExited;

                screenStack.Push(new SubScreenRoundWarmup());

                client.MatchRoomStateChanged += onMatchRoomStateChanged;
                onMatchRoomStateChanged(client.Room!.MatchState);
            }

            private void onScreenPushed(IScreen lastScreen, IScreen newScreen)
            {
                if (newScreen is not MatchmakingSubScreen matchmakingSubScreen)
                    return;

                playersList.DisplayStyle = matchmakingSubScreen.PlayersDisplayStyle;
                playersList.DisplayArea = matchmakingSubScreen.PlayersDisplayArea;
            }

            private void onScreenExited(IScreen lastScreen, IScreen newScreen)
            {
                if (newScreen is not MatchmakingSubScreen matchmakingSubScreen)
                    return;

                playersList.DisplayStyle = matchmakingSubScreen.PlayersDisplayStyle;
                playersList.DisplayArea = matchmakingSubScreen.PlayersDisplayArea;
            }

            private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
            {
                if (state is not MatchmakingRoomState matchmakingState)
                    return;

                switch (matchmakingState.Stage)
                {
                    case MatchmakingStage.WaitingForClientsJoin:
                    case MatchmakingStage.RoundWarmupTime:
                        while (screenStack.CurrentScreen is not SubScreenRoundWarmup)
                            screenStack.Exit();
                        break;

                    case MatchmakingStage.UserBeatmapSelect:
                        screenStack.Push(new SubScreenBeatmapSelect());
                        break;

                    case MatchmakingStage.ServerBeatmapFinalised:
                        Debug.Assert(screenStack.CurrentScreen is SubScreenBeatmapSelect);
                        ((SubScreenBeatmapSelect)screenStack.CurrentScreen).RollFinalBeatmap(matchmakingState.CandidateItems, matchmakingState.CandidateItem);
                        break;

                    case MatchmakingStage.ResultsDisplaying:
                        screenStack.Push(new SubScreenRoundResults());
                        break;

                    case MatchmakingStage.Ended:
                        screenStack.Push(new SubScreenResults());
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
}
