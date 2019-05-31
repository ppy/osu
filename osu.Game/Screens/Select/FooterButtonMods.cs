// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;
using osu.Framework.MathUtils;
using System.Linq;

namespace osu.Game.Screens.Select
{
    public class FooterButtonMods : FooterButton
    {
        private readonly ModDisplay modDisplay;

        public FooterButtonMods(Bindable<IReadOnlyList<Mod>> mods)
        {
            Add(modDisplay = new FooterModDisplay
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Left = 100 },
                DisplayUnrankedText = false,
                Scale = new Vector2(0.8f)
            });

            if (mods != null)
                modDisplay.Current = mods;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"mods";
            Hotkey = Key.F1;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float finalWidth = !modDisplay.Current.Value.Any() ? DEFAULT_SIZE.X : DEFAULT_SIZE.X + modDisplay.IconsContainer.Width;
            Width = Interpolation.ValueAt(MathHelper.Clamp(Clock.ElapsedFrameTime, 0, 1000), Width, finalWidth, 0, 500, Easing.OutQuint);
        }

        private class FooterModDisplay : ModDisplay
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;

            public FooterModDisplay()
            {
                AllowExpand = false;
            }
        }
    }
}
