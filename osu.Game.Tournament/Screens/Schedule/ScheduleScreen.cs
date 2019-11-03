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
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Schedule
{
    public class ScheduleScreen : TournamentScreen, IProvideVideo
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
                new TourneyVideo(storage.GetStream(@"BG Side Logo - OWC.m4v"))
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
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
            upcoming = upcoming.OrderBy(p => p.Date.Value).Take(12);

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.65f,
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
                                new ScheduleContainer("match overview")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p))
                                },
                            }
                        }
                    },
                    new ScheduleContainer("current match")
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.25f,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Margin = new MarginPadding { Left = -10, Bottom = 10, Top = -5 },
                                Spacing = new Vector2(10, 0),
                                Text = match.NewValue.Round.Value?.Name.Value,
                                Colour = Color4.Black,
                                Font = OsuFont.GetFont(size: 20)
                            },
                            new ScheduleMatch(match.NewValue, false),
                            new OsuSpriteText
                            {
                                Text = "Start Time " + match.NewValue.Date.Value.ToUniversalTime().ToString("HH:mm UTC"),
                                Colour = Color4.Black,
                                Font = OsuFont.GetFont(size: 20)
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

                bool conditional = match is ConditionalTournamentMatch;

                if (conditional)
                    Colour = OsuColour.Gray(0.5f);

                if (showTimestamp)
                {
                    AddInternal(new DrawableDate(Match.Date.Value)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopLeft,
                        Colour = Color4.Black,
                        Alpha = conditional ? 0.6f : 1,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    });
                    AddInternal(new OsuSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = Color4.Black,
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
                Padding = new MarginPadding { Left = 30, Top = 30 };
                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        X = 30,
                        Text = title,
                        Colour = Color4.Black,
                        Spacing = new Vector2(10, 0),
                        Font = OsuFont.GetFont(size: 30)
                    },
                    content = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.Both,
                        Margin = new MarginPadding(40)
                    },
                    new Circle
                    {
                        Colour = new Color4(233, 187, 79, 255),
                        Width = 5,
                        RelativeSizeAxes = Axes.Y,
                    }
                };
            }
        }
    }
}
