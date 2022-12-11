// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Displays a single-line horizontal auto-sized flow of mods. For cases where wrapping is required, use <see cref="ModFlowDisplay"/> instead.
    /// </summary>
    public partial class ClickableModDisplay : ModDisplay
    {
        private readonly Bindable<bool> replayLoaded = new Bindable<bool>();

        private readonly FillFlowContainer<ClickableModIcon> iconsContainer;

        public ClickableModDisplay(Bindable<bool> replayLoaded)
        {
            AutoSizeAxes = Axes.Both;
            this.replayLoaded.BindTo(replayLoaded);

            InternalChild = iconsContainer = new ReverseChildIDFillFlowContainer<ClickableModIcon>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
            };
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(updateDisplay, true);

            iconsContainer.FadeInFromZero(FADE_DURATION, Easing.OutQuint);
        }

        private void updateDisplay(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            iconsContainer.Clear();

            if (mods.NewValue == null) return;

            foreach (Mod mod in mods.NewValue)
                iconsContainer.Add(new ClickableModIcon(mod, replayLoaded) { Scale = new Vector2(0.6f) });

            AppearTransform();
        }
    }
}
