// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a container which can house <see cref="ISkinnableDrawable"/>s.
    /// </summary>
    public interface ISkinnableTarget : IDrawable
    {
        /// <summary>
        /// The definition of this target.
        /// </summary>
        SkinnableTarget Target { get; }

        /// <summary>
        /// A bindable list of components which are being tracked by this skinnable target.
        /// </summary>
        IBindableList<ISkinnableDrawable> Components { get; }

        /// <summary>
        /// Serialise all children as <see cref="SkinnableInfo"/>.
        /// </summary>
        /// <returns>The serialised content.</returns>
        IEnumerable<SkinnableInfo> CreateSkinnableInfo() => Components.Select(d => ((Drawable)d).CreateSkinnableInfo());

        /// <summary>
        /// Reload this target from the current skin.
        /// </summary>
        void Reload();

        /// <summary>
        /// Add a new skinnable component to this target.
        /// </summary>
        /// <param name="drawable">The component to add.</param>
        void Add(ISkinnableDrawable drawable);

        /// <summary>
        /// Remove an existing skinnable component from this target.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        public void Remove(ISkinnableDrawable component);
    }
}
