// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Graphics.Backgrounds
{
    [LongRunningLoad]
    public class SeasonalBackgroundLoader : Component
    {
        private List<APISeasonalBackground> backgrounds = new List<APISeasonalBackground>();
        private int current;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            var request = new GetSeasonalBackgroundsRequest();
            request.Success += response =>
            {
                backgrounds = response.Backgrounds ?? backgrounds;
                current = RNG.Next(0, backgrounds.Count);
            };

            api.PerformAsync(request);
        }

        public SeasonalBackground LoadBackground(string fallbackTextureName)
        {
            string url = null;

            if (backgrounds.Any())
            {
                current = (current + 1) % backgrounds.Count;
                url = backgrounds[current].Url;
            }

            return new SeasonalBackground(url, fallbackTextureName);
        }
    }

    [LongRunningLoad]
    public class SeasonalBackground : Background
    {
        private readonly string url;
        private readonly string fallbackTextureName;

        public SeasonalBackground([CanBeNull] string url, string fallbackTextureName = @"Backgrounds/bg1")
        {
            this.url = url;
            this.fallbackTextureName = fallbackTextureName;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Sprite.Texture = textures.Get(url) ?? textures.Get(fallbackTextureName);
        }
    }
}
