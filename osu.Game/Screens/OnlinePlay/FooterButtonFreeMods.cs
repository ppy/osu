// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public class FooterButtonFreeMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => modDisplay.Current;
            set => modDisplay.Current = value;
        }

        private readonly ModDisplay modDisplay;

        public FooterButtonFreeMods()
        {
            ButtonContentContainer.Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                DisplayUnrankedText = false,
                Scale = new Vector2(0.8f),
                ExpansionMode = ExpansionMode.AlwaysContracted,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"freemods";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateModDisplay(), true);
        }

        private void updateModDisplay()
        {
            if (Current.Value?.Count > 0)
                modDisplay.FadeIn();
            else
                modDisplay.FadeOut();
        }
    }
}
