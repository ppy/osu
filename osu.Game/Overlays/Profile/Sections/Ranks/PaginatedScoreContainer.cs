// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class PaginatedScoreContainer : PaginatedContainer
    {
        private readonly bool includeWeight;
        private readonly ScoreType type;
        private GetUserScoresRequest request;

        public Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        public PaginatedScoreContainer(ScoreType type, Bindable<User> user, string header, string missing, bool includeWeight = false)
            : base(user, header, missing)
        {
            this.type = type;
            this.includeWeight = includeWeight;

            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Ruleset.BindValueChanged(rulesetChanged);
        }

        private void rulesetChanged(ValueChangedEvent<RulesetInfo> r)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();

            MoreButton.IsLoading = true;
            ShowMore();
        }

        protected override void ShowMore()
        {
            request?.Cancel();

            request = new GetUserScoresRequest(User.Value.Id, type, VisiblePages++ * ItemsPerPage, Ruleset.Value);
            request.Success += scores => Schedule(() =>
            {
                foreach (var s in scores)
                    s.Ruleset = Rulesets.GetRuleset(s.RulesetID);

                if (!scores.Any() && VisiblePages == 1)
                {
                    MoreButton.Hide();
                    MoreButton.IsLoading = false;
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
                    MoreButton.FadeTo(scores.Count == ItemsPerPage ? 1 : 0);
                    MoreButton.IsLoading = false;

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
