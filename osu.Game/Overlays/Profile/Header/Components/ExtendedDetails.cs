// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        private SpriteText maximumCombo = null!;
        private SpriteText replaysWatched = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var font = OsuFont.Default.With(size: 12);
            const float vertical_spacing = 4;

            AutoSizeAxes = Axes.Both;

            // this should really be a grid, but trying to avoid one to avoid the performance hit.
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(20, 0),
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
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsRankedScore },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsHitAccuracy },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsPlayCount },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsTotalScore },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsTotalHits },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsMaximumCombo },
                            new OsuSpriteText { Font = font, Text = UsersStrings.ShowStatsReplaysWatchedByOthers },
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
                            rankedScore = new OsuSpriteText { Font = font },
                            hitAccuracy = new OsuSpriteText { Font = font },
                            playCount = new OsuSpriteText { Font = font },
                            totalScore = new OsuSpriteText { Font = font },
                            totalHits = new OsuSpriteText { Font = font },
                            maximumCombo = new OsuSpriteText { Font = font },
                            replaysWatched = new OsuSpriteText { Font = font },
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(user => updateStatistics(user.NewValue?.User.Statistics), true);
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
            maximumCombo.Text = statistics.MaxCombo.ToLocalisableString(@"N0");
            replaysWatched.Text = statistics.ReplaysWatched.ToLocalisableString(@"N0");
        }
    }
}
