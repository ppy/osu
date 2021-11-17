// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    public class DrawableRoomPlaylistItem : OsuRearrangeableListItem<PlaylistItem>
    {
        public Action<PlaylistItem> RequestDeletion;

        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        private Container maskingContainer;
        private Container difficultyIconContainer;
        private LinkFlowContainer beatmapText;
        private LinkFlowContainer authorText;
        private ExplicitContentBeatmapPill explicitContentPill;
        private ModDisplay modDisplay;

        private readonly IBindable<bool> valid = new Bindable<bool>();

        private readonly Bindable<IBeatmapInfo> beatmap = new Bindable<IBeatmapInfo>();
        private readonly Bindable<IRulesetInfo> ruleset = new Bindable<IRulesetInfo>();
        private readonly BindableList<Mod> requiredMods = new BindableList<Mod>();

        public readonly PlaylistItem Item;

        private readonly bool allowEdit;
        private readonly bool allowSelection;

        protected override bool ShouldBeConsideredForInput(Drawable child) => allowEdit || !allowSelection || SelectedItem.Value == Model;

        public DrawableRoomPlaylistItem(PlaylistItem item, bool allowEdit, bool allowSelection)
            : base(item)
        {
            Item = item;

            // TODO: edit support should be moved out into a derived class
            this.allowEdit = allowEdit;
            this.allowSelection = allowSelection;

            beatmap.BindTo(item.Beatmap);
            valid.BindTo(item.Valid);
            ruleset.BindTo(item.Ruleset);
            requiredMods.BindTo(item.RequiredMods);

            ShowDragHandle.Value = allowEdit;
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!allowEdit)
                HandleColour = HandleColour.Opacity(0);

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

            refresh();
        }

        private PanelBackground panelBackground;

        private void refresh()
        {
            if (!valid.Value)
            {
                maskingContainer.BorderThickness = 5;
                maskingContainer.BorderColour = colours.Red;
            }

            difficultyIconContainer.Child = new DifficultyIcon(Item.Beatmap.Value, ruleset.Value, requiredMods, performBackgroundDifficultyLookup: false) { Size = new Vector2(32) };

            panelBackground.Beatmap.Value = Item.Beatmap.Value;

            beatmapText.Clear();
            beatmapText.AddLink(Item.Beatmap.Value.GetDisplayTitleRomanisable(), LinkAction.OpenBeatmap, Item.Beatmap.Value.OnlineID.ToString(), null, text =>
            {
                text.Truncate = true;
            });

            authorText.Clear();

            if (!string.IsNullOrEmpty(Item.Beatmap.Value?.Metadata.Author.Username))
            {
                authorText.AddText("mapped by ");
                authorText.AddUserLink(Item.Beatmap.Value.Metadata.Author);
            }

            bool hasExplicitContent = (Item.Beatmap.Value.BeatmapSet as IBeatmapSetOnlineInfo)?.HasExplicitContent == true;
            explicitContentPill.Alpha = hasExplicitContent ? 1 : 0;

            modDisplay.Current.Value = requiredMods.ToArray();
        }

        protected override Drawable CreateContent()
        {
            Action<SpriteText> fontParameters = s => s.Font = OsuFont.Default.With(weight: FontWeight.SemiBold);

            return maskingContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 50,
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
                                    Margin = new MarginPadding { Left = 8, Right = 8, },
                                },
                                new FillFlowContainer
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
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Direction = FillDirection.Horizontal,
                                    Margin = new MarginPadding { Left = 8, Right = 10, },
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(5),
                                    ChildrenEnumerable = CreateButtons().Select(button => button.With(b =>
                                    {
                                        b.Anchor = Anchor.Centre;
                                        b.Origin = Anchor.Centre;
                                    }))
                                }
                            }
                        }
                    },
                }
            };
        }

        protected virtual IEnumerable<Drawable> CreateButtons() =>
            new Drawable[]
            {
                new PlaylistDownloadButton(Item),
                new PlaylistRemoveButton
                {
                    Size = new Vector2(30, 30),
                    Alpha = allowEdit ? 1 : 0,
                    Action = () => RequestDeletion?.Invoke(Model),
                },
            };

        public class PlaylistRemoveButton : GrayButton
        {
            public PlaylistRemoveButton()
                : base(FontAwesome.Solid.MinusSquare)
            {
                TooltipText = "Remove from playlist";
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Icon.Scale = new Vector2(0.8f);
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (allowSelection && valid.Value)
                SelectedItem.Value = Model;
            return true;
        }

        private sealed class PlaylistDownloadButton : BeatmapPanelDownloadButton
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
    }
}
