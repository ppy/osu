// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public class TapToConfirmContainer : Container
    {
        public Action Action;

        /// <summary>
        /// Whether currently in a fired state (and the confirm <see cref="Action"/> has been sent).
        /// </summary>
        private bool fired { get; set; }

        protected void BeginConfirm()
        {
            if (fired) return;

            Action?.Invoke();
            fired = true;
        }
    }
}
