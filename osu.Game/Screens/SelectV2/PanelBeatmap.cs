// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using CommonStrings = osu.Game.Localisation.CommonStrings;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelBeatmap : Panel, IHasContextMenu
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private StarCounter starCounter = null!;
        private ConstrainedIconContainer difficultyIcon = null!;
        private OsuSpriteText keyCountText = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private PanelLocalRankDisplay localRank = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText authorText = null!;

        private IBindable<StarDifficulty?>? starDifficultyBindable;
        private CancellationTokenSource? starDifficultyCancellationSource;

        private readonly List<MenuItem> mainContextItems = new List<MenuItem>();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = TopLevelContent.DrawRectangle;

            // Cover the gaps introduced by the spacing between BeatmapPanels so that clicks will not fall through the carousel.
            //
            // Caveat is that for simplicity, we are covering the full spacing, so panels with frontmost depth will have a slightly
            // larger hit target.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING });

            return inputRectangle.Contains(TopLevelContent.ToLocalSpace(screenSpacePos));
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = HEIGHT;

            Icon = difficultyIcon = new ConstrainedIconContainer
            {
                Size = new Vector2(16f),
                Margin = new MarginPadding { Horizontal = 5f },
                Colour = colourProvider.Background5,
            };

            Content.Children = new[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding { Left = 10f },
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(3, 0),
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Scale = new Vector2(0.875f),
                                },
                                localRank = new PanelLocalRankDisplay
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Scale = new Vector2(0.65f)
                                },
                                starCounter = new StarCounter
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Scale = new Vector2(0.4f)
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                keyCountText = new OsuSpriteText
                                {
                                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Alpha = 0,
                                },
                                difficultyText = new OsuSpriteText
                                {
                                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Right = 5f },
                                },
                                authorText = new OsuSpriteText
                                {
                                    Colour = colourProvider.Content2,
                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft
                                }
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            });

            mods.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            }, true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            var beatmap = (BeatmapInfo)Item.Model;

            difficultyIcon.Icon = beatmap.Ruleset.CreateInstance().CreateIcon();

            localRank.Beatmap = beatmap;
            difficultyText.Text = beatmap.DifficultyName;
            authorText.Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmap.Metadata.Author.Username);

            mainContextItems.Clear();

            if (songSelect != null)
                mainContextItems.AddRange(songSelect.CreateForwardNavigationMenuItemsForBeatmap(beatmap));

            computeStarRating();
            updateKeyCount();
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            localRank.Beatmap = null;
            starDifficultyBindable = null;
        }

        private void computeStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, starDifficultyCancellationSource.Token);
            starDifficultyBindable.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateKeyCount()
        {
            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            if (ruleset.Value.OnlineID == 3)
            {
                // Account for mania differences locally for now.
                // Eventually this should be handled in a more modular way, allowing rulesets to add more information to the panel.
                ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();
                int keyCount = legacyRuleset.GetKeyCount(beatmap, mods.Value);

                keyCountText.Alpha = 1;
                keyCountText.Text = $"[{keyCount}K] ";
            }
            else
                keyCountText.Alpha = 0;
        }

        private void updateDisplay()
        {
            const float duration = 500;

            var starDifficulty = starDifficultyBindable?.Value ?? default;

            starRatingDisplay.Current.Value = starDifficulty;
            starCounter.Current = (float)starDifficulty.Stars;

            difficultyIcon.FadeColour(starDifficulty.Stars > OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.Orange1 : colourProvider.Background5, duration, Easing.OutQuint);

            var starRatingColour = colours.ForStarDifficulty(starDifficulty.Stars);
            starCounter.FadeColour(starRatingColour, duration, Easing.OutQuint);
            AccentColour = starRatingColour;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (Item == null)
                    return Array.Empty<MenuItem>();

                var beatmap = (BeatmapInfo)Item.Model;

                List<MenuItem> items = new List<MenuItem>();

                if (mainContextItems.Count > 0)
                    items.AddRange(mainContextItems);

                if (beatmap.OnlineID > 0)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay?.FetchAndShowBeatmap(beatmap.OnlineID)));

                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmap)).Cast<OsuMenuItem>().ToList();

                collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, () => manageCollectionsDialog?.Show()));

                items.Add(new OsuMenuItem("Collections") { Items = collectionItems });

                if (beatmap.GetOnlineURL(api, ruleset.Value) is string url)
                    items.Add(new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => game?.CopyToClipboard(url)));

                items.Add(new OsuMenuItem("Mark as played", MenuItemType.Standard, () => beatmaps.MarkPlayed(beatmap)));
                items.Add(new OsuMenuItem(WebCommonStrings.ButtonsHide.ToSentence(), MenuItemType.Destructive, () => beatmaps.Hide(beatmap)));

                return items.ToArray();
            }
        }
    }
}
