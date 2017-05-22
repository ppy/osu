// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Framework.Input;

namespace osu.Game.Screens.Play.HUD
{
    public class ModDisplay : Container, IHasCurrentValue<IEnumerable<Mod>>
    {
        private readonly Bindable<IEnumerable<Mod>> mods = new Bindable<IEnumerable<Mod>>();

        public Bindable<IEnumerable<Mod>> Current => mods;

        private readonly FillFlowContainer<ModIcon> iconsContainer;

        public ModDisplay()
        {
            Children = new Drawable[]
            {
                iconsContainer = new IconFlow
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Left = 10, Right = 10 },
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"/ UNRANKED /",
                    Font = @"Venera",
                    TextSize = 12,
                }
            };

            mods.ValueChanged += mods =>
            {
                iconsContainer.Clear();
                foreach (Mod mod in mods)
                {
                    iconsContainer.Add(new ModIcon(mod)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.6f),

                    });
                }

                if (IsLoaded)
                    appearTransform();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            appearTransform();
        }

        private void appearTransform()
        {
            iconsContainer.Flush();
            iconsContainer.FadeInFromZero(1000, EasingTypes.OutQuint);
            expand();
            using (iconsContainer.BeginDelayedSequence(1200))
                contract();
        }

        private void expand() => iconsContainer.TransformSpacingTo(new Vector2(5, 0), 500, EasingTypes.OutQuint);

        private void contract() => iconsContainer.TransformSpacingTo(new Vector2(-25, 0), 500, EasingTypes.OutQuint);

        protected override bool OnHover(InputState state)
        {
            expand();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            contract();
            base.OnHoverLost(state);
        }

        private class IconFlow : FillFlowContainer<ModIcon>
        {
            // just reverses the depth of flow contents.
            protected override IComparer<Drawable> DepthComparer => new ReverseCreationOrderDepthComparer();
            protected override IEnumerable<ModIcon> FlowingChildren => base.FlowingChildren.Reverse();
        }
    }
}
