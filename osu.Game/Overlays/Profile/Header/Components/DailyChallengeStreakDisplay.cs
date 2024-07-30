// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DailyChallengeStreakDisplay : CompositeDrawable, IHasCustomTooltip<DailyChallengeStreakTooltipData>
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public DailyChallengeStreakTooltipData? TooltipContent { get; private set; }

        private OsuSpriteText dailyStreak = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 5;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(5f),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                        {
                            AutoSizeAxes = Axes.Both,
                            // Text = UsersStrings.ShowDailyChallengeTitle
                            Text = "Daily\nChallenge",
                            Margin = new MarginPadding { Horizontal = 5f, Bottom = 2f },
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            CornerRadius = 5f,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background6,
                                },
                                dailyStreak = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    UseFullGlyphHeight = false,
                                    Colour = colourProvider.Content2,
                                    Margin = new MarginPadding { Horizontal = 10f, Vertical = 5f },
                                },
                            }
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (User.Value == null || User.Value.Ruleset.OnlineID != 0)
            {
                Hide();
                return;
            }

            var statistics = User.Value.User.DailyChallengeStatistics;
            // dailyStreak.Text = UsersStrings.ShowDailyChallengeUnitDay(statistics.PlayCount);
            dailyStreak.Text = $"{statistics.PlayCount}d";
            TooltipContent = new DailyChallengeStreakTooltipData(colourProvider, statistics);
            Show();
        }

        public ITooltip<DailyChallengeStreakTooltipData> GetCustomTooltip() => new DailyChallengeStreakTooltip();
    }
}
