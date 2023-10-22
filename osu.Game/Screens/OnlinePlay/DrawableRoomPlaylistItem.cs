// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class DrawableRoomPlaylistItem : OsuRearrangeableListItem<PlaylistItem>, IHasContextMenu
    {
        public const float HEIGHT = 50;

        private const float icon_height = 34;

        /// <summary>
        /// Invoked when this item requests to be deleted.
        /// </summary>
        public Action<PlaylistItem> RequestDeletion;

        /// <summary>
        /// Invoked when this item requests its results to be shown.
        /// </summary>
        public Action<PlaylistItem> RequestResults;

        /// <summary>
        /// Invoked when this item requests to be edited.
        /// </summary>
        public Action<PlaylistItem> RequestEdit;

        /// <summary>
        /// The currently-selected item, used to show a border around this item.
        /// May be updated by this item if <see cref="AllowSelection"/> is <c>true</c>.
        /// </summary>
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        public readonly PlaylistItem Item;

        public bool IsSelectedItem => SelectedItem.Value?.ID == Item.ID;

        private readonly DelayedLoadWrapper onScreenLoader = new DelayedLoadWrapper(Empty) { RelativeSizeAxes = Axes.Both };
        private readonly IBindable<bool> valid = new Bindable<bool>();

        private IBeatmapInfo beatmap;
        private IRulesetInfo ruleset;
        private Mod[] requiredMods = Array.Empty<Mod>();

        private Container maskingContainer;
        private Container difficultyIconContainer;
        private LinkFlowContainer beatmapText;
        private LinkFlowContainer authorText;
        private ExplicitContentBeatmapBadge explicitContent;
        private ModDisplay modDisplay;
        private FillFlowContainer buttonsFlow;
        private UpdateableAvatar ownerAvatar;
        private Drawable showResultsButton;
        private Drawable editButton;
        private Drawable removeButton;
        private PanelBackground panelBackground;
        private FillFlowContainer mainFillFlow;

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; }

        [Resolved(CanBeNull = true)]
        private BeatmapSetOverlay beatmapOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ManageCollectionsDialog manageCollectionsDialog { get; set; }

        protected override bool ShouldBeConsideredForInput(Drawable child) => AllowReordering || AllowDeletion || !AllowSelection || SelectedItem.Value == Model;

        public DrawableRoomPlaylistItem(PlaylistItem item)
            : base(item)
        {
            Item = item;

            valid.BindTo(item.Valid);

            if (item.Expired)
                Colour = OsuColour.Gray(0.5f);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskingContainer.BorderColour = colours.Yellow;

            ruleset = rulesets.GetRuleset(Item.RulesetID);
            var rulesetInstance = ruleset?.CreateInstance();

            if (rulesetInstance != null)
                requiredMods = Item.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(selected =>
            {
                if (!valid.Value)
                {
                    // Don't allow selection when not valid.
                    if (IsSelectedItem)
                    {
                        SelectedItem.Value = selected.OldValue;
                    }

                    // Don't update border when not valid (the border is displaying this fact).
                    return;
                }

                maskingContainer.BorderThickness = IsSelectedItem ? 5 : 0;
            }, true);

            valid.BindValueChanged(_ => Scheduler.AddOnce(refresh));

            onScreenLoader.DelayedLoadStarted += _ =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (showItemOwner)
                        {
                            var foundUser = await userLookupCache.GetUserAsync(Item.OwnerID).ConfigureAwait(false);
                            Schedule(() => ownerAvatar.User = foundUser);
                        }

                        beatmap = await beatmapLookupCache.GetBeatmapAsync(Item.Beatmap.OnlineID).ConfigureAwait(false);

                        Scheduler.AddOnce(refresh);
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Error while populating playlist item {e}");
                    }
                });
            };

            refresh();
        }

        /// <summary>
        /// Whether this item can be selected.
        /// </summary>
        public bool AllowSelection { get; set; }

        /// <summary>
        /// Whether this item can be reordered in the playlist.
        /// </summary>
        public bool AllowReordering
        {
            get => ShowDragHandle.Value;
            set => ShowDragHandle.Value = value;
        }

        private bool allowDeletion;

        /// <summary>
        /// Whether this item can be deleted.
        /// </summary>
        public bool AllowDeletion
        {
            get => allowDeletion;
            set
            {
                allowDeletion = value;

                if (removeButton != null)
                    removeButton.Alpha = value ? 1 : 0;
            }
        }

        private bool allowShowingResults;

        /// <summary>
        /// Whether this item can have results shown.
        /// </summary>
        public bool AllowShowingResults
        {
            get => allowShowingResults;
            set
            {
                allowShowingResults = value;

                if (showResultsButton != null)
                    showResultsButton.Alpha = value ? 1 : 0;
            }
        }

        private bool allowEditing;

        /// <summary>
        /// Whether this item can be edited.
        /// </summary>
        public bool AllowEditing
        {
            get => allowEditing;
            set
            {
                allowEditing = value;

                if (editButton != null)
                    editButton.Alpha = value ? 1 : 0;
            }
        }

        private bool showItemOwner;

        /// <summary>
        /// Whether to display the avatar of the user which owns this playlist item.
        /// </summary>
        public bool ShowItemOwner
        {
            get => showItemOwner;
            set
            {
                showItemOwner = value;

                if (ownerAvatar != null)
                    ownerAvatar.Alpha = value ? 1 : 0;
            }
        }

        private void refresh()
        {
            if (!valid.Value)
            {
                maskingContainer.BorderThickness = 5;
                maskingContainer.BorderColour = colours.Red;
            }

            if (beatmap != null)
                difficultyIconContainer.Child = new DifficultyIcon(beatmap, ruleset) { Size = new Vector2(icon_height) };
            else
                difficultyIconContainer.Clear();

            panelBackground.Beatmap.Value = beatmap;

            beatmapText.Clear();

            if (beatmap != null)
            {
                beatmapText.AddLink(beatmap.GetDisplayTitleRomanisable(includeCreator: false),
                    LinkAction.OpenBeatmap,
                    beatmap.OnlineID.ToString(),
                    null,
                    text =>
                    {
                        text.Truncate = true;
                    });
            }

            authorText.Clear();

            if (!string.IsNullOrEmpty(beatmap?.Metadata.Author.Username))
            {
                authorText.AddText("mapped by ");
                authorText.AddUserLink(beatmap.Metadata.Author);
            }

            bool hasExplicitContent = beatmap?.BeatmapSet is IBeatmapSetOnlineInfo { HasExplicitContent: true };
            explicitContent.Alpha = hasExplicitContent ? 1 : 0;

            modDisplay.Current.Value = requiredMods.ToArray();

            buttonsFlow.Clear();
            buttonsFlow.ChildrenEnumerable = createButtons();

            difficultyIconContainer.FadeInFromZero(500, Easing.OutQuint);
            mainFillFlow.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override Drawable CreateContent()
        {
            Action<SpriteText> fontParameters = s => s.Font = OsuFont.Default.With(weight: FontWeight.SemiBold);

            return maskingContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = HEIGHT,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box // A transparent box that forces the border to be drawn if the panel background is opaque
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    },
                    onScreenLoader,
                    panelBackground = new PanelBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                difficultyIconContainer = new Container
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Left = 8, Right = 8 },
                                },
                                mainFillFlow = new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        beatmapText = new LinkFlowContainer(fontParameters)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            // workaround to ensure only the first line of text shows, emulating truncation (but without ellipsis at the end).
                                            // TODO: remove when text/link flow can support truncation with ellipsis natively.
                                            Height = OsuFont.DEFAULT_FONT_SIZE,
                                            Masking = true
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(10f, 0),
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10f, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        authorText = new LinkFlowContainer(fontParameters) { AutoSizeAxes = Axes.Both },
                                                        explicitContent = new ExplicitContentBeatmapBadge
                                                        {
                                                            Alpha = 0f,
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Margin = new MarginPadding { Top = 3f },
                                                        }
                                                    },
                                                },
                                                new Container
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    AutoSizeAxes = Axes.Both,
                                                    Child = modDisplay = new ModDisplay
                                                    {
                                                        Scale = new Vector2(0.4f),
                                                        ExpansionMode = ExpansionMode.AlwaysExpanded
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                buttonsFlow = new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Direction = FillDirection.Horizontal,
                                    Margin = new MarginPadding { Horizontal = 8 },
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(5),
                                    ChildrenEnumerable = createButtons().Select(button => button.With(b =>
                                    {
                                        b.Anchor = Anchor.Centre;
                                        b.Origin = Anchor.Centre;
                                    }))
                                },
                                ownerAvatar = new OwnerAvatar
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_height),
                                    Margin = new MarginPadding { Right = 8 },
                                    Masking = true,
                                    CornerRadius = 4,
                                    Alpha = ShowItemOwner ? 1 : 0
                                },
                            }
                        }
                    },
                },
            };
        }

        private IEnumerable<Drawable> createButtons() => new[]
        {
            beatmap == null ? Empty() : new PlaylistDownloadButton(beatmap),
            showResultsButton = new GrayButton(FontAwesome.Solid.ChartPie)
            {
                Size = new Vector2(30, 30),
                Action = () => RequestResults?.Invoke(Item),
                Alpha = AllowShowingResults ? 1 : 0,
                TooltipText = "View results"
            },
            editButton = new PlaylistEditButton
            {
                Size = new Vector2(30, 30),
                Alpha = AllowEditing ? 1 : 0,
                Action = () => RequestEdit?.Invoke(Item),
                TooltipText = CommonStrings.ButtonsEdit
            },
            removeButton = new PlaylistRemoveButton
            {
                Size = new Vector2(30, 30),
                Alpha = AllowDeletion ? 1 : 0,
                Action = () => RequestDeletion?.Invoke(Item),
                TooltipText = "Remove from playlist"
            },
        };

        protected override bool OnClick(ClickEvent e)
        {
            if (AllowSelection && valid.Value)
                SelectedItem.Value = Model;
            return true;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (beatmapOverlay != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay.FetchAndShowBeatmap(Item.Beatmap.OnlineID)));

                if (beatmap != null)
                {
                    if (beatmaps.QueryBeatmap(b => b.OnlineID == beatmap.OnlineID) is BeatmapInfo local && !local.BeatmapSet.AsNonNull().DeletePending)
                    {
                        var collectionItems = realm.Realm.All<BeatmapCollection>()
                                                   .OrderBy(c => c.Name)
                                                   .AsEnumerable()
                                                   .Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmap)).Cast<OsuMenuItem>().ToList();

                        if (manageCollectionsDialog != null)
                            collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                        items.Add(new OsuMenuItem("Collections") { Items = collectionItems });
                    }
                }

                return items.ToArray();
            }
        }

        public partial class PlaylistEditButton : GrayButton
        {
            public PlaylistEditButton()
                : base(FontAwesome.Solid.Edit)
            {
            }
        }

        public partial class PlaylistRemoveButton : GrayButton
        {
            public PlaylistRemoveButton()
                : base(FontAwesome.Solid.MinusSquare)
            {
            }
        }

        private sealed partial class PlaylistDownloadButton : BeatmapDownloadButton
        {
            private readonly IBeatmapInfo beatmap;

            [Resolved]
            private BeatmapManager beatmapManager { get; set; }

            // required for download tracking, as this button hides itself. can probably be removed with a bit of consideration.
            public override bool IsPresent => true;

            private const float width = 50;

            public PlaylistDownloadButton(IBeatmapInfo beatmap)
                : base(beatmap.BeatmapSet)
            {
                this.beatmap = beatmap;

                Size = new Vector2(width, 30);
                Alpha = 0;
            }

            protected override void LoadComplete()
            {
                State.BindValueChanged(stateChanged, true);

                // base implementation calls FinishTransforms, so should be run after the above state update.
                base.LoadComplete();
            }

            private void stateChanged(ValueChangedEvent<DownloadState> state)
            {
                switch (state.NewValue)
                {
                    case DownloadState.Unknown:
                        // Ignore initial state to ensure the button doesn't briefly appear.
                        break;

                    case DownloadState.LocallyAvailable:
                        // Perform a local query of the beatmap by beatmap checksum, and reset the state if not matching.
                        if (beatmapManager.QueryBeatmap(b => b.MD5Hash == beatmap.MD5Hash) == null)
                            State.Value = DownloadState.NotDownloaded;
                        else
                        {
                            this.FadeTo(0, 500)
                                .ResizeWidthTo(0, 500, Easing.OutQuint);
                        }

                        break;

                    default:
                        this.ResizeWidthTo(width, 500, Easing.OutQuint)
                            .FadeTo(1, 500);
                        break;
                }
            }
        }

        // For now, this is the same implementation as in PanelBackground, but supports a beatmap info rather than a working beatmap
        private partial class PanelBackground : Container // todo: should be a buffered container (https://github.com/ppy/osu-framework/issues/3222)
        {
            public readonly Bindable<IBeatmapInfo> Beatmap = new Bindable<IBeatmapInfo>();

            public PanelBackground()
            {
                UpdateableBeatmapBackgroundSprite backgroundSprite;

                InternalChildren = new Drawable[]
                {
                    backgroundSprite = new UpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                    },
                    new FillFlowContainer
                    {
                        Depth = -1,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                        Shear = new Vector2(0.8f, 0),
                        Alpha = 0.5f,
                        Children = new[]
                        {
                            // The left half with no gradient applied
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Width = 0.4f,
                            },
                            // Piecewise-linear gradient with 2 segments to make it appear smoother
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.7f)),
                                Width = 0.4f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.7f), new Color4(0, 0, 0, 0.4f)),
                                Width = 0.4f,
                            },
                        }
                    }
                };

                // manual binding required as playlists don't expose IBeatmapInfo currently.
                // may be removed in the future if this changes.
                Beatmap.BindValueChanged(beatmap => backgroundSprite.Beatmap.Value = beatmap.NewValue);
            }
        }

        private partial class OwnerAvatar : UpdateableAvatar, IHasTooltip
        {
            public OwnerAvatar()
            {
                AddInternal(new TooltipArea(this)
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1
                });
            }

            public LocalisableString TooltipText => User == null ? string.Empty : $"queued by {User.Username}";

            private partial class TooltipArea : Component, IHasTooltip
            {
                private readonly OwnerAvatar avatar;

                public TooltipArea(OwnerAvatar avatar)
                {
                    this.avatar = avatar;
                }

                public LocalisableString TooltipText => avatar.TooltipText;
            }
        }
    }
}
