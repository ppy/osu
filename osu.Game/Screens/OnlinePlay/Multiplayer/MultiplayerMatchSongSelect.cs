// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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

        private LoadingLayer loadingLayer;

        /// <summary>
        /// Construct a new instance of multiplayer song select.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="beatmap">An optional initial beatmap selection to perform.</param>
        /// <param name="ruleset">An optional initial ruleset selection to perform.</param>
        public MultiplayerMatchSongSelect(Room room, WorkingBeatmap beatmap = null, RulesetInfo ruleset = null)
            : base(room)
        {
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

                client.ChangeSettings(item: item).ContinueWith(t =>
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
