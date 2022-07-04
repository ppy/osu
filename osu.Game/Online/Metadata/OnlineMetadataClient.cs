// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online.Metadata
{
    public class OnlineMetadataClient : MetadataClient
    {
        private readonly string endpoint;

        private IHubClientConnector? connector;

        private HubConnection? connection => connector?.CurrentConnection;

        public OnlineMetadataClient(EndpointConfiguration endpoints)
        {
            endpoint = endpoints.MetadataEndpointUrl;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
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
            }
        }

        public override Task BeatmapSetsUpdated(BeatmapUpdates updates)
        {
            Logger.Log($"Received beatmap updates {updates.BeatmapSetIDs.Length} updates with last id {updates.LastProcessedQueueID}");
            return Task.CompletedTask;
        }

        public override Task<BeatmapUpdates> GetChangesSince(uint queueId)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            connector?.Dispose();
        }
    }
}
