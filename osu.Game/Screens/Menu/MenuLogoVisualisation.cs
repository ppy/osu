﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Game.Skinning;
using osu.Game.Online.API;
using osu.Game.Users;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Menu
{
    internal class MenuLogoVisualisation : LogoVisualisation
    {
        private Bindable<User> user;
        private Bindable<Skin> skin;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinManager skinManager)
        {
            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            user.ValueChanged += _ => updateColour();
            skin.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            Color4 defaultColour = Color4.White.Opacity(0.2f);

            if (user.Value?.IsSupporter ?? false)
                AccentColour = skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? defaultColour;
            else
                AccentColour = defaultColour;
        }
    }
}
