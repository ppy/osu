// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Skinning.Default;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Represents a <see cref="Fruit"/> caught by the catcher.
    /// </summary>
    public class CaughtFruit : CaughtObject, IHasFruitState
    {
        public Bindable<FruitVisualRepresentation> VisualRepresentation { get; } = new Bindable<FruitVisualRepresentation>();

        public CaughtFruit()
            : base(CatchSkinComponents.Fruit, _ => new FruitPiece())
        {
        }

        public override void CopyStateFrom(IHasCatchObjectState objectState)
        {
            base.CopyStateFrom(objectState);

            var fruitState = (IHasFruitState)objectState;
            VisualRepresentation.Value = fruitState.VisualRepresentation.Value;
        }
    }
}
