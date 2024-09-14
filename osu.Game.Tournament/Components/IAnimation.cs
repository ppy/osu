// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Tournament.Components
{
    public interface IAnimation : IDrawable
    {
        public event Action? OnAnimationComplete;

        public void Fire();

        public AnimationStatus Status { get; }
    }

    public enum AnimationStatus
    {
        Loading,
        Running,
        Complete
    }
}
