// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Users;
using System;
using System.Linq;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class ScoreContainer : FillFlowContainer
    {
        private readonly FillFlowContainer<DrawableScore> scoreContainer;
        private readonly OsuSpriteText missing;
        private readonly OsuHoverContainer showMoreButton;
        private readonly LoadingAnimation showMoreLoading;

        private readonly bool includeWeight;
        private readonly ScoreType type;
        private int visiblePages;
        private readonly Bindable<User> user;

        private RulesetStore rulesets;
        private APIAccess api;

        private void setUser(User newUser)
        {
            visiblePages = 0;
            scoreContainer.Clear();
            showMoreButton.Hide();
            missing.Show();
            showMore();
        }

        public ScoreContainer(ScoreType type, Bindable<User> user, string header, bool includeWeight = false)
        {
            this.type = type;
            this.includeWeight = includeWeight;
            this.user = user;
            user.ValueChanged += setUser;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                    new OsuSpriteText
                    {
                        TextSize = 15,
                        Text = header,
                        Font = "Exo2.0-RegularItalic",
                        Margin = new MarginPadding { Top = 10, Bottom = 10 },
                    },
                    scoreContainer = new FillFlowContainer<DrawableScore>
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                    },
                    showMoreButton = new OsuHoverContainer
                    {
                        Alpha = 0,
                        Action = showMore,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Child = new OsuSpriteText
                        {
                            TextSize = 14,
                            Text = "show more",
                        }
                    },
                    showMoreLoading = new LoadingAnimation
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(14),
                    },
                    missing = new OsuSpriteText
                    {
                        TextSize = 14,
                        Text = type == ScoreType.Recent ? "No performance records. :(" : "No awesome performance records yet. :(",
                    },
            };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, RulesetStore rulesets)
        {
            this.api = api;
            this.rulesets = rulesets;
        }

        private void showMore()
        {
            var req = new GetUserScoresRequest(user.Value.Id, type, visiblePages++ * 5);

            showMoreLoading.Show();
            showMoreButton.Hide();

            req.Success += scores =>
            {
                foreach (var s in scores)
                    s.ApplyRuleset(rulesets.GetRuleset(s.OnlineRulesetID));

                showMoreButton.FadeTo(scores.Count == 5 ? 1 : 0);
                showMoreLoading.Hide();

                if (scores.Any())
                {
                    missing.Hide();
                    foreach (OnlineScore score in scores)
                    {
                        var drawableScore = type == ScoreType.Recent ? (DrawableScore)new TotalScore(score) : new DrawablePerformanceScore(score, includeWeight ? Math.Pow(0.95, scoreContainer.Count) : (double?)null);
                        drawableScore.RelativeSizeAxes = Axes.X;
                        drawableScore.Height = 60;
                        scoreContainer.Add(drawableScore);
                    }
                }
            };

            Schedule(() => { api.Queue(req); });
        }
    }
}
