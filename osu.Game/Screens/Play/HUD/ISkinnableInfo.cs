// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.IO.Serialization;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public interface ISkinnableInfo : IJsonSerializable
    {
        public Type Type { get; set; }

        public Type Target { get; set; }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Anchor Anchor { get; set; }
    }

    /// <summary>
    /// A container which supports skinnable components being added to it.
    /// </summary>
    public interface ISkinnableTarget
    {
    }
}
