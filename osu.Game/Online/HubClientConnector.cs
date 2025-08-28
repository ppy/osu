// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using osu.Framework;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public class HubClientConnector : PersistentEndpointClientConnector, IHubClientConnector
    {
        public const string SERVER_SHUTDOWN_MESSAGE = "Server is shutting down.";

        public const string VERSION_HASH_HEADER = @"X-Osu-Version-Hash";
        public const string CLIENT_SESSION_ID_HEADER = @"X-Client-Session-ID";

        /// <summary>
        /// Invoked whenever a new hub connection is built, to configure it before it's started.
        /// </summary>
        public Action<HubConnection>? ConfigureConnection { get; set; }

        private readonly string endpoint;
        private readonly string versionHash;

        /// <summary>
        /// The current connection opened by this connector.
        /// </summary>
        public new HubConnection? CurrentConnection => ((HubClient?)base.CurrentConnection)?.Connection;

        /// <summary>
        /// Constructs a new <see cref="HubClientConnector"/>.
        /// </summary>
        /// <param name="clientName">The name of the client this connector connects for, used for logging.</param>
        /// <param name="endpoint">The endpoint to the hub.</param>
        /// <param name="api"> An API provider used to react to connection state changes.</param>
        /// <param name="versionHash">The hash representing the current game version, used for verification purposes.</param>
        public HubClientConnector(string clientName, string endpoint, IAPIProvider api, string versionHash)
            : base(api)
        {
            ClientName = clientName;
            this.endpoint = endpoint;
            this.versionHash = versionHash;

            // Automatically start these connections.
            Start();
        }

        protected override Task<PersistentEndpointClient> BuildConnectionAsync(CancellationToken cancellationToken)
        {
            var builder = new HubConnectionBuilder()
                .WithUrl(endpoint, options =>
                {
                    // Configuring proxies is not supported on iOS, see https://github.com/xamarin/xamarin-macios/issues/14632.
                    if (RuntimeInfo.OS != RuntimeInfo.Platform.iOS)
                    {
                        // Use HttpClient.DefaultProxy once on net6 everywhere.
                        // The credential setter can also be removed at this point.
                        options.Proxy = WebRequest.DefaultWebProxy;
                        if (options.Proxy != null)
                            options.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    }

                    options.Headers.Add(@"Authorization", @$"Bearer {API.AccessToken}");
                    // non-standard header name kept for backwards compatibility, can be removed after server side has migrated to `VERSION_HASH_HEADER`
                    options.Headers.Add(@"OsuVersionHash", versionHash);
                    options.Headers.Add(VERSION_HASH_HEADER, versionHash);
                    options.Headers.Add(CLIENT_SESSION_ID_HEADER, API.SessionIdentifier.ToString());
                });

            builder.AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS;
            });

            var newConnection = builder.Build();

            ConfigureConnection?.Invoke(newConnection);

            return Task.FromResult((PersistentEndpointClient)new HubClient(newConnection));
        }

        async Task IHubClientConnector.Disconnect()
        {
            await Disconnect().ConfigureAwait(false);
            API.Logout();
        }

        protected override string ClientName { get; }
    }
}
