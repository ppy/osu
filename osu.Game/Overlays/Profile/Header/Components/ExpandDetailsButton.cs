// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ExpandDetailsButton : OsuHoverContainer
    {
        public readonly BindableBool DetailsVisible = new BindableBool();

        public override string TooltipText => DetailsVisible.Value ? "collapse" : "expand";

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private readonly Box background;
        private readonly SpriteIcon icon;

        public ExpandDetailsButton()
        {
            AutoSizeAxes = Axes.X;
            Add(new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(20, 12),
                        Margin = new MarginPadding { Horizontal = 10 }
                    }
                }
            });

            Action = () => DetailsVisible.Toggle();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.GreySeafoamLight;
            HoverColour = colours.GreySeafoamLight.Darken(0.2f);
        }

        protected override void LoadComplete()
        {
            DetailsVisible.BindValueChanged(visible => updateState(visible.NewValue), true);
            base.LoadComplete();
        }

        private void updateState(bool detailsVisible) => icon.Icon = detailsVisible ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
    }
}
