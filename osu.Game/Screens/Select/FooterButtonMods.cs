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
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Input.Bindings;
using osu.Framework.Utils;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
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
            ButtonContentContainer.Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.8f),
                ExpansionMode = ExpansionMode.AlwaysContracted,
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

            if (Precision.DefinitelyBigger(1.0, multiplier) && multiplier >= 0.995)
                MultiplierText.Text = $"{0.99:N2}x";
            else if (Precision.DefinitelyBigger(multiplier, 1.0) && multiplier < 1.005)
                MultiplierText.Text = $"{1.01:N2}x";
            else
                MultiplierText.Text = multiplier.Equals(1.0) ? string.Empty : $"{multiplier:N2}x";

            if (Precision.DefinitelyBigger(multiplier, 1.0))
                MultiplierText.FadeColour(highMultiplierColour, 200);
            else if (Precision.DefinitelyBigger(1.0, multiplier))
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
