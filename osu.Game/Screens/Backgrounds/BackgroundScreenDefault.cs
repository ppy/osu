// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Skinning;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        private Background background;

        private int currentDisplay;
        private const int background_count = 7;
        private IBindable<APIUser> user;
        private Bindable<Skin> skin;
        private Bindable<BackgroundSource> mode;
        private Bindable<IntroSequence> introSequence;
        private readonly SeasonalBackgroundLoader seasonalBackgroundLoader = new SeasonalBackgroundLoader();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        protected virtual bool AllowStoryboardBackground => true;

        public BackgroundScreenDefault(bool animateOnEnter = true)
            : base(animateOnEnter)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinManager skinManager, OsuConfigManager config)
        {
            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();
            mode = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource);
            introSequence = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence);

            AddInternal(seasonalBackgroundLoader);

            user.ValueChanged += _ => Next();
            skin.ValueChanged += _ => Next();
            mode.ValueChanged += _ => Next();
            beatmap.ValueChanged += _ => Next();
            introSequence.ValueChanged += _ => Next();
            seasonalBackgroundLoader.SeasonalBackgroundChanged += () => Next();

            currentDisplay = RNG.Next(0, background_count);

            Next();
        }

        private ScheduledDelegate nextTask;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Request loading the next background.
        /// </summary>
        /// <returns>Whether a new background was queued for load. May return false if the current background is still valid.</returns>
        public bool Next()
        {
            var nextBackground = createBackground();

            // in the case that the background hasn't changed, we want to avoid cancelling any tasks that could still be loading.
            if (nextBackground == background)
                return false;

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            nextTask?.Cancel();
            nextTask = Scheduler.AddDelayed(() =>
            {
                LoadComponentAsync(nextBackground, displayNext, cancellationTokenSource.Token);
            }, 100);

            return true;
        }

        private void displayNext(Background newBackground)
        {
            background?.FadeOut(800, Easing.InOutSine);
            background?.Expire();

            AddInternal(background = newBackground);
            currentDisplay++;
        }

        private Background createBackground()
        {
            // seasonal background loading gets highest priority.
            Background newBackground = seasonalBackgroundLoader.LoadNextBackground();

            if (newBackground == null && user.Value?.IsSupporter == true)
            {
                switch (mode.Value)
                {
                    case BackgroundSource.Beatmap:
                    case BackgroundSource.BeatmapWithStoryboard:
                    {
                        if (mode.Value == BackgroundSource.BeatmapWithStoryboard && AllowStoryboardBackground)
                            newBackground = new BeatmapBackgroundWithStoryboard(beatmap.Value, getBackgroundTextureName());
                        newBackground ??= new BeatmapBackground(beatmap.Value, getBackgroundTextureName());

                        break;
                    }

                    case BackgroundSource.Skin:
                        // default skins should use the default background rotation, which won't be the case if a SkinBackground is created for them.
                        if (skin.Value is DefaultSkin || skin.Value is DefaultLegacySkin)
                            break;

                        newBackground = new SkinBackground(skin.Value, getBackgroundTextureName());
                        break;
                }
            }

            // this method is called in many cases where the background might not necessarily need to change.
            // if an equivalent background is currently being shown, we don't want to load it again.
            if (newBackground?.Equals(background) == true)
                return background;

            newBackground ??= new Background(getBackgroundTextureName());
            newBackground.Depth = currentDisplay;

            return newBackground;
        }

        private string getBackgroundTextureName()
        {
            switch (introSequence.Value)
            {
                case IntroSequence.Welcome:
                    return @"Intro/Welcome/menu-background";

                default:
                    return $@"Menu/menu-background-{currentDisplay % background_count + 1}";
            }
        }
    }
}
