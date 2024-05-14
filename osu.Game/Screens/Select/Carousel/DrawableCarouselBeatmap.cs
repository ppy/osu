// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
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
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
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

        private const float height = MAX_HEIGHT * 0.625f;
        private const float colour_box_width = 30;
        private const float corner_radius = 10;

        private readonly BeatmapInfo beatmapInfo;

        private MenuItem[]? mainMenuItems;

        private Action<BeatmapInfo>? selectRequested;
        private Action<BeatmapInfo>? hideRequested;

        private StarCounter starCounter = null!;
        private ConstrainedIconContainer iconContainer = null!;

        private Box colourBox = null!;

        private StarRatingDisplay starRatingDisplay = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private OsuSpriteText keyCountText = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private IBindable<StarDifficulty?> starDifficultyBindable = null!;
        private CancellationTokenSource? starDifficultyCancellationSource;
        private Container rightContainer = null!;
        private Box starRatingGradient = null!;

        public DrawableCarouselBeatmap(CarouselBeatmap panel)
        {
            beatmapInfo = panel.BeatmapInfo;
            Item = panel;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager? manager, SongSelect? songSelect)
        {
            Header.Height = height;

            if (songSelect != null)
            {
                mainMenuItems = songSelect.CreateForwardNavigationMenuItemsForBeatmap(() => beatmapInfo);
                selectRequested = b => songSelect.FinaliseSelection(b);
            }

            if (manager != null)
                hideRequested = manager.Hide;

            Header.Children = new Drawable[]
            {
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        colourBox = new Box
                        {
                            Width = colour_box_width + corner_radius,
                            RelativeSizeAxes = Axes.Y,
                            Colour = colours.ForStarDifficulty(0),
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                        rightContainer = new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Masking = true,
                            CornerRadius = corner_radius,
                            RelativeSizeAxes = Axes.X,
                            // We don't want to match the header's size when its selected, hence no relative sizing.
                            Height = height,
                            X = colour_box_width,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4),
                                },
                                starRatingGradient = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                },
                            },
                        },
                    }
                },
                iconContainer = new ConstrainedIconContainer
                {
                    X = colour_box_width / 2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.CentreLeft,
                    Icon = beatmapInfo.Ruleset.CreateInstance().CreateIcon(),
                    Size = new Vector2(20),
                    Colour = colourProvider.Background5,
                    Shear = -CarouselHeader.SHEAR,
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Top = 8, Left = colour_box_width + corner_radius },
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(3, 0),
                            AutoSizeAxes = Axes.Both,
                            Shear = -CarouselHeader.SHEAR,
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
                            Shear = -CarouselHeader.SHEAR,
                            Children = new[]
                            {
                                keyCountText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Alpha = 0,
                                },
                                new OsuSpriteText
                                {
                                    Text = beatmapInfo.DifficultyName,
                                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft
                                },
                                new OsuSpriteText
                                {
                                    Colour = colourProvider.Content2,
                                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                    Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmapInfo.Metadata.Author.Username),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateKeyCount());
            mods.BindValueChanged(_ => updateKeyCount());
        }

        protected override void Selected()
        {
            base.Selected();

            MovementContainer.MoveToX(-50, 500, Easing.OutExpo);

            rightContainer.Height = height - 4;

            colourBox.RelativeSizeAxes = Axes.Both;
            colourBox.Width = 1;
        }

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, 500, Easing.OutExpo);

            rightContainer.Height = height;

            Header.EffectContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(1),
                Radius = 10,
                Colour = Colour4.Black.Opacity(100),
            };

            colourBox.RelativeSizeAxes = Axes.Y;
            colourBox.Width = colour_box_width + corner_radius;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Item?.State.Value == CarouselItemState.Selected)
                selectRequested?.Invoke(beatmapInfo);

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
                    starRatingDisplay.Current.Value = d.NewValue ?? default;
                }, true);

                starRatingDisplay.Current.BindValueChanged(s =>
                {
                    starCounter.Current = (float)s.NewValue.Stars;

                    // Every other element in song select that uses this cut off uses yellow for the upper range but the designs use white here for whatever reason.
                    iconContainer.Colour = s.NewValue.Stars > 6.5f ? Colour4.White : colourProvider.Background5;

                    var starRatingColour = colours.ForStarDifficulty(s.NewValue.Stars);

                    starCounter.Colour = colourBox.Colour = starRatingColour;
                    starRatingGradient.Colour = ColourInfo.GradientHorizontal(starRatingColour.Opacity(0.25f), starRatingColour.Opacity(0));
                    starRatingGradient.Show();

                    if (Item!.State.Value == CarouselItemState.NotSelected) return;

                    // We want to update the EdgeEffect here instead of in selected() to make sure the colours are correct
                    Header.EffectContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = starCounter.Colour.MultiplyAlpha(0.5f),
                        Radius = 10,
                    };
                });

                updateKeyCount();
            }

            base.ApplyState();
        }

        private void updateKeyCount()
        {
            if (Item?.State.Value == CarouselItemState.Collapsed)
                return;

            if (ruleset.Value.OnlineID == 3)
            {
                // Account for mania differences locally for now.
                // Eventually this should be handled in a more modular way, allowing rulesets to add more information to the panel.
                ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();

                keyCountText.Alpha = 1;
                keyCountText.Text = $"[{legacyRuleset.GetKeyCount(beatmapInfo, mods.Value)}K]";
            }
            else
                keyCountText.Alpha = 0;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (mainMenuItems != null)
                    items.AddRange(mainMenuItems);

                if (beatmapInfo.OnlineID > 0 && beatmapOverlay != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay.FetchAndShowBeatmap(beatmapInfo.OnlineID)));

                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmapInfo)).Cast<OsuMenuItem>().ToList();

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
