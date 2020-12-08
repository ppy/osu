// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public interface IHasCatchObjectState
    {
        PalpableCatchHitObject HitObject { get; }
        Bindable<Color4> AccentColour { get; }
        Bindable<bool> HyperDash { get; }

        float Rotation { get; }
        Vector2 Scale { get; }
    }

    public interface IHasFruitState : IHasCatchObjectState
    {
        Bindable<FruitVisualRepresentation> VisualRepresentation { get; }
    }
}
