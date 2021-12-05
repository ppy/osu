// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class HoverHandlingContainer : Container
    {
        public Func<HoverEvent, bool>? Hovered { get; set; }
        public Action<HoverLostEvent>? Unhovered { get; set; }

        protected override bool OnHover(HoverEvent e) => Hovered?.Invoke(e) ?? base.OnHover(e);

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (Unhovered != null)
                Unhovered?.Invoke(e);
            else
                base.OnHoverLost(e);
        }
    }
}
