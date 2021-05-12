// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a container which can house <see cref="ISkinnableComponent"/>s.
    /// </summary>
    public interface ISkinnableTarget
    {
        /// <summary>
        /// The definition of this target.
        /// </summary>
        SkinnableTarget Target { get; }

        /// <summary>
        /// A bindable list of components which are being tracked by this skinnable target.
        /// </summary>
        IBindableList<ISkinnableComponent> Components { get; }

        /// <summary>
        /// Reload this target from the current skin.
        /// </summary>
        void Reload();

        /// <summary>
        /// Add the provided item to this target.
        /// </summary>
        void Add(ISkinnableComponent drawable);
    }
}
