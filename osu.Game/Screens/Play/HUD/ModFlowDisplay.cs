// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A horizontally wrapping display of mods. For cases where wrapping is not required, use <see cref="ModDisplay"/> instead.
    /// </summary>
    public partial class ModFlowDisplay : ReverseChildIDFillFlowContainer<ModIcon>, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        private const int fade_duration = 1000;

        private readonly BindableWithCurrent<IReadOnlyList<Mod>> current = new BindableWithCurrent<IReadOnlyList<Mod>>();

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current.Current;
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                current.Current = value;
            }
        }

        private float iconScale = 1;

        public float IconScale
        {
            get => iconScale;
            set
            {
                iconScale = value;
                updateDisplay();
            }
        }

        public ModFlowDisplay()
        {
            Direction = FillDirection.Full;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateDisplay(), true);

            this.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        private void updateDisplay()
        {
            Clear();

            if (current.Value == null) return;

            Spacing = new Vector2(0, -12 * iconScale);

            foreach (Mod mod in current.Value.AsOrdered())
            {
                Add(new ModIcon(mod)
                {
                    Scale = new Vector2(0.6f * iconScale),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                });
            }
        }
    }
}
