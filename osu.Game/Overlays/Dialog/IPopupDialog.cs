// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Dialog
{
    public interface IPopupDialog : IDrawable
    {
        IEnumerable<PopupDialogButton> Buttons { get; }
        Bindable<Visibility> State { get; }

        /// <summary>
        /// Programmatically clicks the first <see cref="PopupDialogOkButton"/>.
        /// </summary>
        public void PerformOkAction() => PerformAction<PopupDialogOkButton>();

        /// <summary>
        /// Programmatically clicks the first button of the provided type.
        /// </summary>
        public void PerformAction<T>() where T : PopupDialogButton;
    }
}
