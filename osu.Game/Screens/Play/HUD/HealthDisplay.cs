// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.HUD
{
    public abstract class HealthDisplay : Container
    {
        public readonly BindableDouble Current = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1
        };

        protected HealthDisplay()
        {
            Current.ValueChanged += newValue => SetHealth((float)newValue);
        }

        protected abstract void SetHealth(float value);
    }
}
