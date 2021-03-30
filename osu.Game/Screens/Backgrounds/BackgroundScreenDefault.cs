// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        private Background background;

        private int currentDisplay;
        private const int background_count = 7;
        private IBindable<User> user;
        private Bindable<Skin> skin;
        private Bindable<BackgroundSource> mode;
        private Bindable<IntroSequence> introSequence;
        private readonly SeasonalBackgroundLoader seasonalBackgroundLoader = new SeasonalBackgroundLoader();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

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
            seasonalBackgroundLoader.SeasonalBackgroundChanged += Next;

            currentDisplay = RNG.Next(0, background_count);

            Next();
        }

        private void display(Background newBackground)
        {
            background?.FadeOut(800, Easing.InOutSine);
            background?.Expire();

            AddInternal(background = newBackground);
            currentDisplay++;
        }

        private ScheduledDelegate nextTask;
        private CancellationTokenSource cancellationTokenSource;

        public void Next()
        {
            nextTask?.Cancel();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            nextTask = Scheduler.AddDelayed(() => LoadComponentAsync(createBackground(), display, cancellationTokenSource.Token), 100);
        }

        private Background createBackground()
        {
            Background newBackground;
            string backgroundName;

            var seasonalBackground = seasonalBackgroundLoader.LoadNextBackground();

            if (seasonalBackground != null)
            {
                seasonalBackground.Depth = currentDisplay;
                return seasonalBackground;
            }

            switch (introSequence.Value)
            {
                case IntroSequence.Welcome:
                    backgroundName = "Intro/Welcome/menu-background";
                    break;

                default:
                    backgroundName = $@"Menu/menu-background-{currentDisplay % background_count + 1}";
                    break;
            }

            if (user.Value?.IsSupporter ?? false)
            {
                switch (mode.Value)
                {
                    case BackgroundSource.Beatmap:
                        newBackground = new BeatmapBackground(beatmap.Value, backgroundName);
                        break;

                    default:
                        newBackground = new SkinnedBackground(skin.Value, backgroundName);
                        break;
                }
            }
            else
                newBackground = new Background(backgroundName);

            newBackground.Depth = currentDisplay;

            return newBackground;
        }

        private class SkinnedBackground : Background
        {
            private readonly Skin skin;

            public SkinnedBackground(Skin skin, string fallbackTextureName)
                : base(fallbackTextureName)
            {
                this.skin = skin;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Sprite.Texture = skin.GetTexture("menu-background") ?? Sprite.Texture;
            }
        }
    }
}
