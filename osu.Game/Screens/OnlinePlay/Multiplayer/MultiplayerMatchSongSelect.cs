// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerMatchSongSelect : OnlinePlaySongSelect
    {
        [Resolved]
        private MultiplayerClient client { get; set; }

        private readonly long? itemToEdit;

        private LoadingLayer loadingLayer;

        /// <summary>
        /// Construct a new instance of multiplayer song select.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="itemToEdit">The item to be edited. May be null, in which case a new item will be added to the playlist.</param>
        /// <param name="beatmap">An optional initial beatmap selection to perform.</param>
        /// <param name="ruleset">An optional initial ruleset selection to perform.</param>
        public MultiplayerMatchSongSelect(Room room, long? itemToEdit = null, WorkingBeatmap beatmap = null, RulesetInfo ruleset = null)
            : base(room)
        {
            this.itemToEdit = itemToEdit;

            if (beatmap != null || ruleset != null)
            {
                Schedule(() =>
                {
                    if (beatmap != null) Beatmap.Value = beatmap;
                    if (ruleset != null) Ruleset.Value = ruleset;
                });
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(true));
        }

        protected override void SelectItem(PlaylistItem item)
        {
            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                loadingLayer.Show();

                var multiplayerItem = new MultiplayerPlaylistItem
                {
                    ID = itemToEdit ?? 0,
                    BeatmapID = item.BeatmapID,
                    BeatmapChecksum = item.Beatmap.Value.MD5Hash,
                    RulesetID = item.RulesetID,
                    RequiredMods = item.RequiredMods.Select(m => new APIMod(m)).ToArray(),
                    AllowedMods = item.AllowedMods.Select(m => new APIMod(m)).ToArray()
                };

                Task task = itemToEdit != null ? client.EditPlaylistItem(multiplayerItem) : client.AddPlaylistItem(multiplayerItem);

                task.ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        loadingLayer.Hide();

                        if (t.IsFaulted)
                        {
                            Exception exception = t.Exception;

                            if (exception is AggregateException ae)
                                exception = ae.InnerException;

                            Debug.Assert(exception != null);

                            string message = exception is HubException
                                // HubExceptions arrive with additional message context added, but we want to display the human readable message:
                                // "An unexpected error occurred invoking 'AddPlaylistItem' on the server.InvalidStateException: Can't enqueue more than 3 items at once."
                                // We generally use the message field for a user-parseable error (eventually to be replaced), so drop the first part for now.
                                ? exception.Message.Substring(exception.Message.IndexOf(':') + 1).Trim()
                                : exception.Message;

                            Logger.Log(message, level: LogLevel.Important);
                            Carousel.AllowSelection = true;
                            return;
                        }

                        this.Exit();
                    });
                });
            }
            else
            {
                Playlist.Clear();
                Playlist.Add(item);
                this.Exit();
            }
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        protected override bool IsValidFreeMod(Mod mod) => base.IsValidFreeMod(mod) && !(mod is ModTimeRamp) && !(mod is ModRateAdjust);
    }
}
