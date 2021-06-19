// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class ModDisplay : Container, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        private const int fade_duration = 1000;

        public ExpansionMode ExpansionMode = ExpansionMode.ExpandOnHover;

        private readonly Bindable<IReadOnlyList<Mod>> current = new Bindable<IReadOnlyList<Mod>>();

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                current.UnbindBindings();
                current.BindTo(value);
            }
        }

        private readonly FillFlowContainer<ModIcon> iconsContainer;

        public ModDisplay()
        {
            AutoSizeAxes = Axes.Both;

            Child = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    iconsContainer = new ReverseChildIDFillFlowContainer<ModIcon>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                },
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Current.UnbindAll();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(mods =>
            {
                iconsContainer.Clear();

                if (mods.NewValue != null)
                {
                    foreach (Mod mod in mods.NewValue)
                        iconsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.6f) });

                    appearTransform();
                }
            }, true);

            iconsContainer.FadeInFromZero(fade_duration, Easing.OutQuint);
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

    public enum ExpansionMode
    {
        /// <summary>
        /// The <see cref="ModDisplay"/> will expand only when hovered.
        /// </summary>
        ExpandOnHover,

        /// <summary>
        /// The <see cref="ModDisplay"/> will always be expanded.
        /// </summary>
        AlwaysExpanded,

        /// <summary>
        /// The <see cref="ModDisplay"/> will always be contracted.
        /// </summary>
        AlwaysContracted
    }
}
