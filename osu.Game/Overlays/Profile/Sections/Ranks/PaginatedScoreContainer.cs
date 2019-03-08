// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class PaginatedScoreContainer : PaginatedContainer
    {
        private readonly bool includeWeight;
        private readonly ScoreType type;
        private GetUserScoresRequest request;

        public PaginatedScoreContainer(ScoreType type, Bindable<User> user, string header, string missing, bool includeWeight = false)
            : base(user, header, missing)
        {
            this.type = type;
            this.includeWeight = includeWeight;

            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override void ShowMore()
        {
            base.ShowMore();

            request = new GetUserScoresRequest(User.Value.Id, type, VisiblePages++ * ItemsPerPage);
            request.Success += scores => Schedule(() =>
            {
                foreach (var s in scores)
                    s.Ruleset = Rulesets.GetRuleset(s.RulesetID);

                if (!scores.Any() && VisiblePages == 1)
                {
                    ShowMoreButton.Hide();
                    ShowMoreLoading.Hide();
                    MissingText.Show();
                    return;
                }

                IEnumerable<DrawableProfileScore> drawableScores;

                switch (type)
                {
                    default:
                        drawableScores = scores.Select(score => new DrawablePerformanceScore(score, includeWeight ? Math.Pow(0.95, ItemsContainer.Count) : (double?)null));
                        break;
                    case ScoreType.Recent:
                        drawableScores = scores.Select(score => new DrawableTotalScore(score));
                        break;
                }

                LoadComponentsAsync(drawableScores, s =>
                {
                    MissingText.Hide();
                    ShowMoreButton.FadeTo(scores.Count == ItemsPerPage ? 1 : 0);
                    ShowMoreLoading.Hide();

                    ItemsContainer.AddRange(s);
                });
            });

            Api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
