// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonReverseArrow : CompositeDrawable
    {
        private Bindable<Color4> accentColour = null!;

        private SpriteIcon icon = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject hitObject)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Size = new Vector2(40, 20),
                    Colour = Color4.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                icon = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.AngleDoubleRight,
                    Size = new Vector2(16),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            accentColour = hitObject.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(accent => icon.Colour = accent.NewValue.Darken(4), true);
        }
    }
}
