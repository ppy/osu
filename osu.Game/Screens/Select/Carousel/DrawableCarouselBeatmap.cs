// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class DrawableCarouselBeatmap : DrawableCarouselItem, IHasContextMenu
    {
        public const float CAROUSEL_BEATMAP_SPACING = 5;

        /// <summary>
        /// The height of a carousel beatmap, including vertical spacing.
        /// </summary>
        public const float HEIGHT = height + CAROUSEL_BEATMAP_SPACING;

        private const float height = MAX_HEIGHT * 0.6f;

        private readonly BeatmapInfo beatmapInfo;

        private Action<BeatmapInfo>? startRequested;
        private Action<BeatmapInfo>? editRequested;
        private Action<BeatmapInfo>? hideRequested;

        private StarCounter starCounter = null!;
        private ConstrainedIconContainer iconContainer = null!;

        private Box colourBox = null!;

        // The purpose of this underline is to avoid the bleed caused by making the colourBox fill the container, without incurring the performance hit of using a buffered container
        private Box colourUnderline = null!;

        private StarRatingDisplay starRatingDisplay = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IBindable<StarDifficulty?> starDifficultyBindable = null!;
        private CancellationTokenSource? starDifficultyCancellationSource;

        public DrawableCarouselBeatmap(CarouselBeatmap panel)
        {
            beatmapInfo = panel.BeatmapInfo;
            Item = panel;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager? manager, SongSelect? songSelect)
        {
            Header.Height = height;
            Header.HasBorder = false;

            if (songSelect != null)
            {
                startRequested = b => songSelect.FinaliseSelection(b);
                if (songSelect.AllowEditing)
                    editRequested = songSelect.Edit;
            }

            if (manager != null)
                hideRequested = manager.Hide;

            Header.Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        colourBox = new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 40,
                        },
                        colourUnderline = new Box
                        {
                            Alpha = 0,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 3
                        },
                    }
                },
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.X,
                    // We don't want to match the header's size when its selected, hence no relative sizing.
                    Height = height,
                    X = 30,
                    Colour = colourProvider.Background3,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },

                iconContainer = new ConstrainedIconContainer
                {
                    X = 15,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.CentreLeft,
                    Icon = beatmapInfo.Ruleset.CreateInstance().CreateIcon(),
                    Size = new Vector2(20)
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Top = 8, Left = 40 },
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
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
                                        starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small),

                                        //Scaling is applied to size match components of row
                                        new TopLocalRank(beatmapInfo) { Scale = new Vector2(8f / 11) },
                                        starCounter = new StarCounter
                                        {
                                            Margin = new MarginPadding { Top = 8 }, // Better aligns the stars with the star rating display
                                            Scale = new Vector2(8 / 20f)
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(11, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = beatmapInfo.DifficultyName,
                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new OsuSpriteText
                                        {
                                            Colour = Colour4.FromHex("#DBF0E9"),
                                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                            Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmapInfo.Metadata.Author.Username),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void Selected()
        {
            base.Selected();

            MovementContainer.MoveToX(-50, 500, Easing.OutExpo);

            Header.Height = height + 2;
            colourUnderline.FadeInFromZero();
        }

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, 500, Easing.OutExpo);

            Header.Height = height;
            colourUnderline.FadeOutFromOne();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Item?.State.Value == CarouselItemState.Selected)
                startRequested?.Invoke(beatmapInfo);

            return base.OnClick(e);
        }

        protected override void ApplyState()
        {
            if (Item?.State.Value != CarouselItemState.Collapsed && Alpha == 0)
                starCounter.ReplayAnimation();

            starDifficultyCancellationSource?.Cancel();

            // Only compute difficulty when the item is visible.
            if (Item?.State.Value != CarouselItemState.Collapsed)
            {
                // We've potentially cancelled the computation above so a new bindable is required.
                starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmapInfo, (starDifficultyCancellationSource = new CancellationTokenSource()).Token);
                starDifficultyBindable.BindValueChanged(d =>
                {
                    starCounter.Current = (float)(d.NewValue?.Stars ?? 0);
                    starRatingDisplay.Current.Value = d.NewValue ?? default;

                    if (d.NewValue == null) return;

                    // Every other element in song select that uses this cut off uses yellow for the upper range but the designs use white here for whatever reason.
                    iconContainer.Colour = d.NewValue.Value.Stars > 6.5f ? Colour4.White : colours.B5;

                    starCounter.Colour = colourBox.Colour = colourUnderline.Colour =
                        colours.ForStarDifficulty(d.NewValue.Value.Stars);
                }, true);
            }

            base.ApplyState();
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (startRequested != null)
                    items.Add(new OsuMenuItem("Play", MenuItemType.Highlighted, () => startRequested(beatmapInfo)));

                if (editRequested != null)
                    items.Add(new OsuMenuItem(CommonStrings.ButtonsEdit, MenuItemType.Standard, () => editRequested(beatmapInfo)));

                if (beatmapInfo.OnlineID > 0 && beatmapOverlay != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay.FetchAndShowBeatmap(beatmapInfo.OnlineID)));

                var collectionItems = realm.Realm.All<BeatmapCollection>().AsEnumerable().Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmapInfo)).Cast<OsuMenuItem>().ToList();
                if (manageCollectionsDialog != null)
                    collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                items.Add(new OsuMenuItem("Collections") { Items = collectionItems });

                if (hideRequested != null)
                    items.Add(new OsuMenuItem(CommonStrings.ButtonsHide.ToSentence(), MenuItemType.Destructive, () => hideRequested(beatmapInfo)));

                return items.ToArray();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            starDifficultyCancellationSource?.Cancel();
        }
    }
}
