// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class FooterButtonMods : FooterButton
    {
        public FooterButtonMods(Bindable<IReadOnlyList<Mod>> mods)
        {
            FooterModDisplay modDisplay;

            Add(new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Child = modDisplay = new FooterModDisplay
                {
                    DisplayUnrankedText = false,
                    Scale = new Vector2(0.8f)
                },
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Left = 70 }
            });

            if (mods != null)
                modDisplay.Current = mods;
        }

        private class FooterModDisplay : ModDisplay
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;
        }
    }
}
