// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An event trigger which can be used with <see cref="KeyCounter"/> to create visual tracking of button/key presses.
    /// </summary>
    public abstract partial class InputTrigger : Component
    {
        public event Action<bool>? OnActivate;
        public event Action<bool>? OnDeactivate;

        protected InputTrigger(string name)
        {
            Name = name;
        }

        protected void Activate(bool forwardPlayback = true) => OnActivate?.Invoke(forwardPlayback);

        protected void Deactivate(bool forwardPlayback = true) => OnDeactivate?.Invoke(forwardPlayback);
    }
}
