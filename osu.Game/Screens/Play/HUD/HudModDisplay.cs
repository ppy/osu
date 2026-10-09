// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class HudModDisplay : ModDisplay
    {
        private readonly Bindable<bool> replayLoaded = new Bindable<bool>();

        public HudModDisplay(Bindable<bool> replayLoaded, bool showExtendedInformation = true)
            : base(showExtendedInformation)
        {
            this.replayLoaded.BindTo(replayLoaded);
        }

        protected override ModIcon CreateModIcon(Mod mod, bool showExtendedInformation)
        {
            var modIcon = new ClickableModIcon(mod, showExtendedInformation);

            if (modIcon.Mod is IAdjustableWhenReplay dmod)
            {
                dmod.IsDisabled.BindValueChanged(_ => modIcon.Colour = dmod.IsDisabled.Value ? OsuColour.Gray(0.7f) : Color4.White, true);

                modIcon.Action = () =>
                {
                    if (!replayLoaded.Value)
                        return;

                    dmod.IsDisabled.Toggle();
                };
            }

            return modIcon;
        }
    }
}
