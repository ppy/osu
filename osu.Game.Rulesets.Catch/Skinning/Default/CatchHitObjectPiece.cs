// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public abstract class CatchHitObjectPiece : CompositeDrawable
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();

        [Resolved]
        protected IHasCatchObjectState ObjectState { get; private set; }

        /// <summary>
        /// A part of this piece that will be faded out while falling in the playfield.
        /// </summary>
        [CanBeNull]
        protected virtual BorderPiece BorderPiece => null;

        /// <summary>
        /// A part of this piece that will be only visible when <see cref="HyperDash"/> is true.
        /// </summary>
        [CanBeNull]
        protected virtual HyperBorderPiece HyperBorderPiece => null;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindTo(ObjectState.AccentColour);
            HyperDash.BindTo(ObjectState.HyperDash);

            HyperDash.BindValueChanged(hyper =>
            {
                if (HyperBorderPiece != null)
                    HyperBorderPiece.Alpha = hyper.NewValue ? 1 : 0;
            }, true);
        }

        protected override void Update()
        {
            if (BorderPiece != null)
                BorderPiece.Alpha = (float)Math.Clamp((ObjectState.HitObject.StartTime - Time.Current) / 500, 0, 1);
        }
    }
}
