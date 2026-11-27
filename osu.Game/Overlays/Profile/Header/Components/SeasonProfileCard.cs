// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class SeasonProfileCard : ProfileHeaderCardSmall, IHasCustomTooltip<SeasonTooltipData>
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public SeasonTooltipData? TooltipContent { get; private set; }

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon = OsuIcon.Star;

            Label.Text = UsersStrings.ShowSeasonStatsLabel;

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (User.Value == null)
            {
                Hide();
                return;
            }

            var stats = User.Value.User.CurrentSeasonStatistics;

            if (stats == null)
            {
                Hide();
                return;
            }

            var divisionName = getDivisionName(stats);

            Value.Text = divisionName;
            Value.Colour = OsuColour.ForRankingTier(stats.Division.Tier);

            Clear();
            LoadComponentAsync(new SeasonDivisionBadge(stats.Division), Add);

            TooltipContent = new SeasonTooltipData(colourProvider, stats, divisionName);

            Show();
        }

        private LocalisableString getDivisionName(APISeasonStatistics stats)
        {
            if (stats.Season.RulesetID == null || stats.Season.VariantID == null)
                return stats.Division.Name;

            var ruleset = rulesets.GetRuleset(stats.Season.RulesetID.Value)?.CreateInstance();

            Debug.Assert(ruleset != null);

            if (!ruleset.GameplayVariants.Any())
                return stats.Division.Name;

            var variantName = ruleset.GetVariantName(stats.Season.VariantID.Value);

            return LocalisableString.Interpolate($"{stats.Division.Name} ({variantName})");
        }

        public ITooltip<SeasonTooltipData> GetCustomTooltip() => new SeasonStatsTooltip();

        [LongRunningLoad]
        public partial class SeasonDivisionBadge : CompositeDrawable
        {
            public static readonly Vector2 BADGE_SIZE = new Vector2(86, 40);

            private readonly APISeasonDivision division;

            public SeasonDivisionBadge(APISeasonDivision division)
            {
                this.division = division;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Size = BADGE_SIZE;
                Masking = true;
                CornerRadius = 4;

                InternalChild = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                    Texture = textures.Get(division.ImageUrl),
                };
            }
        }
    }
}
