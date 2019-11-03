// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class BeatmapLeaderboard : Leaderboard<BeatmapLeaderboardScope, ScoreInfo>
    {
        public Action<ScoreInfo> ScoreSelected;

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                Scores = null;

                UpdateScores();
            }
        }

        public APILegacyUserTopScoreInfo TopScore
        {
            get => topScoreContainer.Score.Value;
            set
            {
                if (value == null)
                    topScoreContainer.Hide();
                else
                {
                    topScoreContainer.Show();
                    topScoreContainer.Score.Value = value;
                }
            }
        }

        private bool filterMods;

        private UserTopScoreContainer topScoreContainer;

        /// <summary>
        /// Whether to apply the game's currently selected mods as a filter when retrieving scores.
        /// </summary>
        public bool FilterMods
        {
            get => filterMods;
            set
            {
                if (value == filterMods)
                    return;

                filterMods = value;

                UpdateScores();
            }
        }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.ValueChanged += _ => UpdateScores();
            mods.ValueChanged += _ =>
            {
                if (filterMods)
                    UpdateScores();
            };

            Content.Add(topScoreContainer = new UserTopScoreContainer
            {
                ScoreSelected = s => ScoreSelected?.Invoke(s)
            });
        }

        protected override void Reset()
        {
            base.Reset();
            TopScore = null;
        }

        protected override bool IsOnlineScope => Scope != BeatmapLeaderboardScope.Local;

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            if (Beatmap == null)
            {
                PlaceholderState = PlaceholderState.NoneSelected;
                return null;
            }

            if (Scope == BeatmapLeaderboardScope.Local)
            {
                var scores = scoreManager
                    .QueryScores(s => !s.DeletePending && s.Beatmap.ID == Beatmap.ID && s.Ruleset.ID == ruleset.Value.ID);

                if (filterMods && !mods.Value.Any())
                {
                    // we need to filter out all scores that have any mods to get all local nomod scores
                    scores = scores.Where(s => !s.Mods.Any());
                }
                else if (filterMods)
                {
                    // otherwise find all the scores that have *any* of the currently selected mods (similar to how web applies mod filters)
                    // we're creating and using a string list representation of selected mods so that it can be translated into the DB query itself
                    var selectedMods = mods.Value.Select(m => m.Acronym);
                    scores = scores.Where(s => s.Mods.Any(m => selectedMods.Contains(m.Acronym)));
                }

                Scores = scores.OrderByDescending(s => s.TotalScore).ToArray();
                PlaceholderState = Scores.Any() ? PlaceholderState.Successful : PlaceholderState.NoScores;

                return null;
            }

            if (api?.IsLoggedIn != true)
            {
                PlaceholderState = PlaceholderState.NotLoggedIn;
                return null;
            }

            if (Beatmap.OnlineBeatmapID == null || Beatmap?.Status <= BeatmapSetOnlineStatus.Pending)
            {
                PlaceholderState = PlaceholderState.Unavailable;
                return null;
            }

            if (!api.LocalUser.Value.IsSupporter && (Scope != BeatmapLeaderboardScope.Global || filterMods))
            {
                PlaceholderState = PlaceholderState.NotSupporter;
                return null;
            }

            IReadOnlyList<Mod> requestMods = null;

            if (filterMods && !mods.Value.Any())
                // add nomod for the request
                requestMods = new Mod[] { new ModNoMod() };
            else if (filterMods)
                requestMods = mods.Value;

            var req = new GetScoresRequest(Beatmap, ruleset.Value ?? Beatmap.Ruleset, Scope, requestMods);

            req.Success += r =>
            {
                scoresCallback?.Invoke(r.Scores);
                TopScore = r.UserScore;
            };

            return req;
        }

        protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index) => new LeaderboardScore(model, index)
        {
            Action = () => ScoreSelected?.Invoke(model)
        };
    }
}
