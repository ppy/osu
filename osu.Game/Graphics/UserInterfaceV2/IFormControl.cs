// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// Represents an interface for all form controls.
    /// </summary>
    public interface IFormControl : IDrawable, IHasFilterTerms
    {
        /// <summary>
        /// Caption describing this control, displayed on top of the controls.
        /// </summary>
        LocalisableString Caption { get; set; }

        /// <summary>
        /// Hint text containing an extended description of this control, displayed in a tooltip when hovering the caption.
        /// </summary>
        LocalisableString HintText { get; set; }

        /// <summary>
        /// Invoked when the value of the control has changed.
        /// </summary>
        event Action ValueChanged;

        /// <summary>
        /// Whether the value of this control is in a default state.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// If enabled, resets the control to its default state.
        /// </summary>
        void SetDefault();

        /// <summary>
        /// Whether the control is currently disabled.
        /// </summary>
        bool IsDisabled { get; }

        /// <summary>
        /// The height of the main part of the control (when not expanded).
        /// This is used to attach external elements.
        /// </summary>
        float MainDrawHeight { get; }
    }

    public interface IFormControl<T> : IFormControl, IHasCurrentValue<T>;
}
