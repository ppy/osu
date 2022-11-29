// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class SeasonalBackgroundLoader : Component
    {
        /// <summary>
        /// Fired when background should be changed due to receiving backgrounds from API
        /// or when the user setting is changed (as it might require unloading the seasonal background).
        /// </summary>
        public event Action SeasonalBackgroundChanged;

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();
        private Bindable<SeasonalBackgroundMode> seasonalBackgroundMode;
        private Bindable<APISeasonalBackgrounds> seasonalBackgrounds;

        private int current;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics sessionStatics)
        {
            seasonalBackgroundMode = config.GetBindable<SeasonalBackgroundMode>(OsuSetting.SeasonalBackgroundMode);
            seasonalBackgroundMode.BindValueChanged(_ => SeasonalBackgroundChanged?.Invoke());

            seasonalBackgrounds = sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgrounds);
            seasonalBackgrounds.BindValueChanged(_ =>
            {
                if (shouldShowSeasonal)
                    SeasonalBackgroundChanged?.Invoke();
            });

            apiState.BindTo(api.State);
            apiState.BindValueChanged(fetchSeasonalBackgrounds, true);
        }

        private void fetchSeasonalBackgrounds(ValueChangedEvent<APIState> stateChanged)
        {
            if (seasonalBackgrounds.Value != null || stateChanged.NewValue != APIState.Online)
                return;

            var request = new GetSeasonalBackgroundsRequest();
            request.Success += response =>
            {
                seasonalBackgrounds.Value = response;
                current = RNG.Next(0, response.Backgrounds?.Count ?? 0);
            };

            api.PerformAsync(request);
        }

        public SeasonalBackground LoadNextBackground()
        {
            if (!shouldShowSeasonal)
                return null;

            var backgrounds = seasonalBackgrounds.Value.Backgrounds;

            current = (current + 1) % backgrounds.Count;
            string url = backgrounds[current].Url;

            return new SeasonalBackground(url);
        }

        private bool shouldShowSeasonal
        {
            get
            {
                if (seasonalBackgroundMode.Value == SeasonalBackgroundMode.Never)
                    return false;

                if (seasonalBackgroundMode.Value == SeasonalBackgroundMode.Sometimes && !isInSeason)
                    return false;

                return seasonalBackgrounds.Value?.Backgrounds?.Any() == true;
            }
        }

        private bool isInSeason => seasonalBackgrounds.Value != null && DateTimeOffset.Now < seasonalBackgrounds.Value.EndDate;
    }

    [LongRunningLoad]
    public partial class SeasonalBackground : Background
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

        public override bool Equals(Background other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((SeasonalBackground)other).url == url;
        }
    }
}
