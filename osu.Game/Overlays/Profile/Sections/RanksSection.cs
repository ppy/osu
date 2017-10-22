// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Sections.Ranks;
using System;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Users;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly ScoreContainer best, first;

        public RanksSection()
        {
            Children = new Drawable[]
            {
                best = new ScoreContainer(ScoreType.Best, "Best Performance", true),
                first = new ScoreContainer(ScoreType.Firsts, "First Place Ranks"),
            };
        }

        public override User User
        {
            get
            {
                return base.User;
            }

            set
            {
                base.User = value;
                best.User = value;
                first.User = value;
            }
        }

        private class ScoreContainer : FillFlowContainer
        {
            private readonly FillFlowContainer<DrawableScore> scoreContainer;
            private readonly OsuSpriteText missing;
            private readonly OsuHoverContainer showMoreButton;
            private readonly LoadingAnimation showMoreLoading;

            private readonly ScoreType type;
            private int visiblePages;
            private User user;
            private readonly bool includeWeigth;

            private RulesetStore rulesets;
            private APIAccess api;

            public User User
            {
                set
                {
                    user = value;
                    visiblePages = 0;
                    scoreContainer.Clear();
                    showMoreButton.Hide();
                    missing.Show();
                    showMore();
                }
            }

            public ScoreContainer(ScoreType type, string header, bool includeWeigth = false)
            {
                this.type = type;
                this.includeWeigth = includeWeigth;

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
                        Text = "No awesome performance records yet. :(",
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
                var req = new GetUserScoresRequest(user.Id, type, visiblePages++ * 5);

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
                            scoreContainer.Add(new DrawableScore(score, includeWeigth ? Math.Pow(0.95, scoreContainer.Count) : (double?)null)
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 60,
                            });
                    }
                };

                Schedule(() => { api.Queue(req); });
            }
        }
    }
}
