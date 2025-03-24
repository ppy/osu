// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays.Dialog;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Utils;
using osuTK.Graphics;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapSearchGeneralFilterRow : BeatmapSearchMultipleSelectionFilterRow<SearchGeneral>
    {
        public readonly IBindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        public BeatmapSearchGeneralFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersGeneral)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new GeneralFilter
        {
            Ruleset = { BindTarget = Ruleset }
        };

        private partial class GeneralFilter : MultipleSelectionFilter
        {
            public readonly IBindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

            protected override MultipleSelectionFilterTabItem CreateTabItem(SearchGeneral value)
            {
                switch (value)
                {
                    case SearchGeneral.Recommended:
                        return new RecommendedDifficultyTabItem
                        {
                            Ruleset = { BindTarget = Ruleset }
                        };

                    case SearchGeneral.FeaturedArtists:
                        return new FeaturedArtistsTabItem();

                    default:
                        return new MultipleSelectionFilterTabItem(value);
                }
            }
        }

        private partial class RecommendedDifficultyTabItem : MultipleSelectionFilterTabItem
        {
            public readonly IBindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

            [Resolved]
            private DifficultyRecommender? recommender { get; set; }

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            [Resolved]
            private RulesetStore rulesets { get; set; } = null!;

            public RecommendedDifficultyTabItem()
                : base(SearchGeneral.Recommended)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (recommender != null)
                    recommender.StarRatingUpdated += updateText;

                Ruleset.BindValueChanged(_ => updateText(), true);
            }

            private void updateText()
            {
                // fallback to profile default game mode if beatmap listing mode filter is set to Any
                // TODO: find a way to update `PlayMode` when the profile default game mode has changed
                RulesetInfo? ruleset = Ruleset.Value.IsLegacyRuleset() ? Ruleset.Value : rulesets.GetRuleset(api.LocalUser.Value.PlayMode);

                if (ruleset == null) return;

                double? starRating = recommender?.GetRecommendedStarRatingFor(ruleset);

                if (starRating != null)
                    Text.Text = LocalisableString.Interpolate($"{Value.GetLocalisableDescription()} ({starRating.Value.FormatStarRating()})");
                else
                    Text.Text = Value.GetLocalisableDescription();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (recommender != null)
                    recommender.StarRatingUpdated -= updateText;
            }
        }

        private partial class FeaturedArtistsTabItem : MultipleSelectionFilterTabItem, IHasTooltip
        {
            private Bindable<bool> disclaimerShown = null!;

            public FeaturedArtistsTabItem()
                : base(SearchGeneral.FeaturedArtists)
            {
            }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private OsuConfigManager config { get; set; } = null!;

            [Resolved]
            private SessionStatics sessionStatics { get; set; } = null!;

            [Resolved]
            private IDialogOverlay? dialogOverlay { get; set; }

            [Resolved]
            private OsuGame? game { get; set; }

            public LocalisableString TooltipText => BeatmapOverlayStrings.FeaturedArtistsTooltip;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                config.BindWith(OsuSetting.BeatmapListingFeaturedArtistFilter, Active);
                disclaimerShown = sessionStatics.GetBindable<bool>(Static.FeaturedArtistDisclaimerShownOnce);

                // no need to show the disclaimer if the user already had it toggled off in config.
                if (!Active.Value)
                    disclaimerShown.Value = true;

                if (game?.HideUnlicensedContent == true)
                {
                    Enabled.Value = false;
                    Active.Disabled = true;
                }
            }

            protected override Color4 ColourNormal => colours.Orange1;
            protected override Color4 ColourActive => colours.Orange2;

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return true;

                if (!disclaimerShown.Value && dialogOverlay != null)
                {
                    dialogOverlay.Push(new FeaturedArtistConfirmDialog(() =>
                    {
                        disclaimerShown.Value = true;
                        base.OnClick(e);
                    }));

                    return true;
                }

                return base.OnClick(e);
            }
        }
    }

    internal partial class FeaturedArtistConfirmDialog : PopupDialog
    {
        public FeaturedArtistConfirmDialog(Action confirm)
        {
            HeaderText = BeatmapOverlayStrings.UserContentDisclaimerHeader;
            BodyText = BeatmapOverlayStrings.UserContentDisclaimerDescription;

            Icon = FontAwesome.Solid.ExclamationTriangle;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = BeatmapOverlayStrings.UserContentConfirmButtonText,
                    Action = confirm
                },
                new PopupDialogCancelButton
                {
                    Text = CommonStrings.ButtonsCancel,
                },
            };
        }
    }
}
