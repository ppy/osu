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
        private Bindable<APISeasonalBackgrounds> seasonalBackgrounds;
        private int current;

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics, IAPIProvider api)
        {
            seasonalBackgrounds = sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgrounds);

            if (seasonalBackgrounds.Value != null) return;

            var request = new GetSeasonalBackgroundsRequest();
            request.Success += response =>
            {
                seasonalBackgrounds.Value = response;
                current = RNG.Next(0, response.Backgrounds?.Count ?? 0);
            };

            api.PerformAsync(request);
        }

        public SeasonalBackground LoadBackground()
        {
            var backgrounds = seasonalBackgrounds.Value.Backgrounds;
            if (backgrounds == null || !backgrounds.Any()) return null;

            current = (current + 1) % backgrounds.Count;
            string url = backgrounds[current].Url;

            return new SeasonalBackground(url);
        }

        public bool IsInSeason => seasonalBackgrounds.Value != null && DateTimeOffset.Now < seasonalBackgrounds.Value.EndDate;
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
