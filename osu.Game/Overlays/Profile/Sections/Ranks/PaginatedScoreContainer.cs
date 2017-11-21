﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using System.Linq;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class PaginatedScoreContainer : PaginatedContainer
    {
        private readonly bool includeWeight;
        private readonly ScoreType type;
        private Mode playMode;
        private GetUserScoresRequest request;

        public PaginatedScoreContainer(ScoreType type, Bindable<User> user, string header, string missing, bool includeWeight = false)
            : base(user, header, missing)
        {
            this.type = type;
            this.includeWeight = includeWeight;

            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        public void ApplyPlaymode(Mode playMode)
        {
            request?.Cancel();
            this.playMode = playMode;
            TriggerUserChange();
        }

        protected override void ShowMore()
        {
            base.ShowMore();

            request = new GetUserScoresRequest(User.Value.Id, type, playMode, VisiblePages++ * ItemsPerPage);

            request.Success += scores =>
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

                foreach (OnlineScore score in scores)
                {
                    DrawableScore drawableScore;

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
            };

            Api.Queue(request);
        }
    }
}
