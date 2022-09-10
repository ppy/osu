// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Online.Metadata
{
    public class OnlineMetadataClient : MetadataClient
    {
        private readonly string endpoint;

        private IHubClientConnector? connector;

        private Bindable<int> lastQueueId = null!;

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineMetadataClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MetadataEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuConfigManager config)
        {
            // Importantly, we are intentionally not using MessagePack here to correctly support derived class serialization.
            // More information on the limitations / reasoning can be found in osu-server-spectator's initialisation code.
            connector = api.GetHubConnector(nameof(OnlineMetadataClient), endpoint);

            if (connector != null)
            {
                connector.ConfigureConnection = connection =>
                {
                    // this is kind of SILLY
                    // https://github.com/dotnet/aspnetcore/issues/15198
                    connection.On<BeatmapUpdates>(nameof(IMetadataClient.BeatmapSetsUpdated), ((IMetadataClient)this).BeatmapSetsUpdated);
                };

                connector.IsConnected.BindValueChanged(isConnectedChanged, true);
            }

            lastQueueId = config.GetBindable<int>(OsuSetting.LastProcessedMetadataId);
        }

        private bool catchingUp;

        private void isConnectedChanged(ValueChangedEvent<bool> connected)
        {
            if (!connected.NewValue)
                return;

            if (lastQueueId.Value >= 0)
            {
                catchingUp = true;

                Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            Logger.Log($"Requesting catch-up from {lastQueueId.Value}");
                            var catchUpChanges = await GetChangesSince(lastQueueId.Value);

                            lastQueueId.Value = catchUpChanges.LastProcessedQueueID;

                            if (catchUpChanges.BeatmapSetIDs.Length == 0)
                            {
                                Logger.Log($"Catch-up complete at {lastQueueId.Value}");
                                break;
                            }

                            await ProcessChanges(catchUpChanges.BeatmapSetIDs);
                        }
                    }
                    finally
                    {
                        catchingUp = false;
                    }
                });
            }
        }

        public override async Task BeatmapSetsUpdated(BeatmapUpdates updates)
        {
            Logger.Log($"Received beatmap updates {updates.BeatmapSetIDs.Length} updates with last id {updates.LastProcessedQueueID}");

            // If we're still catching up, avoid updating the last ID as it will interfere with catch-up efforts.
            if (!catchingUp)
                lastQueueId.Value = updates.LastProcessedQueueID;

            await ProcessChanges(updates.BeatmapSetIDs);
        }

        public override Task<BeatmapUpdates> GetChangesSince(int queueId)
        {
            if (connector?.IsConnected.Value != true)
                return Task.FromCanceled<BeatmapUpdates>(default);

            Logger.Log($"Requesting any changes since last known queue id {queueId}");

            Debug.Assert(connection != null);

            return connection.InvokeAsync<BeatmapUpdates>(nameof(IMetadataServer.GetChangesSince), queueId);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
