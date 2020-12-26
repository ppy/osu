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
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.BeatmapListing.Panels;
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
        private ModDisplay modDisplay;

        private readonly Bindable<BeatmapInfo> beatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();
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
            ruleset.BindTo(item.Ruleset);
            requiredMods.BindTo(item.RequiredMods);

            ShowDragHandle.Value = allowEdit;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (!allowEdit)
                HandleColour = HandleColour.Opacity(0);

            maskingContainer.BorderColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(selected => maskingContainer.BorderThickness = selected.NewValue == Model ? 5 : 0, true);

            beatmap.BindValueChanged(_ => scheduleRefresh());
            ruleset.BindValueChanged(_ => scheduleRefresh());

            requiredMods.CollectionChanged += (_, __) => scheduleRefresh();

            refresh();
        }

        private ScheduledDelegate scheduledRefresh;

        private void scheduleRefresh()
        {
            scheduledRefresh?.Cancel();
            scheduledRefresh = Schedule(refresh);
        }

        private void refresh()
        {
            difficultyIconContainer.Child = new DifficultyIcon(beatmap.Value, ruleset.Value, requiredMods) { Size = new Vector2(32) };

            beatmapText.Clear();
            beatmapText.AddLink(Item.Beatmap.ToString(), LinkAction.OpenBeatmap, Item.Beatmap.Value.OnlineBeatmapID.ToString());

            authorText.Clear();

            if (Item.Beatmap?.Value?.Metadata?.Author != null)
            {
                authorText.AddText("mapped by ");
                authorText.AddUserLink(Item.Beatmap.Value?.Metadata.Author);
            }

            modDisplay.Current.Value = requiredMods.ToArray();
        }

        protected override Drawable CreateContent() => maskingContainer = new Container
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
                new PanelBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    Beatmap = { BindTarget = beatmap }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 8 },
                    Spacing = new Vector2(8, 0),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        difficultyIconContainer = new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                beatmapText = new LinkFlowContainer { AutoSizeAxes = Axes.Both },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(15, 0),
                                    Children = new Drawable[]
                                    {
                                        authorText = new LinkFlowContainer { AutoSizeAxes = Axes.Both },
                                        modDisplay = new ModDisplay
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Scale = new Vector2(0.4f),
                                            DisplayUnrankedText = false,
                                            ExpansionMode = ExpansionMode.AlwaysExpanded
                                        }
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
                    AutoSizeAxes = Axes.Both,
                    X = -18,
                    ChildrenEnumerable = CreateButtons()
                }
            }
        };

        protected virtual IEnumerable<Drawable> CreateButtons() =>
            new Drawable[]
            {
                new PlaylistDownloadButton(Item)
                {
                    Size = new Vector2(50, 30)
                },
                new IconButton
                {
                    Icon = FontAwesome.Solid.MinusSquare,
                    Alpha = allowEdit ? 1 : 0,
                    Action = () => RequestDeletion?.Invoke(Model),
                },
            };

        protected override bool OnClick(ClickEvent e)
        {
            if (allowSelection)
                SelectedItem.Value = Model;
            return true;
        }

        private class PlaylistDownloadButton : BeatmapPanelDownloadButton
        {
            private readonly PlaylistItem playlistItem;

            [Resolved]
            private BeatmapManager beatmapManager { get; set; }

            public PlaylistDownloadButton(PlaylistItem playlistItem)
                : base(playlistItem.Beatmap.Value.BeatmapSet)
            {
                this.playlistItem = playlistItem;
                Alpha = 0;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                State.BindValueChanged(stateChanged, true);
                FinishTransforms(true);
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
                            this.FadeTo(0, 500);

                        break;

                    default:
                        this.FadeTo(1, 500);
                        break;
                }
            }
        }

        // For now, this is the same implementation as in PanelBackground, but supports a beatmap info rather than a working beatmap
        private class PanelBackground : Container // todo: should be a buffered container (https://github.com/ppy/osu-framework/issues/3222)
        {
            public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

            public PanelBackground()
            {
                InternalChildren = new Drawable[]
                {
                    new UpdateableBeatmapBackgroundSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Beatmap = { BindTarget = Beatmap }
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
                            // Piecewise-linear gradient with 3 segments to make it appear smoother
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                Width = 0.05f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                Width = 0.2f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                                Width = 0.05f,
                            },
                        }
                    }
                };
            }
        }
    }
}
