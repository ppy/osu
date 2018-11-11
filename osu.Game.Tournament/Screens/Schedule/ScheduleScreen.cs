// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens.Schedule
{
    public class ScheduleScreen : TournamentScreen, IProvideVideo
    {
        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();
        private Container mainContainer;
        private LadderInfo ladder;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            this.ladder = ladder;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new VideoSprite(storage.GetStream(@"BG Side Logo - OWC.m4v"))
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

        private void matchChanged(MatchPairing pairing)
        {
            if (pairing == null)
            {
                mainContainer.Clear();
                return;
            }

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
                                    ChildrenEnumerable = ladder.Pairings
                                                               .Where(p => p.Completed.Value)
                                                               .OrderByDescending(p => p.Date.Value)
                                                               .Take(8)
                                                               .Select(p => new SchedulePairing(p))
                                },
                                new ScheduleContainer("match overview")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    ChildrenEnumerable = ladder.Pairings
                                                               .Where(p => !p.Completed.Value)
                                                               .OrderBy(p => p.Date.Value)
                                                               .Take(8)
                                                               .Select(p => new SchedulePairing(p))
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
                                Text = currentMatch.Value.Grouping.Value.Name.Value,
                                Colour = Color4.Black,
                                TextSize = 20
                            },
                            new SchedulePairing(currentMatch),
                            new OsuSpriteText
                            {
                                Text = "Start Time " + pairing.Date.Value.ToUniversalTime().ToString("HH:mm UTC"),
                                Colour = Color4.Black,
                                TextSize = 20
                            },
                        }
                    }
                }
            };
        }

        public class SchedulePairing : DrawableMatchPairing
        {
            public SchedulePairing(MatchPairing pairing)
                : base(pairing)
            {
                Flow.Direction = FillDirection.Horizontal;
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
                        TextSize = 30
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
