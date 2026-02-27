// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using Realms;

namespace osu.Game.Collections
{
    public partial class ManageCollectionsDialog : OsuFocusedOverlayContainer
    {
        private const double enter_duration = 500;
        private const double exit_duration = 200;

        protected override string PopInSampleName => @"UI/overlay-big-pop-in";
        protected override string PopOutSampleName => @"UI/overlay-big-pop-out";

        private IDisposable? duckOperation;

        private BasicSearchTextBox searchTextBox = null!;
        private DrawableCollectionList list = null!;
        private SaveToCollectionDropdown saveToCollectionDropdown = null!;
        private RoundedButton saveFilteredResultsButton = null!;

        private readonly BindableList<CollectionFilterMenuItem> saveToCollectionItems = new BindableList<CollectionFilterMenuItem>();

        private IDisposable? saveCollectionsSubscription;

        private Func<IEnumerable<BeatmapInfo>>? filteredBeatmapsProvider;

        public Func<IEnumerable<BeatmapInfo>>? FilteredBeatmapsProvider
        {
            get => filteredBeatmapsProvider;
            set
            {
                filteredBeatmapsProvider = value;
                updateSaveButtonState();
            }
        }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private MusicController? musicController { get; set; }

        public ManageCollectionsDialog()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.5f, 0.8f);

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GreySeaFoamDark,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = CollectionsStrings.ManageCollectionsTitle,
                                            Font = OsuFont.GetFont(size: 30),
                                            Padding = new MarginPadding { Vertical = 10 },
                                        },
                                        new IconButton
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Icon = FontAwesome.Solid.Times,
                                            Colour = colours.GreySeaFoamDarker,
                                            Scale = new Vector2(0.8f),
                                            X = -10,
                                            Action = () => State.Value = Visibility.Hidden
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.GreySeaFoamDarker,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(10),
                                            Children = new Drawable[]
                                            {
                                                list = new DrawableCollectionList
                                                {
                                                    Padding = new MarginPadding { Top = 90, Bottom = 50 },
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                searchTextBox = new BasicSearchTextBox
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Y = 45,
                                                    Height = 40,
                                                    ReleaseFocusOnCommit = false,
                                                    HoldFocus = true,
                                                    PlaceholderText = HomeStrings.SearchPlaceholder,
                                                },
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    Children = new Drawable[]
                                                    {
                                                        new NewCollectionEntryItem()
                                                    }
                                                },
                                                new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = 40,
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension(GridSizeMode.Absolute, 10),
                                                        new Dimension(),
                                                        new Dimension(GridSizeMode.Absolute, 10),
                                                        new Dimension(GridSizeMode.AutoSize),
                                                    },
                                                    Content = new[]
                                                    {
                                                        new[]
                                                        {
                                                            new OsuSpriteText
                                                            {
                                                                Anchor = Anchor.CentreLeft,
                                                                Origin = Anchor.CentreLeft,
                                                                Text = "Save filter result to Collection:",
                                                                Font = OsuFont.GetFont(size: 16),
                                                            },
                                                            Empty(),
                                                            saveToCollectionDropdown = new SaveToCollectionDropdown
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                            },
                                                            Empty(),
                                                            saveFilteredResultsButton = new RoundedButton
                                                            {
                                                                Width = 90,
                                                                Height = 40,
                                                                Text = "Save",
                                                                Action = saveFilteredResults,
                                                            },
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            searchTextBox.Current.BindValueChanged(_ =>
            {
                list.SearchTerm = searchTextBox.Current.Value;
            });

            saveToCollectionDropdown.ItemSource = saveToCollectionItems;
            saveToCollectionDropdown.Current.BindValueChanged(_ => updateSaveButtonState(), true);

            saveCollectionsSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), saveCollectionsChanged);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            duckOperation?.Dispose();
            saveCollectionsSubscription?.Dispose();
        }

        protected override void PopIn()
        {
            duckOperation = musicController?.Duck(new DuckParameters
            {
                DuckVolumeTo = 1,
                DuckDuration = 100,
                RestoreDuration = 100,
            });

            this.FadeIn(enter_duration, Easing.OutQuint);
            this.ScaleTo(0.9f).Then().ScaleTo(1f, enter_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            duckOperation?.Dispose();

            this.FadeOut(exit_duration, Easing.OutQuint);
            this.ScaleTo(0.9f, exit_duration);

            // Ensure that textboxes commit
            GetContainingFocusManager()?.TriggerFocusContention(this);
        }

        private partial class NewCollectionEntryItem : DrawableCollectionListItem
        {
            [Resolved]
            private RealmAccess realm { get; set; } = null!;

            public NewCollectionEntryItem()
                : base(new BeatmapCollection().ToLiveUnmanaged(), false)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                TextBox.OnCommit += (_, _) =>
                {
                    if (string.IsNullOrEmpty(TextBox.Text))
                        return;

                    realm.Write(r => r.Add(new BeatmapCollection(TextBox.Text)));
                    TextBox.Text = string.Empty;
                };
            }
        }

        private void saveCollectionsChanged(IRealmCollection<BeatmapCollection> collections, ChangeSet? changes)
        {
            var selectedId = saveToCollectionDropdown.Current.Value?.Collection?.ID;

            saveToCollectionItems.Clear();
            saveToCollectionItems.AddRange(collections.Select(c => new CollectionFilterMenuItem(c.ToLive(realm))));

            if (saveToCollectionItems.Count > 0)
            {
                var selectedItem = saveToCollectionItems.FirstOrDefault(i => i.Collection?.ID == selectedId) ?? saveToCollectionItems[0];
                saveToCollectionDropdown.Current.Value = selectedItem;
            }

            updateSaveButtonState();
        }

        private void updateSaveButtonState()
        {
            var currentId = saveToCollectionDropdown.Current.Value?.Collection?.ID;
            saveFilteredResultsButton.Enabled.Value = FilteredBeatmapsProvider != null
                                                      && currentId != null
                                                      && saveToCollectionItems.Any(item => item.Collection?.ID == currentId);
        }

        private void saveFilteredResults()
        {
            var provider = FilteredBeatmapsProvider;
            var collection = saveToCollectionDropdown.Current.Value?.Collection;

            if (provider == null || collection == null)
                return;
            // Collect distinct, non-empty hashes without intermediate lists
            var newHashes = new HashSet<string>();

            foreach (var beatmap in provider())
            {
                string h = beatmap.MD5Hash;
                if (!string.IsNullOrEmpty(h))
                    newHashes.Add(h);
            }

            if (newHashes.Count == 0)
                return;

            // Perform write on background thread. Use a HashSet of existing hashes
            // to avoid O(n) Contains calls against the realm list.
            Task.Run(() => collection.PerformWrite(c =>
            {
                var existing = new HashSet<string>(c.BeatmapMD5Hashes);

                foreach (string hash in newHashes)
                {
                    if (!existing.Contains(hash))
                    {
                        c.BeatmapMD5Hashes.Add(hash);
                        existing.Add(hash);
                    }
                }
            }));
        }

        private partial class SaveToCollectionDropdown : OsuDropdown<CollectionFilterMenuItem>
        {
            protected override LocalisableString GenerateItemText(CollectionFilterMenuItem item) => item.CollectionName;
        }
    }
}
