// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Desktop.IPC.Messages;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IPC;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using osu.Game.Utils;
using BeatmapDifficulty = osu.Desktop.IPC.Messages.BeatmapDifficulty;
using BeatmapMetadata = osu.Desktop.IPC.Messages.BeatmapMetadata;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace osu.Desktop.IPC
{
    public partial class OsuWebSocketProvider : Component
    {
        private WebSocketServer? server;
        private readonly Bindable<UserActivity?> userActivity = new Bindable<UserActivity?>();

        [Resolved]
        private Bindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            server = new WebSocketServer(49727);
            server.StartAsync().FireAndForget(onError: ex => Logger.Error(ex, "Failed to start websocket"));

            sessionStatics.BindWith(Static.UserOnlineActivity, userActivity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userActivity.BindValueChanged(val =>
            {
                if (val.NewValue == null)
                    return;

                if (server?.IsRunning != true)
                    return;

                var msg = new UserActivityMessage
                {
                    Status = val.NewValue.GetType().Name,
                };

                broadcast(msg);
            }, true);

            workingBeatmap.BindValueChanged(val =>
            {
                // default beatmap on load
                if (val.NewValue is DummyWorkingBeatmap)
                    return;

                if (val.NewValue.BeatmapInfo.OnlineID == val.OldValue.BeatmapInfo.OnlineID)
                    return;

                if (server?.IsRunning != true)
                    return;

                var msg = new BeatmapMessage
                {
                    BeatmapId = val.NewValue.BeatmapInfo.OnlineID,
                    BeatmapSetId = val.NewValue.BeatmapSetInfo.OnlineID,
                    BeatmapHash = val.NewValue.BeatmapInfo.OnlineMD5Hash,
                    Metadata = new BeatmapMetadata
                    {
                        Artist = val.NewValue.BeatmapInfo.Metadata.Artist,
                        ArtistUnicode = val.NewValue.BeatmapInfo.Metadata.ArtistUnicode,
                        Title = val.NewValue.BeatmapInfo.Metadata.Title,
                        TitleUnicode = val.NewValue.BeatmapInfo.Metadata.TitleUnicode,
                        Author = val.NewValue.BeatmapInfo.Metadata.Author.Username,
                        Source = val.NewValue.BeatmapInfo.Metadata.Source,
                        Tags = val.NewValue.BeatmapInfo.Metadata.Tags,
                        UserTags = val.NewValue.BeatmapInfo.Metadata.UserTags.ToArray(),
                    },
                    Difficulty = new BeatmapDifficulty
                    {
                        ApproachRate = val.NewValue.BeatmapInfo.Difficulty.ApproachRate,
                        CircleSize = val.NewValue.BeatmapInfo.Difficulty.CircleSize,
                        DrainRate = val.NewValue.BeatmapInfo.Difficulty.DrainRate,
                        OverallDifficulty = val.NewValue.BeatmapInfo.Difficulty.OverallDifficulty,
                    },
                    DifficultyName = val.NewValue.BeatmapInfo.DifficultyName,
                    RulesetId = val.NewValue.BeatmapInfo.Ruleset.OnlineID,
                    BPM = Math.Round(val.NewValue.BeatmapInfo.BPM, 2),
                    StarRating = val.NewValue.BeatmapInfo.StarRating.FloorToDecimalDigits(2),
                    Status = val.NewValue.BeatmapInfo.Status,
                };

                broadcast(msg);
            }, true);
        }

        private void broadcast(OsuWebSocketMessage message)
        {
            if (server?.IsRunning != true)
                return;

            string messageString = JsonConvert.SerializeObject(message);
            server.BroadcastAsync(messageString).FireAndForget();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (server?.IsRunning == true)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                server.StopAsync(cts.Token).WaitSafely();
                server = null;
            }
        }
    }
}
