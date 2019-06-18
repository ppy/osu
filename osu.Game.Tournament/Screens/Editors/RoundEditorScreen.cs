// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public class RoundEditorScreen : TournamentEditorScreen<RoundEditorScreen.RoundRow>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var r in LadderInfo.Rounds)
                Flow.Add(new RoundRow(r));
        }

        protected override void AddNew()
        {
            var round = new TournamentRound
            {
                StartDate = { Value = DateTimeOffset.UtcNow }
            };

            Flow.Add(new RoundRow(round));
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
                        Text = "Delete Round",
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
