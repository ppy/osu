// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Displays a single-line horizontal auto-sized flow of mods. For cases where wrapping is required, use <see cref="ModFlowDisplay"/> instead.
    /// </summary>
    public class ClickableModDisplay : CompositeDrawable, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        private const int fade_duration = 1000;

        public ExpansionMode ExpansionMode = ExpansionMode.ExpandOnHover;

        private readonly BindableWithCurrent<IReadOnlyList<Mod>> current = new BindableWithCurrent<IReadOnlyList<Mod>>();

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current.Current;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                current.Current = value;
            }
        }

        private readonly FillFlowContainer<ClickableModIcon> iconsContainer;

        public ClickableModDisplay()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = iconsContainer = new ReverseChildIDFillFlowContainer<ClickableModIcon>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(updateDisplay, true);

            iconsContainer.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        private void updateDisplay(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            iconsContainer.Clear();

            if (mods.NewValue == null) return;

            foreach (Mod mod in mods.NewValue)
                iconsContainer.Add(new ClickableModIcon(mod) { Scale = new Vector2(0.6f) });

            appearTransform();
        }

        private void appearTransform()
        {
            expand();

            using (iconsContainer.BeginDelayedSequence(1200))
                contract();
        }

        private void expand()
        {
            if (ExpansionMode != ExpansionMode.AlwaysContracted)
                iconsContainer.TransformSpacingTo(new Vector2(5, 0), 500, Easing.OutQuint);
        }

        private void contract()
        {
            if (ExpansionMode != ExpansionMode.AlwaysExpanded)
                iconsContainer.TransformSpacingTo(new Vector2(-25, 0), 500, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            expand();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            contract();
            base.OnHoverLost(e);
        }
    }
}
