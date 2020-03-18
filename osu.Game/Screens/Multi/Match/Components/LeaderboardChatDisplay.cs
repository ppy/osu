// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class LeaderboardChatDisplay : MultiplayerComposite
    {
        private const double fade_duration = 100;

        private readonly OsuTabControl<DisplayMode> tabControl;
        private readonly MatchLeaderboard leaderboard;
        private readonly MatchChatDisplay chat;

        public LeaderboardChatDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        tabControl = new DisplayModeTabControl
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 24,
                        }
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 10 },
                            Children = new Drawable[]
                            {
                                leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both },
                                chat = new MatchChatDisplay
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0
                                }
                            }
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabControl.AccentColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            tabControl.Current.BindValueChanged(changeTab);
        }

        public void RefreshScores() => leaderboard.RefreshScores();

        private void changeTab(ValueChangedEvent<DisplayMode> mode)
        {
            chat.FadeTo(mode.NewValue == DisplayMode.Chat ? 1 : 0, fade_duration);
            leaderboard.FadeTo(mode.NewValue == DisplayMode.Leaderboard ? 1 : 0, fade_duration);
        }

        private class DisplayModeTabControl : OsuTabControl<DisplayMode>
        {
            protected override TabItem<DisplayMode> CreateTabItem(DisplayMode value) => base.CreateTabItem(value).With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            });
        }

        private enum DisplayMode
        {
            Leaderboard,
            Chat,
        }
    }
}
