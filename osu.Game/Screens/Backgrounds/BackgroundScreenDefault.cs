﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Threading;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BlurrableBackgroundScreen
    {
        private int currentDisplay;
        private const int background_count = 5;

        private string backgroundName => $@"Menu/menu-background-{currentDisplay % background_count + 1}";

        private Bindable<User> user;
        private Bindable<Skin> skin;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinManager skinManager)
        {
            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            user.ValueChanged += _ => Next();
            skin.ValueChanged += _ => Next();

            currentDisplay = RNG.Next(0, background_count);

            Next();
        }

        private void display(Background newBackground)
        {
            Background?.FadeOut(800, Easing.InOutSine);
            Background?.Expire();

            AddInternal(Background = newBackground);
            currentDisplay++;
        }

        private ScheduledDelegate nextTask;

        public void Next()
        {
            nextTask?.Cancel();
            nextTask = Scheduler.AddDelayed(() =>
            {
                Background background;

                if (user.Value?.IsSupporter ?? false)
                    background = new SkinnedBackground(skin.Value, backgroundName);
                else
                    background = new Background(backgroundName);

                background.Depth = currentDisplay;

                LoadComponentAsync(background, display);
            }, 100);
        }

        private class SkinnedBackground : Background
        {
            private readonly Skin skin;

            public SkinnedBackground(Skin skin, string fallbackTextureName) : base(fallbackTextureName)
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
