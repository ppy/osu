// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class FooterButtonMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => modDisplay.Current;
            set => modDisplay.Current = value;
        }

        private readonly FooterModDisplay modDisplay;

        public FooterButtonMods()
        {
            Add(new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Shear = -SHEAR,
                Child = modDisplay = new FooterModDisplay
                {
                    DisplayUnrankedText = false,
                    Scale = new Vector2(0.8f)
                },
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Left = 70 }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"mods";
            Hotkey = Key.F1;
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
