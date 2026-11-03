// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which is intended to be serialised to <see cref="SerialisedDrawableInfo"/>.
    /// </summary>
    /// <remarks>
    /// This is currently used exclusively for serialisation to a skin, and leaned on heavily to allow placement and customisation in the skin layout editor.
    /// That said, it is intended to be flexible enough to potentially be used in other places we want to serialise drawables in the future.
    ///
    /// Attaching this interface to any <see cref="IDrawable"/> will make it serialisable via <see cref="SerialisableDrawableExtensions.CreateSerialisedInfo"/>.
    /// Adding <see cref="SettingSourceAttribute"/> annotated bindables will also allow serialising settings automatically.
    /// </remarks>
    public interface ISerialisableDrawable : IDrawable
    {
        /// <summary>
        /// Whether this component can be placed alongside <paramref name="otherComponents"/>.
        /// </summary>
        /// <remarks>
        /// This method allows to implement e.g. skinnable components which can only be placed iff another component isn't already present in the container,
        /// skinnable components which are singletons (only one instance can exist in the container), etc.
        /// </remarks>
        bool CanBePlaced(IEnumerable<ISerialisableDrawable> otherComponents) => true;

        /// <summary>
        /// Whether this component can be selected in the skin editor.
        /// </summary>
        bool CanBeSelected => true;

        /// <summary>
        /// Whether this component can be moved from its default position.
        /// </summary>
        bool CanBeMoved => CanBeSelected;

        /// <summary>
        /// Whether this component can be scaled from its default size.
        /// </summary>
        bool CanBeScaled(Axes axes) => CanBeSelected;

        /// <summary>
        /// Whether this component can be rotated from its default rotation.
        /// </summary>
        bool CanBeRotated => CanBeSelected;

        /// <summary>
        /// In the context of the skin layout editor, whether this <see cref="ISerialisableDrawable"/> has a permanent anchor defined.
        /// If <see langword="false"/>, this <see cref="ISerialisableDrawable"/>'s <see cref="Drawable.Anchor"/> is automatically determined by proximity,
        /// If <see langword="true"/>, a fixed anchor point has been defined.
        /// </summary>
        bool UsesFixedAnchor { get; set; }

        void CopyAdjustedSetting(IBindable target, object source)
        {
            if (source is IBindable sourceBindable)
            {
                // copy including transfer of default values.
                target.BindTo(sourceBindable);
                target.UnbindFrom(sourceBindable);
            }
            else
            {
                if (!(target is IParseable parseable))
                    throw new InvalidOperationException($"Bindable type {target.GetType().ReadableName()} is not {nameof(IParseable)}.");

                parseable.Parse(source, CultureInfo.InvariantCulture);
            }
        }
    }
}
