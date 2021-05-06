// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Serialised information governing custom changes to an <see cref="ISkinnableComponent"/>.
    /// </summary>
    public class StoredSkinnableInfo : ISkinnableInfo
    {
        public StoredSkinnableInfo(Drawable component)
        {
            Type = component.GetType();

            var target = component.Parent as ISkinnableTarget
                         // todo: this is temporary until we serialise the default layouts out of SkinnableDrawables.
                         ?? component.Parent?.Parent as ISkinnableTarget;

            Target = target?.GetType();

            Position = component.Position;
            Rotation = component.Rotation;
            Scale = component.Scale;
            Anchor = component.Anchor;
        }

        public Type Type { get; set; }

        public Type Target { get; set; }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Anchor Anchor { get; set; }
    }
}
