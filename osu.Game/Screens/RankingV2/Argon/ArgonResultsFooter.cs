// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Screens.Footer;
using osu.Game.Screens.RankingV2.Legacy;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonResultsFooter : CompositeDrawable, ISerialisableDrawable
    {
        public bool CanBePlaced(IEnumerable<ISerialisableDrawable> components) => components.All(c => c is not ArgonResultsFooter && c is not LegacyOnlineRankingButton);
        public bool CanBeMoved => false;
        public bool CanBeRotated => false;
        public bool CanBeScaled(Axes _) => false;

        private LeasedBindable<ScreenFooterContent?>? leasedFooterContent;

        public ArgonResultsFooter()
        {
            RelativeSizeAxes = Axes.X;
            Height = ScreenFooterButton.HEIGHT;
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(IResultsScreenV2? results, SkinEditor? skinEditor)
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Y = 0;

            if (skinEditor == null)
            {
                leasedFooterContent = results?.FooterContent.BeginLease(true);

                leasedFooterContent?.Value = ScreenFooterContent.Argon(
                    leftButtons: createLeftButtons,
                    rightButtons: createRightButtons);
            }
            else
            {
                // for toolbox preview purposes
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7, 0),
                    ChildrenEnumerable = createLeftButtons()
                };
            }
        }

        private IEnumerable<ScreenFooterButton> createLeftButtons() =>
        [
            new ScreenFooterButton
            {
                Icon = OsuIcon.Ranking,
                Text = ResultsScreenStrings.RankingFooterButtonText,
                Action = () => { },
                AccentColour = colours.Green1, // to match web ranking pages
            },
            new ScreenFooterButton
            {
                Icon = OsuIcon.Information,
                Text = ResultsScreenStrings.MoreStatisticsFooterButtonText,
                Action = () => { },
                AccentColour = colours.Blue1, // to match web score pages
            },
            new ScreenFooterButton
            {
                Icon = OsuIcon.Folder,
                Text = ResultsScreenStrings.CatalogueFooterButtonText,
                Action = () => { },
                AccentColour = colours.Blue1, // to match web beatmap pages
            },
        ];

        private IEnumerable<ShearedButton> createRightButtons() =>
        [
            // button widths mostly arbitrary, but chosen to ensure no overflows at 4:3 aspect ratio
            new ShearedButtonWithIcon
            {
                Width = 150,
                Text = ResultsScreenStrings.WatchReplayFooterButtonText,
                Icon = OsuIcon.Reel,
                Action = () => { },
                DarkerColour = colours.Lime3,
                LighterColour = colours.Lime1,
            },
            new ShearedButtonWithIcon
            {
                Width = 150,
                Text = ResultsScreenStrings.PlayAgainFooterButtonText,
                Icon = OsuIcon.ArrowCycle,
                Action = () => { },
                DarkerColour = colours.Blue3,
                LighterColour = colours.Blue1,
            }
        ];

        protected override void Dispose(bool isDisposing)
        {
            leasedFooterContent?.Return();

            base.Dispose(isDisposing);
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; } = true;

        #endregion
    }
}
