// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class ModDisplay : Container, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        private const int fade_duration = 1000;

        public bool DisplayUnrankedText = true;

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

        protected readonly FillFlowContainer<ModIcon> IconsContainer;
        private readonly OsuSpriteText unrankedText;

        public ModDisplay()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                IconsContainer = new ReverseChildIDFillFlowContainer<ModIcon>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Left = 10, Right = 10 },
                },
                unrankedText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"/ UNRANKED /",
                    Font = OsuFont.Numeric.With(size: 12)
                }
            };

            Current.ValueChanged += mods =>
            {
                IconsContainer.Clear();

                foreach (Mod mod in mods.NewValue)
                {
                    IconsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.6f) });
                }

                if (IsLoaded)
                    appearTransform();
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

            appearTransform();
            IconsContainer.FadeInFromZero(fade_duration, Easing.OutQuint);
        }

        private void appearTransform()
        {
            if (DisplayUnrankedText && Current.Value.Any(m => !m.Ranked))
                unrankedText.FadeInFromZero(fade_duration, Easing.OutQuint);
            else
                unrankedText.Hide();

            expand();

            using (IconsContainer.BeginDelayedSequence(1200))
                contract();
        }

        private void expand()
        {
            if (ExpansionMode != ExpansionMode.AlwaysContracted)
                IconsContainer.TransformSpacingTo(new Vector2(5, 0), 500, Easing.OutQuint);
        }

        private void contract()
        {
            if (ExpansionMode != ExpansionMode.AlwaysExpanded)
                IconsContainer.TransformSpacingTo(new Vector2(-25, 0), 500, Easing.OutQuint);
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
