// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Graphics.Backgrounds
{
    [LongRunningLoad]
    public class SeasonalBackgroundLoader : Component
    {
        private Bindable<APISeasonalBackgrounds> cachedResponse;
        private int current;

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics, IAPIProvider api)
        {
            cachedResponse = sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgroundsResponse);

            if (cachedResponse.Value != null) return;

            var request = new GetSeasonalBackgroundsRequest();
            request.Success += response =>
            {
                cachedResponse.Value = response;
                current = RNG.Next(0, cachedResponse.Value.Backgrounds.Count);
            };

            api.PerformAsync(request);
        }

        public SeasonalBackground LoadBackground()
        {
            var backgrounds = cachedResponse.Value.Backgrounds;
            if (!backgrounds.Any()) return null;

            current = (current + 1) % backgrounds.Count;
            string url = backgrounds[current].Url;

            return new SeasonalBackground(url);
        }

        public bool IsInSeason => DateTimeOffset.Now < cachedResponse.Value.EndDate;
    }

    [LongRunningLoad]
    public class SeasonalBackground : Background
    {
        private readonly string url;
        private const string fallback_texture_name = @"Backgrounds/bg1";

        public SeasonalBackground(string url)
        {
            this.url = url;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Sprite.Texture = textures.Get(url) ?? textures.Get(fallback_texture_name);
        }
    }
}
