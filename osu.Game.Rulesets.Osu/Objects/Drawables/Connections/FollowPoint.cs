// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPoint : Container
    {
        private const float width = 8;

        public override bool RemoveWhenNotAlive => false;

        public FollowPoint()
        {
            Origin = Anchor.Centre;

            Child = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.FollowPoint), _ => new Container
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                CornerRadius = width / 2,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Color4.White.Opacity(0.2f),
                    Radius = 4,
                },
                Child = new Box
                {
                    Size = new Vector2(width),
                    Blending = BlendingParameters.Additive,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0.5f,
                }
            }, confineMode: ConfineMode.NoScaling);
        }
    }
}
