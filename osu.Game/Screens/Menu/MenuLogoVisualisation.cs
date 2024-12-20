// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Seasonal;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    internal partial class MenuLogoVisualisation : LogoVisualisation
    {
        private IBindable<APIUser> user;
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
            if (SeasonalUIConfig.ENABLED)
                Colour = SeasonalUIConfig.AMBIENT_COLOUR_1;
            else if (user.Value?.IsSupporter ?? false)
                Colour = skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White;
            else
                Colour = Color4.White;
        }
    }
}
