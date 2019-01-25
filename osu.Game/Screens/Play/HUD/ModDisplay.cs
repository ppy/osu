﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Play.HUD
{
    public class ModDisplay : Container, IHasCurrentValue<IEnumerable<Mod>>
    {
        private const int fade_duration = 1000;

        public bool DisplayUnrankedText = true;

        private readonly Bindable<IEnumerable<Mod>> current = new Bindable<IEnumerable<Mod>>();

        public Bindable<IEnumerable<Mod>> Current
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
        private readonly OsuSpriteText unrankedText;

        public ModDisplay()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                iconsContainer = new ReverseChildIDFillFlowContainer<ModIcon>
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
                    Font = @"Venera",
                    TextSize = 12,
                }
            };

            Current.ValueChanged += mods =>
            {
                iconsContainer.Clear();
                foreach (Mod mod in mods)
                {
                    iconsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.6f) });
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
        }

        private void appearTransform()
        {
            if (DisplayUnrankedText && Current.Value.Any(m => !m.Ranked))
                unrankedText.FadeInFromZero(fade_duration, Easing.OutQuint);
            else
                unrankedText.Hide();

            iconsContainer.FinishTransforms();
            iconsContainer.FadeInFromZero(fade_duration, Easing.OutQuint);
            expand();
            using (iconsContainer.BeginDelayedSequence(1200))
                contract();
        }

        private void expand() => iconsContainer.TransformSpacingTo(new Vector2(5, 0), 500, Easing.OutQuint);

        private void contract() => iconsContainer.TransformSpacingTo(new Vector2(-25, 0), 500, Easing.OutQuint);

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
