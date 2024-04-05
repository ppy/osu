// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which can house <see cref="ISerialisableDrawable"/>s.
    /// Contains functionality for new drawables to be added, removed, and reloaded from provided <see cref="SerialisedDrawableInfo"/>.
    /// </summary>
    public interface ISerialisableDrawableContainer : IDrawable
    {
        /// <summary>
        /// The lookup criteria which will be used to retrieve components from the active skin.
        /// </summary>
        ISkinComponentLookup Lookup { get; }

        /// <summary>
        /// A bindable list of components which are being tracked by this skinnable target.
        /// </summary>
        IBindableList<ISerialisableDrawable> Components { get; }

        /// <summary>
        /// Whether all components in this container have been loaded.
        /// </summary>
        bool ComponentsLoaded { get; }

        /// <summary>
        /// Whether the components in this container can be edited by an end user.
        /// </summary>
        bool IsEditable => true;

        /// <summary>
        /// Whether components may be added or removed from this container.
        /// </summary>
        bool ContentsMutable => true;

        /// <summary>
        /// Serialise all children as <see cref="SerialisedDrawableInfo"/>.
        /// </summary>
        /// <returns>The serialised content.</returns>
        IEnumerable<SerialisedDrawableInfo> CreateSerialisedInfo() => Components.Select(d => ((Drawable)d).CreateSerialisedInfo());

        /// <summary>
        /// Reload this target from the current skin.
        /// </summary>
        void Reload();

        /// <summary>
        /// Reload this target from the provided skinnable information.
        /// </summary>
        void Reload(SerialisedDrawableInfo[] skinnableInfo);

        /// <summary>
        /// Add a new skinnable component to this target.
        /// </summary>
        /// <param name="drawable">The component to add.</param>
        void Add(ISerialisableDrawable drawable);

        /// <summary>
        /// Remove an existing skinnable component from this target.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <param name="disposeImmediately">Whether removed items should be immediately disposed.</param>
        void Remove(ISerialisableDrawable component, bool disposeImmediately);
    }
}
