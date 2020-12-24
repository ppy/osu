// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class ProductionEndpointConfiguration : EndpointConfiguration
    {
        public ProductionEndpointConfiguration()
        {
            APIEndpoint = @"https://osu.ppy.sh";
            APIClientSecret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";
            APIClientID = "5";
            SpectatorEndpoint = "https://spectator.ppy.sh/spectator";
            MultiplayerEndpoint = "https://spectator.ppy.sh/multiplayer";
        }
    }
}
