// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    public class DrawableRoomPlaylistItem : OsuRearrangeableListItem<PlaylistItem>
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

        private readonly DelayedLoadWrapper onScreenLoader = new DelayedLoadWrapper(Empty) { RelativeSizeAxes = Axes.Both };
        private readonly IBindable<bool> valid = new Bindable<bool>();
        private readonly Bindable<IBeatmapInfo> beatmap = new Bindable<IBeatmapInfo>();
        private readonly Bindable<IRulesetInfo> ruleset = new Bindable<IRulesetInfo>();
        private readonly BindableList<Mod> requiredMods = new BindableList<Mod>();

        private Container maskingContainer;
        private Container difficultyIconContainer;
        private LinkFlowContainer beatmapText;
        private LinkFlowContainer authorText;
        private ExplicitContentBeatmapPill explicitContentPill;
        private ModDisplay modDisplay;
        private FillFlowContainer buttonsFlow;
        private UpdateableAvatar ownerAvatar;
        private Drawable showResultsButton;
        private Drawable editButton;
        private Drawable removeButton;
        private PanelBackground panelBackground;
        private FillFlowContainer mainFillFlow;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private MultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; }

        protected override bool ShouldBeConsideredForInput(Drawable child) => AllowReordering || AllowDeletion || !AllowSelection || SelectedItem.Value == Model;

        public DrawableRoomPlaylistItem(PlaylistItem item)
            : base(item)
        {
            Item = item;

            beatmap.BindTo(item.Beatmap);
            valid.BindTo(item.Valid);
            ruleset.BindTo(item.Ruleset);
            requiredMods.BindTo(item.RequiredMods);

            if (item.Expired)
                Colour = OsuColour.Gray(0.5f);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskingContainer.BorderColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(selected =>
            {
                bool isCurrent = selected.NewValue == Model;

                if (!valid.Value)
                {
                    // Don't allow selection when not valid.
                    if (isCurrent)
                    {
                        SelectedItem.Value = selected.OldValue;
                    }

                    // Don't update border when not valid (the border is displaying this fact).
                    return;
                }

                maskingContainer.BorderThickness = isCurrent ? 5 : 0;
            }, true);

            beatmap.BindValueChanged(_ => Scheduler.AddOnce(refresh));
            ruleset.BindValueChanged(_ => Scheduler.AddOnce(refresh));
            valid.BindValueChanged(_ => Scheduler.AddOnce(refresh));
            requiredMods.CollectionChanged += (_, __) => Scheduler.AddOnce(refresh);

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

                        if (Item.Beatmap.Value == null)
                        {
                            IBeatmapInfo foundBeatmap;

                            if (multiplayerClient != null)
                                // This call can eventually go away (and use the else case below).
                                // Currently required only due to the method being overridden to provide special behaviour in tests.
                                foundBeatmap = await multiplayerClient.GetAPIBeatmap(Item.BeatmapID).ConfigureAwait(false);
                            else
                                foundBeatmap = await beatmapLookupCache.GetBeatmapAsync(Item.BeatmapID).ConfigureAwait(false);

                            Schedule(() => Item.Beatmap.Value = foundBeatmap);
                        }
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

            if (Item.Beatmap.Value != null)
                difficultyIconContainer.Child = new DifficultyIcon(Item.Beatmap.Value, ruleset.Value, requiredMods, performBackgroundDifficultyLookup: false) { Size = new Vector2(icon_height) };
            else
                difficultyIconContainer.Clear();

            panelBackground.Beatmap.Value = Item.Beatmap.Value;

            beatmapText.Clear();

            if (Item.Beatmap.Value != null)
            {
                beatmapText.AddLink(Item.Beatmap.Value.GetDisplayTitleRomanisable(), LinkAction.OpenBeatmap, Item.Beatmap.Value.OnlineID.ToString(), null, text =>
                {
                    text.Truncate = true;
                });
            }

            authorText.Clear();

            if (!string.IsNullOrEmpty(Item.Beatmap.Value?.Metadata.Author.Username))
            {
                authorText.AddText("mapped by ");
                authorText.AddUserLink(Item.Beatmap.Value.Metadata.Author);
            }

            bool hasExplicitContent = (Item.Beatmap.Value?.BeatmapSet as IBeatmapSetOnlineInfo)?.HasExplicitContent == true;
            explicitContentPill.Alpha = hasExplicitContent ? 1 : 0;

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
                                                        explicitContentPill = new ExplicitContentBeatmapPill
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
                }
            };
        }

        private IEnumerable<Drawable> createButtons() => new[]
        {
            showResultsButton = new GrayButton(FontAwesome.Solid.ChartPie)
            {
                Size = new Vector2(30, 30),
                Action = () => RequestResults?.Invoke(Item),
                Alpha = AllowShowingResults ? 1 : 0,
                TooltipText = "View results"
            },
            Item.Beatmap.Value == null ? Empty() : new PlaylistDownloadButton(Item),
            editButton = new PlaylistEditButton
            {
                Size = new Vector2(30, 30),
                Alpha = AllowEditing ? 1 : 0,
                Action = () => RequestEdit?.Invoke(Item),
                TooltipText = "Edit"
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

        public class PlaylistEditButton : GrayButton
        {
            public PlaylistEditButton()
                : base(FontAwesome.Solid.Edit)
            {
            }
        }

        public class PlaylistRemoveButton : GrayButton
        {
            public PlaylistRemoveButton()
                : base(FontAwesome.Solid.MinusSquare)
            {
            }
        }

        private sealed class PlaylistDownloadButton : BeatmapDownloadButton
        {
            private readonly PlaylistItem playlistItem;

            [Resolved]
            private BeatmapManager beatmapManager { get; set; }

            // required for download tracking, as this button hides itself. can probably be removed with a bit of consideration.
            public override bool IsPresent => true;

            private const float width = 50;

            public PlaylistDownloadButton(PlaylistItem playlistItem)
                : base(playlistItem.Beatmap.Value.BeatmapSet)
            {
                this.playlistItem = playlistItem;

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
                    case DownloadState.LocallyAvailable:
                        // Perform a local query of the beatmap by beatmap checksum, and reset the state if not matching.
                        if (beatmapManager.QueryBeatmap(b => b.MD5Hash == playlistItem.Beatmap.Value.MD5Hash) == null)
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
        private class PanelBackground : Container // todo: should be a buffered container (https://github.com/ppy/osu-framework/issues/3222)
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

        private class OwnerAvatar : UpdateableAvatar, IHasTooltip
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

            private class TooltipArea : Component, IHasTooltip
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
