// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly ScoreFlowContainer best, first;
        private readonly OsuSpriteText bestMissing, firstMissing;

        private APIAccess api;
        private RulesetStore rulesets;

        public RanksSection()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    TextSize = 15,
                    Text = "Best Performance",
                    Font = "Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                },
                best = new ScoreFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                bestMissing = new OsuSpriteText
                {
                    TextSize = 14,
                    Text = "No awesome performance records yet. :(",
                },
                new OsuSpriteText
                {
                    TextSize = 15,
                    Text = "First Place Ranks",
                    Font = "Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Top = 20, Bottom = 10 },
                },
                first = new ScoreFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                firstMissing = new OsuSpriteText
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

        public override User User
        {
            get
            {
                return base.User;
            }

            set
            {
                base.User = value;

                // fetch online ranks
                foreach (ScoreType m in new[] { ScoreType.Best, ScoreType.Firsts })
                {
                    ScoreType thisType = m;
                    var req = new GetUserScoresRequest(User.Id, m);
                    req.Success += scores =>
                    {
                        foreach (var s in scores)
                            s.ApplyRuleset(rulesets.GetRuleset(s.OnlineRulesetID));

                        switch (thisType)
                        {
                            case ScoreType.Best:
                                ScoresBest = scores;
                                break;
                            case ScoreType.Firsts:
                                ScoresFirst = scores;
                                break;
                        }
                    };

                    Schedule(() => { api.Queue(req); });
                }
            }
        }


        public IEnumerable<Score> ScoresBest
        {
            set
            {
                best.Clear();
                if (!value.Any())
                {
                    bestMissing.Show();
                }
                else
                {
                    bestMissing.Hide();
                    int i = 0;
                    foreach (Score score in value)
                    {
                        best.Add(new DrawableScore(score, Math.Pow(0.95, i))
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                            Alpha = 0,
                        });
                        i++;
                    }
                }
                best.ShowMore();
            }
        }

        public IEnumerable<Score> ScoresFirst
        {
            set
            {
                first.Clear();
                if (!value.Any())
                {
                    firstMissing.Show();
                }
                else
                {
                    firstMissing.Hide();
                    foreach (Score score in value)
                        first.Add(new DrawableScore(score)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                            Alpha = 0,
                        });
                }
                first.ShowMore();
            }
        }

        public class ScoreFlowContainer : Container<DrawableScore>
        {
            private FillFlowContainer<DrawableScore> scores;
            private OsuClickableContainer showMoreText;

            protected override Container<DrawableScore> Content => scores;

            protected override void LoadComplete()
            {
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        scores = new FillFlowContainer<DrawableScore>
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,
                        },
                    },
                };
            }

            public override void Clear(bool disposeChildren)
            {
                base.Clear(disposeChildren);
                if (showMoreText == null)
                    ((FillFlowContainer)InternalChild).Add(showMoreText = new OsuHoverContainer
                    {
                        Action = ShowMore,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Child = new OsuSpriteText
                        {
                            TextSize = 14,
                            Text = "show more",
                        }
                    });
                else
                    showMoreText.Show();
            }

            public void ShowMore() => showMoreText.Alpha = Children.Where(d => !d.IsPresent).Where((d, i) => (d.Alpha = i < 5 ? 1 : 0) == 0).Any() ? 1 : 0;
        }
    }
}
