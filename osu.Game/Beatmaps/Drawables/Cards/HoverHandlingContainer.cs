// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public partial class HoverHandlingContainer : Container
    {
        public Func<HoverEvent, bool>? Hovered { get; set; }
        public Action<HoverLostEvent>? Unhovered { get; set; }

        protected override bool OnHover(HoverEvent e)
        {
            bool handledByBase = base.OnHover(e);
            return Hovered?.Invoke(e) ?? handledByBase;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            Unhovered?.Invoke(e);
        }
    }
}
