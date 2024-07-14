// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class ComboCounter : RollingCounter<int>, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override double GetProportionalDuration(int currentValue, int newValue)
        {
            return Math.Abs(currentValue - newValue) * RollingDuration * 100.0f;
        }
    }
}
