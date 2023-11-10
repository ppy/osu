// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Serialised backing data for <see cref="ISerialisableDrawable"/>s.
    /// Used for json serialisation in user skins.
    /// </summary>
    /// <remarks>
    /// Can be created using <see cref="SerialisableDrawableExtensions.CreateSerialisedInfo"/>.
    /// Can also be applied to an existing drawable using <see cref="SerialisableDrawableExtensions.ApplySerialisedInfo"/>.
    /// </remarks>
    [Serializable]
    public sealed class SerialisedDrawableInfo
    {
        public Type Type { get; set; } = null!;

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Vector2 Size { get; set; }

        public Anchor Anchor { get; set; }

        public Anchor Origin { get; set; }

        /// <inheritdoc cref="ISerialisableDrawable.UsesFixedAnchor"/>
        public bool UsesFixedAnchor { get; set; }

        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        public List<SerialisedDrawableInfo> Children { get; } = new List<SerialisedDrawableInfo>();

        [JsonConstructor]
        public SerialisedDrawableInfo()
        {
        }

        /// <summary>
        /// Construct a new instance populating all attributes from the provided drawable.
        /// </summary>
        /// <param name="component">The drawable which attributes should be sourced from.</param>
        public SerialisedDrawableInfo(Drawable component)
        {
            Type = component.GetType();

            Position = component.Position;
            Rotation = component.Rotation;
            Scale = component.Scale;
            Size = component.Size;
            Anchor = component.Anchor;
            Origin = component.Origin;

            if (component is ISerialisableDrawable serialisableDrawable)
                UsesFixedAnchor = serialisableDrawable.UsesFixedAnchor;

            foreach (var (_, property) in component.GetSettingsSourceProperties())
            {
                var bindable = (IBindable)property.GetValue(component)!;

                Settings.Add(property.Name.ToSnakeCase(), bindable.GetUnderlyingSettingValue());
            }

            if (component is Container<Drawable> container)
            {
                foreach (var child in container.OfType<ISerialisableDrawable>().OfType<Drawable>())
                    Children.Add(child.CreateSerialisedInfo());
            }
        }

        /// <summary>
        /// Construct an instance of the drawable with all attributes applied.
        /// </summary>
        /// <returns>The new instance.</returns>
        public Drawable CreateInstance()
        {
            try
            {
                Drawable d = (Drawable)Activator.CreateInstance(Type)!;
                d.ApplySerialisedInfo(this);
                return d;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Unable to create skin component {Type.Name}");
                return Drawable.Empty();
            }
        }

        /// <summary>
        /// Retrieve all types available which support serialisation.
        /// </summary>
        /// <param name="ruleset">The ruleset to filter results to. If <c>null</c>, global components will be returned instead.</param>
        public static Type[] GetAllAvailableDrawables(RulesetInfo? ruleset = null)
        {
            return (ruleset?.CreateInstance().GetType() ?? typeof(OsuGame))
                   .Assembly.GetTypes()
                   .Where(t => !t.IsInterface && !t.IsAbstract && t.IsPublic)
                   .Where(t => typeof(ISerialisableDrawable).IsAssignableFrom(t))
                   .OrderBy(t => t.Name)
                   .ToArray();
        }
    }
}
