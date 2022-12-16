// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonOpenInMvis : FooterButton
    {
        private BindableBool OptUIEnabled = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, MConfigManager config)
        {
            Alpha = 0;
            SelectedColour = new Color4(0, 86, 73, 255);
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"在LLin中打开";

            config.BindWith(MSetting.OptUI, OptUIEnabled);
        }

        protected override void LoadComplete()
        {
            OptUIEnabled.BindValueChanged(v =>
            {
                switch(v.NewValue)
                {
                    case true:
                        this.FadeIn(750, Easing.OutQuint);
                        break;

                    case false:
                        this.FadeOut(750, Easing.OutQuint);
                        break;
                }
            }, true);
        }
    }
}
