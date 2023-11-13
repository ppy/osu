// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using osu.Framework;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public class HubClientConnector : PersistentEndpointClientConnector, IHubClientConnector
    {
        public const string SERVER_SHUTDOWN_MESSAGE = "Server is shutting down.";

        /// <summary>
        /// Invoked whenever a new hub connection is built, to configure it before it's started.
        /// </summary>
        public Action<HubConnection>? ConfigureConnection { get; set; }

        private readonly string endpoint;
        private readonly string versionHash;
        private readonly bool preferMessagePack;
        private readonly IAPIProvider api;

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
        /// <param name="preferMessagePack">Whether to use MessagePack for serialisation if available on this platform.</param>
        public HubClientConnector(string clientName, string endpoint, IAPIProvider api, string versionHash, bool preferMessagePack = true)
            : base(api)
        {
            ClientName = clientName;
            this.endpoint = endpoint;
            this.api = api;
            this.versionHash = versionHash;
            this.preferMessagePack = preferMessagePack;

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

                    options.Headers.Add("Authorization", $"Bearer {api.AccessToken}");
                    options.Headers.Add("OsuVersionHash", versionHash);
                });

            if (RuntimeFeature.IsDynamicCodeCompiled && preferMessagePack)
            {
                builder.AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS;
                });
            }
            else
            {
                // eventually we will precompile resolvers for messagepack, but this isn't working currently
                // see https://github.com/neuecc/MessagePack-CSharp/issues/780#issuecomment-768794308.
                builder.AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.PayloadSerializerSettings.Converters = new List<JsonConverter>
                    {
                        new SignalRDerivedTypeWorkaroundJsonConverter(),
                    };
                });
            }

            var newConnection = builder.Build();

            ConfigureConnection?.Invoke(newConnection);

            return Task.FromResult((PersistentEndpointClient)new HubClient(newConnection));
        }

        Task IHubClientConnector.Disconnect() => base.Disconnect();

        protected override string ClientName { get; }
    }
}
