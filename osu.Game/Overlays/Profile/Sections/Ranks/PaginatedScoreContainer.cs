// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

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
                    s.ApplyRuleset(Rulesets.GetRuleset(s.OnlineRulesetID));

                ShowMoreButton.FadeTo(scores.Count == ItemsPerPage ? 1 : 0);
                ShowMoreLoading.Hide();

                if (!scores.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                MissingText.Hide();

                foreach (APIScore score in scores)
                {
                    DrawableProfileScore drawableScore;

                    switch (type)
                    {
                        default:
                            drawableScore = new DrawablePerformanceScore(score, includeWeight ? Math.Pow(0.95, ItemsContainer.Count) : (double?)null);
                            break;
                        case ScoreType.Recent:
                            drawableScore = new DrawableTotalScore(score);
                            break;
                    }

                    ItemsContainer.Add(drawableScore);
                }
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
