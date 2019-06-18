// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Rounds
{
    public class RoundEditorScreen : TournamentScreen, IProvideVideo
    {
        private readonly FillFlowContainer<RoundRow> items;

        public RoundEditorScreen()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = items = new FillFlowContainer<RoundRow>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        LayoutDuration = 200,
                        LayoutEasing = Easing.OutQuint,
                    },
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Add new",
                            Action = addNew
                        },
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var r in LadderInfo.Rounds)
                items.Add(new RoundRow(r));
        }

        private void addNew()
        {
            var round = new TournamentRound
            {
                StartDate =
                {
                    Value = DateTimeOffset.UtcNow
                }
            };

            items.Add(new RoundRow(round));
            LadderInfo.Rounds.Add(round);
        }

        public class RoundRow : CompositeDrawable
        {
            public readonly TournamentRound Round;

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public RoundRow(TournamentRound round)
            {
                Margin = new MarginPadding(10);

                Round = round;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Padding = new MarginPadding { Right = 160 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.33f,
                                Bindable = Round.Name
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Description",
                                Width = 0.33f,
                                Bindable = Round.Description
                            },
                            new DateTextBox
                            {
                                LabelText = "Start Time",
                                Width = 0.33f,
                                Bindable = Round.StartDate
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Best of",
                                Width = 0.33f,
                                Bindable = Round.BestOf
                            },
                        }
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = "Delete",
                        Action = () =>
                        {
                            Expire();
                            ladderInfo.Rounds.Remove(Round);
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
