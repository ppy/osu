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
        /// The OAuth client secret.
        /// </summary>
        public string APIClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// The OAuth client ID.
        /// </summary>
        public string APIClientID { get; set; } = string.Empty;

        /// <summary>
        /// The base URL for the website. Does not include a trailing slash.
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the main (osu-web) API. Does not include a trailing slash.
        /// </summary>
        public string APIUrl { get; set; } = string.Empty;

        /// <summary>
        /// The root URL for the service handling beatmap submission. Does not include a trailing slash.
        /// </summary>
        public string? BeatmapSubmissionServiceUrl { get; set; }

        /// <summary>
        /// The endpoint for the SignalR spectator server.
        /// </summary>
        public string SpectatorUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the SignalR multiplayer server.
        /// </summary>
        public string MultiplayerUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the SignalR metadata server.
        /// </summary>
        public string MetadataUrl { get; set; } = string.Empty;
    }
}
