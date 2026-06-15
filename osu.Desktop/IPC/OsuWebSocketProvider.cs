// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

                broadcast(msg).FireAndForget();
            }, true);

            workingBeatmap.BindValueChanged(val =>
            {
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
                        ApproachRate = Math.Round(val.NewValue.BeatmapInfo.Difficulty.ApproachRate, 2),
                        CircleSize = Math.Round(val.NewValue.BeatmapInfo.Difficulty.CircleSize, 2),
                        DrainRate = Math.Round(val.NewValue.BeatmapInfo.Difficulty.DrainRate, 2),
                        OverallDifficulty = Math.Round(val.NewValue.BeatmapInfo.Difficulty.OverallDifficulty, 2),
                    },
                    DifficultyName = val.NewValue.BeatmapInfo.DifficultyName,
                    RulesetId = val.NewValue.BeatmapInfo.Ruleset.OnlineID,
                    BPM = Math.Round(val.NewValue.BeatmapInfo.BPM, 2),
                    StarRating = val.NewValue.BeatmapInfo.StarRating.FloorToDecimalDigits(2),
                    Status = val.NewValue.BeatmapInfo.Status,
                };

                broadcast(msg).FireAndForget();
            }, true);
        }

        private Task broadcast(OsuWebSocketMessage message) => Task.Run(async () =>
        {
            if (server?.IsRunning != true)
                return;

            string messageString = JsonSerializer.Serialize(message, message.GetType());
            await server.BroadcastAsync(messageString).ConfigureAwait(false);
        });

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
