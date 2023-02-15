// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which can be serialised to a skin, placed and customised via the skin layout editor.
    /// </summary>
    /// <remarks>
    /// Attaching this interface to any <see cref="IDrawable"/> will make it serialisable to user skins (see <see cref="SkinImporter.Save"/>).
    /// Adding <see cref="SettingSourceAttribute"/> annotated bindables will also allow serialising settings automatically.
    ///
    /// Serialisation is done via <see cref="SerialisedDrawableInfo"/> using <see cref="osu.Game.Extensions.DrawableExtensions.CreateSerialisedInfo"/>.
    /// </remarks>
    public interface ISerialisableDrawable : IDrawable
    {
        /// <summary>
        /// Whether this component should be editable by an end user.
        /// </summary>
        bool IsEditable => true;

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

                parseable.Parse(source);
            }
        }
    }
}
