// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public abstract class PulpFormation : CompositeDrawable
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        [Resolved(canBeNull: true)]
        private DrawablePool<Pulp> pulpPool { get; set; }

        protected const float LARGE_PULP_3 = 16f * FruitPiece.RADIUS_ADJUST;
        protected const float DISTANCE_FROM_CENTRE_3 = 0.15f;

        protected const float LARGE_PULP_4 = LARGE_PULP_3 * 0.925f;
        protected const float DISTANCE_FROM_CENTRE_4 = DISTANCE_FROM_CENTRE_3 / 0.925f;

        protected const float SMALL_PULP = LARGE_PULP_3 / 2;

        protected PulpFormation()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected static Vector2 PositionAt(float angle, float distance) => new Vector2(
            distance * MathF.Sin(angle * MathF.PI / 180),
            distance * MathF.Cos(angle * MathF.PI / 180));

        protected void Clear() => ClearInternal(false);

        protected void Add(Vector2 position, Vector2 size)
        {
            var pulp = pulpPool?.Get() ?? new Pulp();
            pulp.Position = position;
            pulp.Size = size;
            pulp.AccentColour.BindTo(AccentColour);
            AddInternal(pulp);
        }
    }
}
