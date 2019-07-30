// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings.Sections.Graphics;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        private Background background;

        private int currentDisplay;
        private const int background_count = 7;

        private string backgroundName => $@"Menu/menu-background-{currentDisplay % background_count + 1}";

        public readonly Bindable<MainMenuBackgroundMode> BackgroundMode = new Bindable<MainMenuBackgroundMode>();
        public Bindable<User> User = new Bindable<User>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private Bindable<Skin> skin;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinManager skinManager, OsuConfigManager config, Bindable<WorkingBeatmap> workingBeatmap)
        {
            skin = skinManager.CurrentSkin.GetBoundCopy();
            beatmap.BindTo(workingBeatmap);
            User = api.LocalUser.GetBoundCopy();
            config.BindWith(OsuSetting.MenuBackgroundMode, BackgroundMode);

            User.BindValueChanged(onUserChanged);
            skin.BindValueChanged(onSkinChanged);
            beatmap.BindValueChanged(onBeatmapChanged);
            BackgroundMode.BindValueChanged(onModeChanged);

            currentDisplay = RNG.Next(0, background_count);

            display(createBackground());
        }

        private void display(Background newBackground)
        {
            background?.FadeOut(800, Easing.InOutSine);
            background?.Expire();

            AddInternal(background = newBackground);
            currentDisplay++;
        }

        private Texture skinBackground(Skin skin) => skin.GetTexture("menu-background");

        private bool skinHasBackground => skin.Value.GetTexture("menu-background") != null;

        private void onUserChanged(ValueChangedEvent<User> user)
        {
            if (BackgroundMode.Value != MainMenuBackgroundMode.Default && user.OldValue?.IsSupporter != user.NewValue?.IsSupporter &&
                (BackgroundMode.Value != MainMenuBackgroundMode.Skin | skinHasBackground))
                UpdateBackground();
        }

        private void onSkinChanged(ValueChangedEvent<Skin> skin)
        {
            if ((User.Value?.IsSupporter ?? false) && BackgroundMode.Value == MainMenuBackgroundMode.Skin && skinBackground(skin.OldValue) != skinBackground(skin.NewValue))
                UpdateBackground();
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            if (!((User.Value?.IsSupporter ?? true) && BackgroundMode.Value == MainMenuBackgroundMode.Skin && skinHasBackground))
                UpdateBackground();
        }

        private void onModeChanged(ValueChangedEvent<MainMenuBackgroundMode> mode)
        {
            if (((User.Value?.IsSupporter) ?? false)
                && (!(mode.OldValue == MainMenuBackgroundMode.Default && mode.NewValue == MainMenuBackgroundMode.Skin && !skinHasBackground)
                    & !(mode.OldValue == MainMenuBackgroundMode.Skin && mode.NewValue == MainMenuBackgroundMode.Default && !skinHasBackground)))
                UpdateBackground();
        }

        private ScheduledDelegate nextTask;

        protected virtual void UpdateBackground()
        {
            nextTask?.Cancel();
            nextTask = Scheduler.AddDelayed(() => { LoadComponentAsync(createBackground(), display); }, 100);
        }

        private Background createBackground()
        {
            Background newBackground;

            if (User.Value?.IsSupporter ?? false)
            {
                switch (BackgroundMode.Value)
                {
                    default:
                    case MainMenuBackgroundMode.Default:
                        newBackground = new Background(backgroundName);
                        break;

                    case MainMenuBackgroundMode.Skin:
                        newBackground = new SkinnedBackground(skin.Value, backgroundName);
                        break;

                    case MainMenuBackgroundMode.Beatmap:
                        newBackground = new BeatmapBackground(beatmap.Value, backgroundName);
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

        private class BeatmapBackground : Background
        {
            private readonly WorkingBeatmap beatmap;

            public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName)
                : base(fallbackTextureName)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Sprite.Texture = beatmap?.Background ?? Sprite.Texture;
            }
        }
    }
}
