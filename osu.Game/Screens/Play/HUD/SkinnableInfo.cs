// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.IO.Serialization;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Serialised information governing custom changes to an <see cref="ISkinnableComponent"/>.
    /// </summary>
    [Serializable]
    public class SkinnableInfo : IJsonSerializable
    {
        public SkinnableInfo()
        {
        }

        public SkinnableInfo(Drawable component)
        {
            Type = component.GetType();

            ISkinnableTarget target = component.Parent as ISkinnableTarget;

            Target = target?.Target;

            Position = component.Position;
            Rotation = component.Rotation;
            Scale = component.Scale;
            Anchor = component.Anchor;
        }

        public Type Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SkinnableTarget? Target { get; set; }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Anchor Anchor { get; set; }

        public Drawable CreateInstance()
        {
            Drawable d = (Drawable)Activator.CreateInstance(Type);
            d.ApplySerialisedInformation(this);
            return d;
        }
    }
}
