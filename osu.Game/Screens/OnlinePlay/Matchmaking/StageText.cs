// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class StageText : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private OsuSpriteText text = null!;

        public StageText()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = text = new OsuSpriteText
            {
                Height = 16,
                Font = OsuFont.Default,
                AlwaysPresent = true,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            onMatchRoomStateChanged(client.Room!.MatchState);
        }

        private void onMatchRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            text.Text = getTextForStatus(matchmakingState.Stage);
        });

        private LocalisableString getTextForStatus(MatchmakingStage status)
        {
            switch (status)
            {
                case MatchmakingStage.WaitingForClientsJoin:
                    return "Players are joining the match...";

                case MatchmakingStage.WaitingForClientsBeatmapDownload:
                    return "Players are downloading the beatmap...";

                case MatchmakingStage.Gameplay:
                    return "Game is in progress...";

                case MatchmakingStage.Ended:
                    return "Thanks for playing! The match will close shortly.";

                default:
                    return string.Empty;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onMatchRoomStateChanged;
        }
    }
}
