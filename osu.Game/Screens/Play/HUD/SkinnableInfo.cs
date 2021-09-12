// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Extensions;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Serialised information governing custom changes to an <see cref="ISkinnableDrawable"/>.
    /// </summary>
    [Serializable]
    public class SkinnableInfo
    {
        public Type Type { get; set; }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Anchor Anchor { get; set; }

        public Anchor Origin { get; set; }

        /// <inheritdoc cref="ISkinnableDrawable.UsesFixedAnchor"/>
        public bool UsesFixedAnchor { get; set; }

        public List<SkinnableInfo> Children { get; } = new List<SkinnableInfo>();

        [JsonConstructor]
        public SkinnableInfo()
        {
        }

        /// <summary>
        /// Construct a new instance populating all attributes from the provided drawable.
        /// </summary>
        /// <param name="component">The drawable which attributes should be sourced from.</param>
        public SkinnableInfo(Drawable component)
        {
            Type = component.GetType();

            Position = component.Position;
            Rotation = component.Rotation;
            Scale = component.Scale;
            Anchor = component.Anchor;
            Origin = component.Origin;

            if (component is ISkinnableDrawable skinnable)
                UsesFixedAnchor = skinnable.UsesFixedAnchor;

            if (component is Container<Drawable> container)
            {
                foreach (var child in container.OfType<ISkinnableDrawable>().OfType<Drawable>())
                    Children.Add(child.CreateSkinnableInfo());
            }
        }

        /// <summary>
        /// Construct an instance of the drawable with all attributes applied.
        /// </summary>
        /// <returns>The new instance.</returns>
        public Drawable CreateInstance()
        {
            Drawable d = (Drawable)Activator.CreateInstance(Type);
            d.ApplySkinnableInfo(this);
            return d;
        }
    }
}
