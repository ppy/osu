// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
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

        protected readonly OsuSpriteText MultiplierText;
        private readonly FooterModDisplay modDisplay;
        private Color4 lowMultiplierColour;
        private Color4 highMultiplierColour;

        public FooterButtonMods()
        {
            ButtonContentContainer.Add(modDisplay = new FooterModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                DisplayUnrankedText = false,
                Scale = new Vector2(0.8f)
            });
            ButtonContentContainer.Add(MultiplierText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.GetFont(weight: FontWeight.Bold),
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            lowMultiplierColour = colours.Red;
            highMultiplierColour = colours.Green;
            Text = @"mods";
            Hotkey = Key.F1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateMultiplierText(), true);
        }

        private void updateMultiplierText()
        {
            double multiplier = Current.Value?.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier) ?? 1;

            MultiplierText.Text = multiplier.Equals(1.0) ? string.Empty : $"{multiplier:N2}x";

            if (multiplier > 1.0)
                MultiplierText.FadeColour(highMultiplierColour, 200);
            else if (multiplier < 1.0)
                MultiplierText.FadeColour(lowMultiplierColour, 200);
            else
                MultiplierText.FadeColour(Color4.White, 200);

            if (Current.Value?.Count > 0)
                modDisplay.FadeIn();
            else
                modDisplay.FadeOut();
        }

        private class FooterModDisplay : ModDisplay
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;

            public FooterModDisplay()
            {
                ExpansionMode = ExpansionMode.AlwaysContracted;
                IconsContainer.Margin = new MarginPadding();
            }
        }
    }
}
