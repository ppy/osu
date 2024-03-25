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
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Utils;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => modDisplay.Current;
            set => modDisplay.Current = value;
        }

        protected OsuSpriteText MultiplierText { get; private set; } = null!;
        protected Container UnrankedBadge { get; private set; } = null!;

        private readonly ModDisplay modDisplay;

        private ModSettingChangeTracker? modSettingChangeTracker;

        private Color4 lowMultiplierColour;
        private Color4 highMultiplierColour;

        public FooterButtonMods()
        {
            // must be created in ctor for correct operation of `Current`.
            modDisplay = new ModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.8f),
                ExpansionMode = ExpansionMode.AlwaysContracted,
            };
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

            ButtonContentContainer.AddRange(new Drawable[]
            {
                modDisplay,
                MultiplierText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                },
                UnrankedBadge = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colours.Yellow,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colours.Gray2,
                            Padding = new MarginPadding(5),
                            UseFullGlyphHeight = false,
                            Text = ModSelectOverlayStrings.Unranked.ToLower()
                        }
                    }
                },
            });
        }

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
            MultiplierText.Text = multiplier == 1 ? string.Empty : ModUtils.FormatScoreMultiplier(multiplier);

            if (multiplier > 1)
                MultiplierText.FadeColour(highMultiplierColour, 200);
            else if (multiplier < 1)
                MultiplierText.FadeColour(lowMultiplierColour, 200);
            else
                MultiplierText.FadeColour(Color4.White, 200);

            if (Current.Value?.Count > 0)
                modDisplay.FadeIn();
            else
                modDisplay.FadeOut();

            bool anyUnrankedMods = Current.Value?.Any(m => !m.EligibleForPP) == true;
            UnrankedBadge.FadeTo(anyUnrankedMods ? 1 : 0);
        });
    }
}
