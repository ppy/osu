// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Performance;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class CaughtObjectEntry : LifetimeEntry, IHasFruitState
    {
        /// <summary>
        /// Whether this caught object is stacked or dropped.
        /// </summary>
        public readonly CaughtObjectState State;

        /// <summary>
        /// The position of this object in relative to the catcher.
        /// </summary>
        public readonly Vector2 PositionInStack;

        public CatchHitObject HitObject { get; }

        public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>();

        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();

        public Vector2 DisplaySize { get; }

        public float DisplayRotation { get; }

        public Bindable<FruitVisualRepresentation> VisualRepresentation { get; } = new Bindable<FruitVisualRepresentation>();

        /// <summary>
        /// The action invoked to drawable representation of this caught object when dropped.
        /// </summary>
        public Action<DrawableCaughtObject> ApplyTransforms;

        /// <summary>
        /// The initial position of the object when dropped.
        /// </summary>
        public Vector2 DropPosition;

        /// <summary>
        /// 1 or -1 representing visual mirroring of the caught object.
        /// </summary>
        public int MirrorDirection = 1;

        public CaughtObjectEntry(CaughtObjectState state, Vector2 positionInStack, IHasCatchObjectState source)
        {
            State = state;
            PositionInStack = positionInStack;

            HitObject = source.HitObject;
            AccentColour.Value = source.AccentColour.Value;
            HyperDash.Value = source.HyperDash.Value;
            DisplaySize = source.DisplaySize;
            DisplayRotation = source.DisplayRotation;

            if (source is IHasFruitState fruit)
                VisualRepresentation.Value = fruit.VisualRepresentation.Value;
        }
    }

    public enum CaughtObjectState
    {
        Stacked,
        Dropped
    }
}
