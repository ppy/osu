﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Online.Placeholders
{
    public sealed partial class LoginPlaceholder : ClickablePlaceholder
    {
        [Resolved(CanBeNull = true)]
        private LoginOverlay login { get; set; }

        public LoginPlaceholder(LocalisableString actionMessage)
            : base(actionMessage, FontAwesome.Solid.UserLock)
        {
            Action = () => login?.Show();
        }
    }
}
