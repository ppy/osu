// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    /// <summary>
    /// Holds configuration for API endpoints.
    /// </summary>
    public class EndpointConfiguration
    {
        /// <summary>
        /// The base URL for the website.
        /// </summary>
        public string WebsiteRootUrl { get; set; }

        /// <summary>
        /// The endpoint for the main (osu-web) API.
        /// </summary>
        public string APIEndpointUrl { get; set; }

        /// <summary>
        /// The OAuth client secret.
        /// </summary>
        public string APIClientSecret { get; set; }

        /// <summary>
        /// The OAuth client ID.
        /// </summary>
        public string APIClientID { get; set; }

        /// <summary>
        /// The endpoint for the SignalR spectator server.
        /// </summary>
        public string SpectatorEndpointUrl { get; set; }

        /// <summary>
        /// The endpoint for the SignalR multiplayer server.
        /// </summary>
        public string MultiplayerEndpointUrl { get; set; }
    }
}
