// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DailyChallengeStatsDisplay : CompositeDrawable, IHasCustomTooltip<DailyChallengeTooltipData>
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public DailyChallengeTooltipData? TooltipContent { get; private set; }

        private OsuSpriteText dailyPlayCount = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

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
                            // can't use this because osu-web does weird stuff with \\n.
                            // Text = UsersStrings.ShowDailyChallengeTitle.,
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
                                dailyPlayCount = new OsuSpriteText
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

            APIUserDailyChallengeStatistics stats = User.Value.User.DailyChallengeStatistics;

            if (stats.PlayCount == 0)
            {
                Hide();
                return;
            }

            dailyPlayCount.Text = DailyChallengeStatsDisplayStrings.UnitDay(stats.PlayCount.ToLocalisableString("N0"));
            dailyPlayCount.Colour = colours.ForRankingTier(TierForPlayCount(stats.PlayCount));

            TooltipContent = new DailyChallengeTooltipData(colourProvider, stats);

            Show();
        }

        // Rounding up is needed here to ensure the overlay shows the same colour as osu-web for the play count.
        // This is because, for example, 31 / 3 > 10 in JavaScript because floats are used, while here it would
        // get truncated to 10 with an integer division and show a lower tier.
        public static RankingTier TierForPlayCount(int playCount) => DailyChallengeStatsTooltip.TierForDaily((int)Math.Ceiling(playCount / 3.0d));

        public ITooltip<DailyChallengeTooltipData> GetCustomTooltip() => new DailyChallengeStatsTooltip();
    }
}
