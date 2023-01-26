// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public abstract partial class OsuSliderBar<T> : SliderBar<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
    }
}
