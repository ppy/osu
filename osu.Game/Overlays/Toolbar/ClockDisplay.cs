// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Toolbar
{
    public abstract partial class ClockDisplay : CompositeDrawable
    {
        private int? lastSecond;

        protected override void Update()
        {
            base.Update();

            var now = DateTimeOffset.Now;

            if (now.Second != lastSecond)
            {
                lastSecond = now.Second;
                UpdateDisplay(now);
            }
        }

        protected abstract void UpdateDisplay(DateTimeOffset now);
    }
}
