// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Select.Leaderboards
{
    [LongRunningLoad]
    public partial class PlaylistsGameplayLeaderboardProvider : Component, IGameplayLeaderboardProvider
    {
        public IBindableList<GameplayLeaderboardScore> Scores => scores;
        private readonly BindableList<GameplayLeaderboardScore> scores = new BindableList<GameplayLeaderboardScore>();

        private readonly Room room;
        private readonly PlaylistItem playlistItem;

        private readonly Cached sorting = new Cached();
        private bool isPartial;

        public PlaylistsGameplayLeaderboardProvider(Room room, PlaylistItem playlistItem)
        {
            this.room = room;
            this.playlistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, GameplayState? gameplayState)
        {
            var scoresRequest = new IndexPlaylistScoresRequest(room.RoomID!.Value, playlistItem.ID);
            scoresRequest.Success += response =>
            {
                var newScores = new List<GameplayLeaderboardScore>();

                isPartial = response.Scores.Count < response.TotalScores;

                for (int i = 0; i < response.Scores.Count; i++)
                {
                    var score = response.Scores[i];
                    score.Position = i + 1;
                    newScores.Add(new GameplayLeaderboardScore(score, tracked: false, GameplayLeaderboardScore.ComboDisplayMode.Highest));
                }

                if (response.UserScore != null && response.Scores.All(s => s.ID != response.UserScore.ID))
                    newScores.Add(new GameplayLeaderboardScore(response.UserScore, tracked: false, GameplayLeaderboardScore.ComboDisplayMode.Highest));

                scores.AddRange(newScores);
            };
            api.Perform(scoresRequest);

            if (gameplayState != null)
            {
                var localScore = new GameplayLeaderboardScore(gameplayState, tracked: true, GameplayLeaderboardScore.ComboDisplayMode.Highest);
                localScore.TotalScore.BindValueChanged(_ => sorting.Invalidate());
                scores.Add(localScore);
            }

            Scheduler.AddDelayed(sort, 1000, true);
        }

        // logic shared with SoloGameplayLeaderboardProvider
        private void sort()
        {
            if (sorting.IsValid)
                return;

            var orderedByScore = scores
                                 .OrderByDescending(i => i.TotalScore.Value)
                                 .ThenBy(i => i.TotalScoreTiebreaker)
                                 .ToList();

            int delta = 0;

            for (int i = 0; i < orderedByScore.Count; i++)
            {
                var score = orderedByScore[i];

                // see `SoloResultsScreen.FetchScores()` for another place that does the same thing with slight deviations
                // if this code is changed, that code should probably be changed as well

                score.DisplayOrder.Value = i + 1;

                // if we know we have all scores there can ever be, we can do the simple and obvious thing.
                if (!isPartial)
                    score.Position.Value = i + 1;
                else
                {
                    // we have a partial leaderboard, with potential gaps.
                    // we have initial score positions which were valid at the point of starting play.
                    // the assumption here is that non-tracked scores here cannot move around, only tracked ones can.
                    if (score.Tracked)
                    {
                        int? previousScorePosition = i > 0 ? orderedByScore[i - 1].InitialPosition : 0;
                        int? nextScorePosition = i < orderedByScore.Count - 1 ? orderedByScore[i + 1].InitialPosition : null;

                        // if the tracked score is perfectly between two scores which have known neighbouring initial positions,
                        // we can assign it the position of the previous score plus one...
                        if (previousScorePosition != null && nextScorePosition != null && previousScorePosition + 1 == nextScorePosition)
                        {
                            score.Position.Value = previousScorePosition + 1;
                            // but we also need to ensure all subsequent scores get shifted down one position, too.
                            delta++;
                        }
                        // conversely, if the tracked score is not between neighbouring two scores and the leaderboard is partial,
                        // we can't really assign a valid position at all. it could be any number between the two neighbours.
                        else
                            score.Position.Value = null;
                    }
                    // for non-tracked scores, we just need to apply any delta that might have come from the tracked scores
                    // which might have been encountered and assigned a position earlier.
                    else
                        score.Position.Value = score.InitialPosition + delta;
                }
            }

            sorting.Validate();
        }
    }
}
