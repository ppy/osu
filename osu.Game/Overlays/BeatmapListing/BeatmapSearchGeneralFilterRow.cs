// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapSearchGeneralFilterRow : BeatmapSearchMultipleSelectionFilterRow<SearchGeneral>
    {
        public BeatmapSearchGeneralFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersGeneral)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new GeneralFilter();

        private partial class GeneralFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem CreateTabItem(SearchGeneral value)
            {
                if (value == SearchGeneral.FeaturedArtists)
                    return new FeaturedArtistsTabItem();

                return new MultipleSelectionFilterTabItem(value);
            }
        }

        private partial class FeaturedArtistsTabItem : MultipleSelectionFilterTabItem
        {
            private Bindable<bool> disclaimerShown;

            public FeaturedArtistsTabItem()
                : base(SearchGeneral.FeaturedArtists)
            {
            }

            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private SessionStatics sessionStatics { get; set; }

            [Resolved(canBeNull: true)]
            private IDialogOverlay dialogOverlay { get; set; }

            protected override Color4 GetStateColour() => colours.Orange1;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                disclaimerShown = sessionStatics.GetBindable<bool>(Static.FeaturedArtistDisclaimerShownOnce);
            }

            protected override bool OnClick(ClickEvent e)
            {
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
