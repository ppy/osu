// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ExtendedDetails : CompositeDrawable
    {
        public Bindable<UserProfileData?> User { get; } = new Bindable<UserProfileData?>();

        private SpriteText rankedScore = null!;
        private SpriteText hitAccuracy = null!;
        private SpriteText playCount = null!;
        private SpriteText totalScore = null!;
        private SpriteText totalHits = null!;
        private SpriteText hitsPerPlay = null!;
        private SpriteText maximumCombo = null!;
        private SpriteText replaysWatched = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            const float vertical_spacing = 4;
            const float horizontal_spacing = 20;

            AutoSizeAxes = Axes.Both;
            CornerRadius = 6;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                // this should really be a grid, but trying to avoid one to avoid the performance hit.
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(horizontal_spacing, 0),
                    Padding = new MarginPadding { Vertical = 18, Horizontal = 12 },
                    Children = new[]
                    {
                        new FillFlowContainer
                        {
                            Name = @"Labels",
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, vertical_spacing),
                            Children = new Drawable[]
                            {
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsRankedScore },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsHitAccuracy },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsPlayCount },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsTotalScore },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsTotalHits },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsHitsPerPlay },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsMaximumCombo },
                                new ExtendedDetailsEntryLabel(colourProvider) { Text = UsersStrings.ShowStatsReplaysWatchedByOthers },
                            }
                        },
                        new FillFlowContainer
                        {
                            Name = @"Values",
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, vertical_spacing),
                            Children = new Drawable[]
                            {
                                rankedScore = new ExtendedDetailsEntryValue(colourProvider),
                                hitAccuracy = new ExtendedDetailsEntryValue(colourProvider),
                                playCount = new ExtendedDetailsEntryValue(colourProvider),
                                totalScore = new ExtendedDetailsEntryValue(colourProvider),
                                totalHits = new ExtendedDetailsEntryValue(colourProvider),
                                hitsPerPlay = new ExtendedDetailsEntryValue(colourProvider),
                                maximumCombo = new ExtendedDetailsEntryValue(colourProvider),
                                replaysWatched = new ExtendedDetailsEntryValue(colourProvider),
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(user => updateStatistics(user.NewValue?.User.Statistics), true);
        }

        private int getHitsPerPlay(UserStatistics statistics)
        {
            return statistics.PlayCount == 0 ? 0 : statistics.TotalHits / statistics.PlayCount;
        }

        private void updateStatistics(UserStatistics? statistics)
        {
            if (statistics == null)
            {
                Alpha = 0;
                return;
            }

            Alpha = 1;

            rankedScore.Text = statistics.RankedScore.ToLocalisableString(@"N0");
            hitAccuracy.Text = statistics.DisplayAccuracy;
            playCount.Text = statistics.PlayCount.ToLocalisableString(@"N0");
            totalScore.Text = statistics.TotalScore.ToLocalisableString(@"N0");
            totalHits.Text = statistics.TotalHits.ToLocalisableString(@"N0");
            hitsPerPlay.Text = getHitsPerPlay(statistics).ToLocalisableString(@"N0");
            maximumCombo.Text = statistics.MaxCombo.ToLocalisableString(@"N0");
            replaysWatched.Text = statistics.ReplaysWatched.ToLocalisableString(@"N0");
        }

        public partial class ExtendedDetailsEntryLabel : OsuSpriteText
        {
            public ExtendedDetailsEntryLabel(OverlayColourProvider colourProvider)
            {
                Font = OsuFont.Default.With(size: 12);
                Colour = colourProvider.Content1;
            }
        }

        public partial class ExtendedDetailsEntryValue : OsuSpriteText
        {
            public ExtendedDetailsEntryValue(OverlayColourProvider colourProvider)
            {
                Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold);
                Colour = colourProvider.Content2;
            }
        }
    }
}
