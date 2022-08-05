// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that grows in size to fit its child and retains its size when its child shrinks
    /// </summary>
    public class GrowToFitContainer : Container
    {
        protected override void Update()
        {
            base.Update();
            Height = Math.Max(Child.Height, Height);
            Width = Math.Max(Child.Width, Width);
        }
    }

}
