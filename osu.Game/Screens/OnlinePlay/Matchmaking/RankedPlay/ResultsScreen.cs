// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen : RankedPlaySubScreen
    {
        public override LocalisableString StageHeading => "Results";

        public override bool ShowBeatmapBackground => true;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> globalRuleset { get; set; } = null!;

        private LoadingSpinner loadingSpinner = null!;
        private MainPanel? mainPanel;

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerPieceVisibility.Value = Visibility.Hidden;

            AddInternal(loadingSpinner = new LoadingSpinner
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadingSpinner.Show();

            fetchFinalScores().FireAndForget();
        }

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private async Task fetchFinalScores()
        {
            try
            {
                if (client.Room == null)
                    return;

                TaskCompletionSource<List<MultiplayerScore>> scoreLookup = new TaskCompletionSource<List<MultiplayerScore>>();

                var request = new IndexPlaylistScoresRequest(client.Room.RoomID, client.Room.Settings.PlaylistItemId);

                request.Success += req => scoreLookup.SetResult(req.Scores);
                request.Failure += scoreLookup.SetException;

                api.Queue(request);

                List<MultiplayerScore> apiScores = await scoreLookup.Task.ConfigureAwait(false);

                ScoreInfo[] scores = apiScores.Select(s => s.CreateScoreInfo(scoreManager, rulesets, working.Value.BeatmapInfo)).ToArray();

                Debug.Assert(scores.Length <= 2);

                int localUserId = api.LocalUser.Value.OnlineID;
                int opponentId = matchInfo.RoomState.Users.Keys.Single(it => it != localUserId);

                ScoreInfo playerScore = scores.SingleOrDefault(s => s.UserID == localUserId) ?? new ScoreInfo
                {
                    Rank = ScoreRank.F,
                    Ruleset = globalRuleset.Value,
                    User = new APIUser { Id = localUserId }
                };

                ScoreInfo opponentScore = scores.SingleOrDefault(s => s.UserID == opponentId) ?? new ScoreInfo
                {
                    Rank = ScoreRank.F,
                    Ruleset = globalRuleset.Value,
                    User = new APIUser { Id = opponentId }
                };

                Schedule(() =>
                {
                    LoadComponentAsync(new MainPanel
                    {
                        RelativeSizeAxes = Axes.Both,
                        // A little bit of room for the countdown timer...
                        Margin = new MarginPadding { Top = 45 },
                        PlayerScore = playerScore,
                        OpponentScore = opponentScore,
                        PlayerDamageInfo = matchInfo.RoomState.Users[localUserId].DamageInfo!,
                        OpponentDamageInfo = matchInfo.RoomState.Users[opponentId].DamageInfo!,
                    }, loaded =>
                    {
                        AddInternal(loaded);
                        mainPanel = loaded;
                    });
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load scores for playlist item.");
                throw;
            }
            finally
            {
                Scheduler.Add(() => loadingSpinner.Hide());
            }
        }

        public override void OnExiting(RankedPlaySubScreen? next)
        {
            mainPanel?.StopAllSamples();
            base.OnExiting(next);
        }
    }
}
