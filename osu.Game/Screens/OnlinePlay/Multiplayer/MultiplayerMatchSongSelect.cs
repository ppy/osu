// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerMatchSongSelect : OnlinePlaySongSelect
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OngoingOperationTracker operationTracker { get; set; } = null!;

        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();
        private readonly PlaylistItem? itemToEdit;

        private LoadingLayer loadingLayer = null!;
        private IDisposable? selectionOperation;

        /// <summary>
        /// Construct a new instance of multiplayer song select.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="itemToEdit">The item to be edited. May be null, in which case a new item will be added to the playlist.</param>
        public MultiplayerMatchSongSelect(Room room, PlaylistItem? itemToEdit = null)
            : base(room, itemToEdit)
        {
            this.itemToEdit = itemToEdit;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(true));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            operationInProgress.BindTo(operationTracker.InProgress);
            operationInProgress.BindValueChanged(_ => updateLoadingLayer(), true);
        }

        private void updateLoadingLayer()
        {
            if (operationInProgress.Value)
                loadingLayer.Show();
            else
                loadingLayer.Hide();
        }

        protected override bool SelectItem(PlaylistItem item)
        {
            if (operationInProgress.Value)
            {
                Logger.Log($"{nameof(SelectItem)} aborted due to {nameof(operationInProgress)}");
                return false;
            }

            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                selectionOperation = operationTracker.BeginOperation();

                var multiplayerItem = new MultiplayerPlaylistItem
                {
                    ID = itemToEdit?.ID ?? 0,
                    BeatmapID = item.Beatmap.OnlineID,
                    BeatmapChecksum = item.Beatmap.MD5Hash,
                    RulesetID = item.RulesetID,
                    RequiredMods = item.RequiredMods.ToArray(),
                    AllowedMods = item.AllowedMods.ToArray()
                };

                Task task = itemToEdit != null ? client.EditPlaylistItem(multiplayerItem) : client.AddPlaylistItem(multiplayerItem);

                task.FireAndForget(onSuccess: () =>
                {
                    selectionOperation.Dispose();

                    Schedule(() =>
                    {
                        // If an error or server side trigger occurred this screen may have already exited by external means.
                        if (this.IsCurrentScreen())
                            this.Exit();
                    });
                }, onError: _ =>
                {
                    selectionOperation.Dispose();

                    Schedule(() =>
                    {
                        Carousel.AllowSelection = true;
                    });
                });
            }
            else
            {
                Playlist.Clear();
                Playlist.Add(item);
                this.Exit();
            }

            return true;
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        protected override bool IsValidMod(Mod mod) => base.IsValidMod(mod) && mod.ValidForMultiplayer;

        protected override bool IsValidFreeMod(Mod mod) => base.IsValidFreeMod(mod) && mod.ValidForMultiplayerAsFreeMod;
    }
}
