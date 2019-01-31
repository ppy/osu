// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Multi.Match
{
    public class MatchSubScreen : MultiplayerSubScreen
    {
        public override bool AllowBeatmapRulesetChange => false;

        public override string Title => room.RoomID.Value == null ? "New room" : room.Name.Value;
        public override string ShortTitle => "room";

        private readonly RoomBindings bindings = new RoomBindings();

        private readonly MatchLeaderboard leaderboard;

        private readonly Action<Screen> pushGameplayScreen;

        [Cached]
        private readonly Room room;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public MatchSubScreen(Room room, Action<Screen> pushGameplayScreen)
        {
            this.room = room;
            this.pushGameplayScreen = pushGameplayScreen;

            bindings.Room = room;

            MatchChatDisplay chat;
            Components.Header header;
            Info info;
            GridContainer bottomRow;
            MatchSettingsOverlay settings;

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[] { header = new Components.Header(room) { Depth = -1 } },
                        new Drawable[] { info = new Info(room) { OnStart = onStart } },
                        new Drawable[]
                        {
                            bottomRow = new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        leaderboard = new MatchLeaderboard
                                        {
                                            Padding = new MarginPadding
                                            {
                                                Left = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                                                Right = 10,
                                                Vertical = 10,
                                            },
                                            RelativeSizeAxes = Axes.Both,
                                            Room = room
                                        },
                                        new Container
                                        {
                                            Padding = new MarginPadding
                                            {
                                                Left = 10,
                                                Right = 10 + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                                                Vertical = 10,
                                            },
                                            RelativeSizeAxes = Axes.Both,
                                            Child = chat = new MatchChatDisplay(room)
                                            {
                                                RelativeSizeAxes = Axes.Both
                                            }
                                        },
                                    },
                                },
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Components.Header.HEIGHT },
                    Child = settings = new MatchSettingsOverlay(room) { RelativeSizeAxes = Axes.Both },
                },
            };

            header.OnRequestSelectBeatmap = () => this.Push(new MatchSongSelect
            {
                Selected = addPlaylistItem,
            });

            header.Tabs.Current.ValueChanged += t =>
            {
                const float fade_duration = 500;
                if (t is SettingsMatchPage)
                {
                    settings.Show();
                    info.FadeOut(fade_duration, Easing.OutQuint);
                    bottomRow.FadeOut(fade_duration, Easing.OutQuint);
                }
                else
                {
                    settings.Hide();
                    info.FadeIn(fade_duration, Easing.OutQuint);
                    bottomRow.FadeIn(fade_duration, Easing.OutQuint);
                }
            };

            chat.Exit += () =>
            {
                if (this.IsCurrentScreen())
                    this.Exit();
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapManager.ItemAdded += beatmapAdded;
        }

        public override bool OnExiting(IScreen next)
        {
            Manager?.PartRoom();
            return base.OnExiting(next);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bindings.CurrentBeatmap.BindValueChanged(setBeatmap, true);
            bindings.CurrentRuleset.BindValueChanged(setRuleset, true);
        }

        private void setBeatmap(BeatmapInfo beatmap)
        {
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

            Game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
        }

        private void setRuleset(RulesetInfo ruleset)
        {
            if (ruleset == null)
                return;

            Game?.ForcefullySetRuleset(ruleset);
        }

        private void beatmapAdded(BeatmapSetInfo model, bool existing, bool silent) => Schedule(() =>
        {
            if (Beatmap.Value != beatmapManager.DefaultBeatmap)
                return;

            if (bindings.CurrentBeatmap.Value == null)
                return;

            // Try to retrieve the corresponding local beatmap
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == bindings.CurrentBeatmap.Value.OnlineBeatmapID);

            if (localBeatmap != null)
                Game?.ForcefullySetBeatmap(beatmapManager.GetWorkingBeatmap(localBeatmap));
        });

        private void addPlaylistItem(PlaylistItem item)
        {
            bindings.Playlist.Clear();
            bindings.Playlist.Add(item);
        }

        private void onStart()
        {
            Beatmap.Value.Mods.Value = bindings.CurrentMods.Value.ToArray();

            switch (bindings.Type.Value)
            {
                default:
                case GameTypeTimeshift _:
                    pushGameplayScreen?.Invoke(new PlayerLoader(() => new TimeshiftPlayer(room, room.Playlist.First().ID)
                    {
                        Exited = () => leaderboard.RefreshScores()
                    }));
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmapManager != null)
                beatmapManager.ItemAdded -= beatmapAdded;
        }
    }
}
