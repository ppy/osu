// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Schedule
{
    public class ScheduleScreen : TournamentScreen // IProvidesVideo
    {
        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
        private Container mainContainer;
        private LadderInfo ladder;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            this.ladder = ladder;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("schedule")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(100) { Bottom = 50 },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DrawableTournamentTitleText(),
                                            new Container
                                            {
                                                Margin = new MarginPadding { Top = 40 },
                                                AutoSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        Colour = Color4.White,
                                                        Size = new Vector2(50, 10),
                                                    },
                                                    new TournamentSpriteTextWithBackground("Schedule")
                                                    {
                                                        X = 60,
                                                        Scale = new Vector2(0.8f)
                                                    }
                                                }
                                            },
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    mainContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        }
                    }
                },
            };

            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            if (match.NewValue == null)
            {
                mainContainer.Clear();
                return;
            }

            var upcoming = ladder.Matches.Where(p => !p.Completed.Value && p.Team1.Value != null && p.Team2.Value != null && Math.Abs(p.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < 4);
            var conditionals = ladder
                               .Matches.Where(p => !p.Completed.Value && (p.Team1.Value == null || p.Team2.Value == null) && Math.Abs(p.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < 4)
                               .SelectMany(m => m.ConditionalMatches.Where(cp => m.Acronyms.TrueForAll(a => cp.Acronyms.Contains(a))));

            upcoming = upcoming.Concat(conditionals);
            upcoming = upcoming.OrderBy(p => p.Date.Value).Take(8);

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.74f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleContainer("recent matches")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.4f,
                                    ChildrenEnumerable = ladder.Matches
                                                               .Where(p => p.Completed.Value && p.Team1.Value != null && p.Team2.Value != null
                                                                           && Math.Abs(p.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < 4)
                                                               .OrderByDescending(p => p.Date.Value)
                                                               .Take(8)
                                                               .Select(p => new ScheduleMatch(p))
                                },
                                new ScheduleContainer("upcoming matches")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p))
                                },
                            }
                        }
                    },
                    new ScheduleContainer("coming up next")
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.25f,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(30),
                                Children = new Drawable[]
                                {
                                    new ScheduleMatch(match.NewValue, false)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    new TournamentSpriteTextWithBackground(match.NewValue.Round.Value?.Name.Value)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Scale = new Vector2(0.5f)
                                    },
                                    new TournamentSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = match.NewValue.Team1.Value?.FullName + " vs " + match.NewValue.Team2.Value?.FullName,
                                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold)
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            new TournamentSpriteText
                                            {
                                                Text = "Starting ",
                                                Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Regular)
                                            },
                                            new DrawableDate(match.NewValue.Date.Value)
                                            {
                                                Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Regular)
                                            }
                                        }
                                    },
                                }
                            },
                        }
                    }
                }
            };
        }

        public class ScheduleMatch : DrawableTournamentMatch
        {
            public ScheduleMatch(TournamentMatch match, bool showTimestamp = true)
                : base(match)
            {
                Flow.Direction = FillDirection.Horizontal;

                Scale = new Vector2(0.8f);

                CurrentMatchSelectionBox.Scale = new Vector2(1.02f, 1.15f);

                bool conditional = match is ConditionalTournamentMatch;

                if (conditional)
                    Colour = OsuColour.Gray(0.5f);

                if (showTimestamp)
                {
                    AddInternal(new DrawableDate(Match.Date.Value)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Font = OsuFont.Torus,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    });
                    AddInternal(new TournamentSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Text = match.Date.Value.ToUniversalTime().ToString("HH:mm UTC") + (conditional ? " (conditional)" : "")
                    });
                }
            }
        }

        public class ScheduleContainer : Container
        {
            protected override Container<Drawable> Content => content;

            private readonly FillFlowContainer content;

            public ScheduleContainer(string title)
            {
                Padding = new MarginPadding { Left = 60, Top = 10 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteTextWithBackground(title.ToUpperInvariant())
                            {
                                Scale = new Vector2(0.5f)
                            },
                            content = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.Both,
                                Margin = new MarginPadding(10)
                            },
                        }
                    },
                };
            }
        }
    }
}
