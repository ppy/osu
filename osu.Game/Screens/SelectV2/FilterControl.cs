// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    public partial class FilterControl : OverlayContainer
    {
        // taken from draw visualiser. used for carousel alignment purposes.
        public const float HEIGHT_FROM_SCREEN_TOP = 141 - corner_radius;

        private const float corner_radius = 10;

        private SongSelectSearchTextBox searchTextBox = null!;
        private ShearedToggleButton showConvertedBeatmapsButton = null!;
        private DifficultyRangeSlider difficultyRangeSlider = null!;
        private ShearedDropdown<SortMode> sortDropdown = null!;
        private ShearedDropdown<GroupMode> groupDropdown = null!;
        private CollectionDropdown collectionDropdown = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo localUserPlayInfo { get; set; } = null!;

        private IBindable<APIUser> localUser = null!;

        private ImmutableHashSet<string>? cachedFavoriteMD5Hashes;
        private bool isFetchingFavorites;
        private int? lastFetchedUserId;
        private DateTimeOffset? cacheTime;

        private double lastPeriodicCheck;

        /// <summary>
        /// Interval for periodic favorites cache refresh in milliseconds (5 minutes).
        /// </summary>
        private const double PERIODIC_REFRESH_INTERVAL = 300000;

        public LocalisableString StatusText
        {
            get => searchTextBox.StatusText;
            set => searchTextBox.StatusText = value;
        }

        public event Action<FilterCriteria>? CriteriaChanged;

        private FilterCriteria currentCriteria = null!;

        private IDisposable? collectionsSubscription;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Shear = OsuGame.SHEAR;
            Margin = new MarginPadding { Top = -corner_radius, Right = -40 };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Child = new WedgeBackground
                    {
                        Anchor = Anchor.TopRight,
                        Scale = new Vector2(-1, 1),
                    }
                },
                new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 5f),
                    Padding = new MarginPadding { Top = corner_radius + 5, Bottom = 2, Right = 40f, Left = 2f },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Child = searchTextBox = new SongSelectSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                HoldFocus = true,
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute), // can probably be removed?
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    difficultyRangeSlider = new DifficultyRangeSlider
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        MinRange = 0.1f,
                                    },
                                    Empty(),
                                    showConvertedBeatmapsButton = new ShearedToggleButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = UserInterfaceStrings.ShowConverts,
                                        Height = 30f,
                                    },
                                },
                            }
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(maxSize: 180),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(maxSize: 180),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    sortDropdown = new ShearedDropdown<SortMode>(SongSelectStrings.Sort)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<SortMode>(),
                                    },
                                    Empty(),
                                    groupDropdown = new ShearedDropdown<GroupMode>(SongSelectStrings.Group)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<GroupMode>(),
                                    },
                                    Empty(),
                                    collectionDropdown = new CollectionDropdown
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                }
                            }
                        },
                    },
                }
            };

            localUser = api.LocalUser.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            difficultyRangeSlider.LowerBound = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            difficultyRangeSlider.UpperBound = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);
            config.BindWith(OsuSetting.ShowConvertedBeatmaps, showConvertedBeatmapsButton.Active);
            config.BindWith(OsuSetting.SongSelectSortingMode, sortDropdown.Current);
            config.BindWith(OsuSetting.SongSelectGroupMode, groupDropdown.Current);

            ruleset.BindValueChanged(_ => updateCriteria());
            mods.BindValueChanged(m =>
            {
                // The following is a note carried from old song select and may not be a valid reason anymore:
                // // Mods are updated once by the mod select overlay when song select is entered,
                // // regardless of if there are any mods or any changes have taken place.
                // // Updating the criteria here so early triggers a re-ordering of panels on song select, via... some mechanism.
                // // Todo: Investigate/fix and potentially remove this.
                // TODO: this might be simply removable with the new song select & carousel code.
                if (m.NewValue.SequenceEqual(m.OldValue))
                    return;

                var rulesetCriteria = currentCriteria.RulesetCriteria;
                if (rulesetCriteria?.FilterMayChangeFromMods(m) == true)
                    updateCriteria();
            });

            searchTextBox.Current.BindValueChanged(_ => updateCriteria());
            difficultyRangeSlider.LowerBound.BindValueChanged(_ => updateCriteria());
            difficultyRangeSlider.UpperBound.BindValueChanged(_ => updateCriteria());
            showConvertedBeatmapsButton.Active.BindValueChanged(_ => updateCriteria());
            sortDropdown.Current.BindValueChanged(_ => updateCriteria());
            groupDropdown.Current.BindValueChanged(_ => updateCriteria());
            collectionDropdown.Current.BindValueChanged(v =>
            {
                // The hope would be that this never arrives here, but due to bindings receiving changes before
                // local ValueChanged events, that's not the case (see https://github.com/ppy/osu-framework/pull/1545).
                if (v.NewValue is ManageCollectionsFilterMenuItem || v.OldValue is ManageCollectionsFilterMenuItem)
                    return;

                // If switching to favorites, only fetch if no cache exists
                if (v.NewValue is FavoriteBeatmapsCollectionFilterMenuItem)
                {
                    // Only fetch if we don't have cached data
                    if (cachedFavoriteMD5Hashes == null)
                    {
                        fetchFavoritesAsync();
                    }
                }

                updateCriteria();
            });
            collectionsSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>(), (collections, changeSet) =>
            {
                if (changeSet != null && groupDropdown.Current.Value == GroupMode.Collections)
                    updateCriteria();
            });

            api.LocalUser.BindValueChanged(user =>
            {
                // Clear cache when user changes
                if (user.NewValue?.Id != lastFetchedUserId)
                {
                    cachedFavoriteMD5Hashes = null;
                    lastFetchedUserId = user.NewValue?.Id;
                    cacheTime = null;
                }
                updateCriteria();
            });


            updateCriteria();

            // Pre-load favorites cache on startup
            if (api.LocalUser.Value?.Id > 1)
            {
                fetchFavoritesAsync();
            }

            // Subscribe to favorite change events
            PostBeatmapFavouriteRequest.FavouriteChanged += onFavouriteChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            collectionsSubscription?.Dispose();
            PostBeatmapFavouriteRequest.FavouriteChanged -= onFavouriteChanged;
        }

        /// <summary>
        /// Creates a <see cref="FilterCriteria"/> based on the current state of the controls.
        /// </summary>
        public FilterCriteria CreateCriteria()
        {
            string query = searchTextBox.Current.Value;
            bool isValidUser = localUser.Value.Id > 1;

            var criteria = new FilterCriteria
            {
                Sort = sortDropdown.Current.Value,
                Group = groupDropdown.Current.Value,
                AllowConvertedBeatmaps = showConvertedBeatmapsButton.Active.Value,
                Ruleset = ruleset.Value,
                Mods = mods.Value,
                CollectionBeatmapMD5Hashes = GetCollectionHashes(collectionDropdown.Current.Value),
                LocalUserId = isValidUser ? localUser.Value.Id : null,
                LocalUserUsername = isValidUser ? localUser.Value.Username : null,
            };

            if (!difficultyRangeSlider.LowerBound.IsDefault)
                criteria.UserStarDifficulty.Min = difficultyRangeSlider.LowerBound.Value;

            if (!difficultyRangeSlider.UpperBound.IsDefault)
                criteria.UserStarDifficulty.Max = difficultyRangeSlider.UpperBound.Value;

            criteria.RulesetCriteria = ruleset.Value.CreateInstance().CreateRulesetFilterCriteria();

            FilterQueryParser.ApplyQueries(criteria, query);
            return criteria;
        }

        private ImmutableHashSet<string>? GetCollectionHashes(CollectionFilterMenuItem? item)
        {
            if (item is FavoriteBeatmapsCollectionFilterMenuItem)
            {
                return GetFavoriteBeatmapHashes();
            }

            return item?.Collection?.PerformRead(c => c.BeatmapMD5Hashes).ToImmutableHashSet();
        }

        private ImmutableHashSet<string>? GetFavoriteBeatmapHashes()
        {
            // Check if user is logged in and API is available
            if (api.LocalUser.Value?.Id <= 1 || api.State.Value != APIState.Online)
                return null;

            // Return cached result if available (no expiry check - we refresh on selection)
            return cachedFavoriteMD5Hashes;
        }

        private void fetchFavoritesAsync()
        {
            if (isFetchingFavorites || api.LocalUser.Value?.Id <= 1)
                return;

            isFetchingFavorites = true;

            var userId = api.LocalUser.Value.Id;
            var request = new GetUserBeatmapsRequest(userId, BeatmapSetType.Favourite, new PaginationParameters(0, 200));

            request.Success += response =>
            {
                Schedule(() =>
                {
                    try
                    {
                        var favoriteMD5Hashes = new HashSet<string>();

                        if (response?.Count > 0)
                        {
                            var favoriteOnlineIds = response.Select(set => set.OnlineID).Where(id => id > 0).ToHashSet();

                            if (favoriteOnlineIds.Count > 0)
                            {
                                // Get all local beatmap sets
                                var localBeatmapSets = beatmapManager.GetAllUsableBeatmapSets();

                                // Find local beatmaps that match the favorite online IDs
                                foreach (var localSet in localBeatmapSets)
                                {
                                    if (localSet.OnlineID > 0 && favoriteOnlineIds.Contains(localSet.OnlineID))
                                    {
                                        foreach (var beatmap in localSet.Beatmaps)
                                        {
                                            if (!string.IsNullOrEmpty(beatmap.MD5Hash))
                                                favoriteMD5Hashes.Add(beatmap.MD5Hash);
                                        }
                                    }
                                }
                            }
                        }

                        cachedFavoriteMD5Hashes = favoriteMD5Hashes.ToImmutableHashSet();
                        cacheTime = DateTimeOffset.Now;
                        isFetchingFavorites = false;

                        // Update criteria to apply the new filter if favorites is currently selected
                        if (collectionDropdown.Current.Value is FavoriteBeatmapsCollectionFilterMenuItem)
                        {
                            updateCriteria();
                        }
                    }
                    catch
                    {
                        // On error, cache empty set and allow retry later
                        cachedFavoriteMD5Hashes = ImmutableHashSet<string>.Empty;
                        cacheTime = DateTimeOffset.Now;
                        isFetchingFavorites = false;
                    }
                });
            };

            request.Failure += _ =>
            {
                Schedule(() =>
                {
                    // On failure, cache null (show all beatmaps) and allow retry later
                    cachedFavoriteMD5Hashes = null;
                    isFetchingFavorites = false;
                });
            };

            api.Queue(request);
        }

        private void onFavouriteChanged(int beatmapSetId, bool favourited)
        {
            // If we don't have cached favorites yet, just trigger a full fetch
            if (cachedFavoriteMD5Hashes == null)
            {
                if (collectionDropdown.Current.Value is FavoriteBeatmapsCollectionFilterMenuItem)
                {
                    fetchFavoritesAsync();
                }
                return;
            }

            // Update cache directly with the changed beatmap set
            updateCacheForBeatmapSet(beatmapSetId, favourited);

            // If favorites filter is currently active, update the criteria
            if (collectionDropdown.Current.Value is FavoriteBeatmapsCollectionFilterMenuItem)
            {
                updateCriteria();
            }
        }

        private void updateCacheForBeatmapSet(int beatmapSetId, bool favourited)
        {
            // Find the local beatmap set that matches this online ID
            var localBeatmapSets = beatmapManager.GetAllUsableBeatmapSets();
            var matchingSet = localBeatmapSets.FirstOrDefault(set => set.OnlineID == beatmapSetId);

            if (matchingSet == null)
                return;

            // Get all MD5 hashes for this beatmap set
            var setMD5Hashes = matchingSet.Beatmaps
                .Where(beatmap => !string.IsNullOrEmpty(beatmap.MD5Hash))
                .Select(beatmap => beatmap.MD5Hash)
                .ToHashSet();

            if (setMD5Hashes.Count == 0)
                return;

            // Update the cached set based on favorite status
            var currentHashes = cachedFavoriteMD5Hashes?.ToHashSet() ?? new HashSet<string>();

            if (favourited)
            {
                // Add hashes to favorites
                foreach (var hash in setMD5Hashes)
                    currentHashes.Add(hash);
            }
            else
            {
                // Remove hashes from favorites
                foreach (var hash in setMD5Hashes)
                    currentHashes.Remove(hash);
            }

            // Update the cache
            cachedFavoriteMD5Hashes = currentHashes.ToImmutableHashSet();
            cacheTime = DateTimeOffset.Now;
        }

        protected override void Update()
        {
            base.Update();

            // Check for periodic favorites refresh
            if (Time.Current - lastPeriodicCheck >= PERIODIC_REFRESH_INTERVAL)
            {
                lastPeriodicCheck = Time.Current;
                periodicFavoritesRefresh();
            }
        }

        private void periodicFavoritesRefresh()
        {
            // Only refresh if user is not playing
            if (localUserPlayInfo.PlayingState.Value != LocalUserPlayingState.NotPlaying)
                return;

            // Only refresh if user is logged in
            if (api.LocalUser.Value?.Id <= 1)
                return;

            // Only refresh if we have existing cache
            if (cachedFavoriteMD5Hashes == null || !cacheTime.HasValue)
                return;

            // Refresh the cache to catch external changes
            fetchFavoritesAsync();
        }

        private void updateCriteria()
        {
            currentCriteria = CreateCriteria();
            CriteriaChanged?.Invoke(currentCriteria);
        }

        /// <summary>
        /// Set the query to the search text box.
        /// </summary>
        /// <param name="query">The string to search.</param>
        public void Search(string query)
        {
            searchTextBox.Current.Value = query;
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        internal partial class SongSelectSearchTextBox : ShearedFilterTextBox
        {
            protected override InnerSearchTextBox CreateInnerTextBox() => new InnerTextBox();

            private partial class InnerTextBox : InnerFilterTextBox
            {
                public override bool HandleLeftRightArrows => false;

                public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
                {
                    // Conflicts with default group navigation keys (shift-left shift-right).
                    if (e.Action == PlatformAction.SelectBackwardChar || e.Action == PlatformAction.SelectForwardChar)
                        return false;

                    // the "cut" platform key binding (shift-delete) conflicts with the beatmap deletion action.
                    if (e.Action == PlatformAction.Cut && e.ShiftPressed && e.CurrentState.Keyboard.Keys.IsPressed(Key.Delete))
                        return false;

                    return base.OnPressed(e);
                }
            }
        }
    }
}
