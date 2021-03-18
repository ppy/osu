// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private StatefulMultiplayerClient client { get; set; }

        private LoadingLayer loadingLayer;

        /// <summary>
        /// Construct a new instance of multiplayer song select.
        /// </summary>
        /// <param name="beatmap">An optional initial beatmap selection to perform.</param>
        /// <param name="ruleset">An optional initial ruleset selection to perform.</param>
        public MultiplayerMatchSongSelect(WorkingBeatmap beatmap = null, RulesetInfo ruleset = null)
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

                        if (t.IsCompletedSuccessfully)
                            this.Exit();
                        else
                        {
                            Logger.Log($"Could not use current beatmap ({t.Exception?.Message})", level: LogLevel.Important);
                            Carousel.AllowSelection = true;
                        }
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
