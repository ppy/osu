// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class BeatmapListingOverlay : OnlineOverlay<BeatmapListingHeader>
    {
        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private IBindable<APIUser> apiUser;

        private Container panelTarget;
        private FillFlowContainer<BeatmapCard> foundContent;

        private BeatmapListingFilterControl filterControl => Header.FilterControl;

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
                            }
                        },
                    },
                }
            };

            filterControl.TypingStarted = onTypingStarted;
            filterControl.SearchStarted = onSearchStarted;
            filterControl.SearchFinished = onSearchFinished;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            filterControl.CardSize.BindValueChanged(_ => onCardSizeChanged());

            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => Schedule(() =>
            {
                if (api.IsLoggedIn)
                    replaceResultsAreaContent(Drawable.Empty());
            }));
        }

        public void ShowWithSearch(string query)
        {
            filterControl.Search(query);
            Show();
            ScrollFlow.ScrollToStart();
        }

        public void ShowWithGenreFilter(SearchGenre genre)
        {
            ShowWithSearch(string.Empty);
            filterControl.FilterGenre(genre);
        }

        public void ShowWithLanguageFilter(SearchLanguage language)
        {
            ShowWithSearch(string.Empty);
            filterControl.FilterLanguage(language);
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
                var supporterOnly = new SupporterRequiredDrawable(searchResult.SupporterOnlyFiltersUsed);
                replaceResultsAreaContent(supporterOnly);
                return;
            }

            var newCards = createCardsFor(searchResult.Results);

            if (filterControl.CurrentPage == 0)
            {
                //No matches case
                if (!newCards.Any())
                {
                    replaceResultsAreaContent(new NotFoundDrawable());
                    return;
                }

                var content = createCardContainerFor(newCards);

                panelLoadTask = LoadComponentAsync(foundContent = content, replaceResultsAreaContent, (cancellationToken = new CancellationTokenSource()).Token);
            }
            else
            {
                // new results may contain beatmaps from a previous page,
                // this is dodgy but matches web behaviour for now.
                // see: https://github.com/ppy/osu-web/issues/9270
                newCards = newCards.ExceptBy(foundContent.Select(c => c.BeatmapSet.OnlineID), c => c.BeatmapSet.OnlineID);

                panelLoadTask = LoadComponentsAsync(newCards, loaded =>
                {
                    lastFetchDisplayedTime = Time.Current;
                    foundContent.AddRange(loaded);
                    loaded.ForEach(p => p.FadeIn(200, Easing.OutQuint));
                }, (cancellationToken = new CancellationTokenSource()).Token);
            }
        }

        private IEnumerable<BeatmapCard> createCardsFor(IEnumerable<APIBeatmapSet> beatmapSets) => beatmapSets.Select(set => BeatmapCard.Create(set, filterControl.CardSize.Value).With(c =>
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

        private void replaceResultsAreaContent(Drawable content)
        {
            Loading.Hide();
            lastFetchDisplayedTime = Time.Current;

            panelTarget.Child = content;

            content.FadeInFromZero();
        }

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

        public partial class NotFoundDrawable : CompositeDrawable
        {
            public NotFoundDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
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
        public partial class SupporterRequiredDrawable : CompositeDrawable
        {
            private LinkFlowContainer supporterRequiredText;

            private readonly List<LocalisableString> filtersUsed;

            public SupporterRequiredDrawable(List<LocalisableString> filtersUsed)
            {
                RelativeSizeAxes = Axes.X;
                Height = 225;
                Alpha = 0;

                this.filtersUsed = filtersUsed;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
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

                supporterRequiredText.AddText(
                    BeatmapsStrings.ListingSearchSupporterFilterQuoteDefault(string.Join(" and ", filtersUsed), "").ToString(),
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
