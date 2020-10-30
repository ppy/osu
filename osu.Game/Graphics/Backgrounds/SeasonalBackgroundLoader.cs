// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        private Bindable<DateTimeOffset> endDate;
        private Bindable<List<APISeasonalBackground>> backgrounds;
        private int current;

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics, IAPIProvider api)
        {
            endDate = sessionStatics.GetBindable<DateTimeOffset>(Static.SeasonEndDate);
            backgrounds = sessionStatics.GetBindable<List<APISeasonalBackground>>(Static.SeasonalBackgrounds);

            if (backgrounds.Value.Any()) return;

            var request = new GetSeasonalBackgroundsRequest();
            request.Success += response =>
            {
                endDate.Value = response.EndDate;
                backgrounds.Value = response.Backgrounds ?? backgrounds.Value;

                current = RNG.Next(0, backgrounds.Value.Count);
            };

            api.PerformAsync(request);
        }

        public SeasonalBackground LoadBackground()
        {
            if (!backgrounds.Value.Any()) return null;

            current = (current + 1) % backgrounds.Value.Count;
            string url = backgrounds.Value[current].Url;

            return new SeasonalBackground(url);
        }

        public bool IsInSeason() => DateTimeOffset.Now < endDate.Value;
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
