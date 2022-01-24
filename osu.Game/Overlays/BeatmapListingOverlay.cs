﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Localisation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class BeatmapListingOverlay : OnlineOverlay<BeatmapListingHeader>
    {
        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private Drawable currentContent;
        private Container panelTarget;
        private FillFlowContainer<BeatmapCard> foundContent;
        private NotFoundDrawable notFoundContent;
        private SupporterRequiredDrawable supporterRequiredContent;
        private BeatmapListingFilterControl filterControl;

        public BeatmapListingOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    filterControl = new BeatmapListingFilterControl
                    {
                        TypingStarted = onTypingStarted,
                        SearchStarted = onSearchStarted,
                        SearchFinished = onSearchFinished,
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourProvider.Background5,
                            },
                            panelTarget = new Container
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Masking = true,
                                Padding = new MarginPadding { Horizontal = 20 },
                                Children = new Drawable[]
                                {
                                    notFoundContent = new NotFoundDrawable(),
                                    supporterRequiredContent = new SupporterRequiredDrawable(),
                                }
                            }
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            filterControl.CardSize.BindValueChanged(_ => onCardSizeChanged());
        }

        public void ShowWithSearch(string query)
        {
            filterControl.Search(query);
            Show();
        }

        protected override BeatmapListingHeader CreateHeader() => new BeatmapListingHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        private void onTypingStarted()
        {
            // temporary until the textbox/header is updated to always stay on screen.
            ScrollFlow.ScrollToStart();
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            filterControl.TakeFocus();
        }

        private CancellationTokenSource cancellationToken;

        private Task panelLoadTask;

        private void onSearchStarted()
        {
            cancellationToken?.Cancel();

            previewTrackManager.StopAnyPlaying(this);

            if (panelTarget.Any())
                Loading.Show();
        }

        private void onSearchFinished(BeatmapListingFilterControl.SearchResult searchResult)
        {
            cancellationToken?.Cancel();

            if (searchResult.Type == BeatmapListingFilterControl.SearchResultType.SupporterOnlyFilters)
            {
                supporterRequiredContent.UpdateText(searchResult.SupporterOnlyFiltersUsed);
                addContentToResultsArea(supporterRequiredContent);
                return;
            }

            var newCards = createCardsFor(searchResult.Results);

            if (filterControl.CurrentPage == 0)
            {
                //No matches case
                if (!newCards.Any())
                {
                    addContentToResultsArea(notFoundContent);
                    return;
                }

                var content = createCardContainerFor(newCards);

                panelLoadTask = LoadComponentAsync(foundContent = content, addContentToResultsArea, (cancellationToken = new CancellationTokenSource()).Token);
            }
            else
            {
                panelLoadTask = LoadComponentsAsync(newCards, loaded =>
                {
                    lastFetchDisplayedTime = Time.Current;
                    foundContent.AddRange(loaded);
                    loaded.ForEach(p => p.FadeIn(200, Easing.OutQuint));
                }, (cancellationToken = new CancellationTokenSource()).Token);
            }
        }

        private BeatmapCard[] createCardsFor(IEnumerable<APIBeatmapSet> beatmapSets) => beatmapSets.Select(set => BeatmapCard.Create(set, filterControl.CardSize.Value).With(c =>
        {
            c.Anchor = Anchor.TopCentre;
            c.Origin = Anchor.TopCentre;
        })).ToArray();

        private static ReverseChildIDFillFlowContainer<BeatmapCard> createCardContainerFor(IEnumerable<BeatmapCard> newCards)
        {
            // spawn new children with the contained so we only clear old content at the last moment.
            // reverse ID flow is required for correct Z-ordering of the cards' expandable content (last card should be front-most).
            var content = new ReverseChildIDFillFlowContainer<BeatmapCard>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Alpha = 0,
                Margin = new MarginPadding
                {
                    Top = 15,
                    // the + 20 adjustment is roughly eyeballed in order to fit all of the expanded content height after it's scaled
                    // as well as provide visual balance to the top margin.
                    Bottom = ExpandedContentScrollContainer.HEIGHT + 20
                },
                ChildrenEnumerable = newCards
            };
            return content;
        }

        private void addContentToResultsArea(Drawable content)
        {
            Loading.Hide();
            lastFetchDisplayedTime = Time.Current;

            if (content == currentContent)
                return;

            var lastContent = currentContent;

            if (lastContent != null)
            {
                lastContent.FadeOut();
                if (!isPlaceholderContent(lastContent))
                    lastContent.Expire();
            }

            if (!content.IsAlive)
                panelTarget.Add(content);

            content.FadeInFromZero();
            currentContent = content;
        }

        /// <summary>
        /// Whether <paramref name="drawable"/> is a static placeholder reused multiple times by this overlay.
        /// </summary>
        private bool isPlaceholderContent(Drawable drawable)
            => drawable == notFoundContent || drawable == supporterRequiredContent;

        private void onCardSizeChanged()
        {
            if (foundContent?.IsAlive != true || !foundContent.Any())
                return;

            Loading.Show();

            var newCards = createCardsFor(foundContent.Reverse().Select(card => card.BeatmapSet));

            cancellationToken?.Cancel();

            panelLoadTask = LoadComponentsAsync(newCards, cards =>
            {
                foundContent.Clear();
                foundContent.AddRange(cards);
                Loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }

        public class NotFoundDrawable : CompositeDrawable
        {
            public NotFoundDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get(@"Online/not-found")
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = BeatmapsStrings.ListingSearchNotFoundQuote,
                        }
                    }
                });
            }
        }

        // TODO: localisation requires Text/LinkFlowContainer support for localising strings with links inside
        // (https://github.com/ppy/osu-framework/issues/4530)
        public class SupporterRequiredDrawable : CompositeDrawable
        {
            private LinkFlowContainer supporterRequiredText;

            public SupporterRequiredDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 225;
                Alpha = 0;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get(@"Online/supporter-required"),
                        },
                        supporterRequiredText = new LinkFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Bottom = 10 },
                        },
                    }
                });
            }

            public void UpdateText(List<LocalisableString> filters)
            {
                supporterRequiredText.Clear();

                supporterRequiredText.AddText(
                    BeatmapsStrings.ListingSearchSupporterFilterQuoteDefault(string.Join(" and ", filters), "").ToString(),
                    t =>
                    {
                        t.Font = OsuFont.GetFont(size: 16);
                        t.Colour = Colour4.White;
                    }
                );

                supporterRequiredText.AddLink(BeatmapsStrings.ListingSearchSupporterFilterQuoteLinkText.ToString(), @"/store/products/supporter-tag");
            }
        }

        private const double time_between_fetches = 500;

        private double lastFetchDisplayedTime;

        protected override void Update()
        {
            base.Update();

            const int pagination_scroll_distance = 500;

            bool shouldShowMore = panelLoadTask?.IsCompleted != false
                                  && Time.Current - lastFetchDisplayedTime > time_between_fetches
                                  && (ScrollFlow.ScrollableExtent > 0 && ScrollFlow.IsScrolledToEnd(pagination_scroll_distance));

            if (shouldShowMore)
                filterControl.FetchNextPage();
        }
    }
}
