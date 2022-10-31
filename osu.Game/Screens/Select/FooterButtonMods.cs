// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Input.Bindings;

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
        private readonly ModDisplay modDisplay;
        private Color4 lowMultiplierColour;
        private Color4 highMultiplierColour;

        public FooterButtonMods()
        {
            ModsContainer.Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Scale = new Vector2(0.8f),
                ExpansionMode = ExpansionMode.AlwaysContracted,
            });
            ModsContainer.Add(MultiplierText = new OsuSpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.Centre,
                Font = OsuFont.GetFont(weight: FontWeight.Bold),
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            //TODO: use https://fontawesome.com/icons/arrow-right-arrow-left?s=solid&f=classic when local Fontawesome is updated
            IconUsageBox = FontAwesome.Solid.ArrowsAlt;
            ButtonAccentColour = Colour4.FromHex("#B2FF66");
            lowMultiplierColour = colours.Red;
            highMultiplierColour = colours.Green;
            Text = @"Mods";
            Hotkey = GlobalAction.ToggleModSelection;
        }

        [CanBeNull]
        private ModSettingChangeTracker modSettingChangeTracker;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(mods =>
            {
                modSettingChangeTracker?.Dispose();

                updateMultiplierText();

                if (mods.NewValue != null)
                {
                    modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
                    modSettingChangeTracker.SettingChanged += _ => updateMultiplierText();
                }
            }, true);
        }

        private void updateMultiplierText() => Schedule(() =>
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
        });
    }
}
