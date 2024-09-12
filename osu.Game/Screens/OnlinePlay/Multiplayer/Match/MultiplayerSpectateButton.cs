// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerSpectateButton : MultiplayerRoomComposite
    {
        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private IBindable<bool> operationInProgress = null!;

        private readonly RoundedButton button;

        public MultiplayerSpectateButton()
        {
            InternalChild = button = new RoundedButton
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                Enabled = { Value = true },
                Action = onClick
            };
        }

        private void onClick()
        {
            var clickOperation = ongoingOperationTracker.BeginOperation();

            Client.ToggleSpectate().ContinueWith(_ => endOperation());

            void endOperation() => clickOperation?.Dispose();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
            operationInProgress.BindValueChanged(_ => updateState());

            automaticallyDownload = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps);
            automaticallyDownload.BindValueChanged(_ => Scheduler.AddOnce(checkForAutomaticDownload));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentPlaylistItem.BindValueChanged(_ => Scheduler.AddOnce(checkForAutomaticDownload), true);
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            updateState();
        }

        private void updateState()
        {
            switch (Client.LocalUser?.State)
            {
                default:
                    button.Text = "Spectate";
                    button.BackgroundColour = colours.BlueDark;
                    break;

                case MultiplayerUserState.Spectating:
                    button.Text = "Stop spectating";
                    button.BackgroundColour = colours.Gray4;
                    break;
            }

            button.Enabled.Value = Client.Room != null
                                   && Client.Room.State != MultiplayerRoomState.Closed
                                   && !operationInProgress.Value;

            Scheduler.AddOnce(checkForAutomaticDownload);
        }

        #region Automatic download handling

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private Bindable<bool> automaticallyDownload = null!;

        private CancellationTokenSource? downloadCheckCancellation;

        private void checkForAutomaticDownload()
        {
            PlaylistItem? currentItem = CurrentPlaylistItem.Value;

            downloadCheckCancellation?.Cancel();

            if (currentItem == null)
                return;

            if (!automaticallyDownload.Value)
                return;

            // While we can support automatic downloads when not spectating, there are some usability concerns.
            // - In host rotate mode, this could potentially be unwanted by some users (even though they want automatic downloads everywhere else).
            // - When first joining a room, the expectation should be that the user is checking out the room, and they may not immediately want to download the selected beatmap.
            //
            // Rather than over-complicating this flow, let's only auto-download when spectating for the time being.
            // A potential path forward would be to have a local auto-download checkbox above the playlist item list area.
            if (Client.LocalUser?.State != MultiplayerUserState.Spectating)
                return;

            // In a perfect world we'd use BeatmapAvailability, but there's no event-driven flow for when a selection changes.
            // ie. if selection changes from "not downloaded" to another "not downloaded" we wouldn't get a value changed raised.
            beatmapLookupCache
                .GetBeatmapAsync(currentItem.Beatmap.OnlineID, (downloadCheckCancellation = new CancellationTokenSource()).Token)
                .ContinueWith(resolved => Schedule(() =>
                {
                    var beatmapSet = resolved.GetResultSafely()?.BeatmapSet;

                    if (beatmapSet == null)
                        return;

                    if (beatmaps.IsAvailableLocally(new BeatmapSetInfo { OnlineID = beatmapSet.OnlineID }))
                        return;

                    beatmapDownloader.Download(beatmapSet);
                }));
        }

        #endregion
    }
}
