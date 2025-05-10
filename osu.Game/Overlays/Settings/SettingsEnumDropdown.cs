// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsEnumDropdown<T> : SettingsDropdown<T>
        where T : struct, Enum
    {
        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(Control.Items.Select(i => i.GetLocalisableDescription()));

        protected override OsuDropdown<T> CreateDropdown() => new DropdownControl();

        protected new partial class DropdownControl : OsuEnumDropdown<T>
        {
            public DropdownControl()
            {
                RelativeSizeAxes = Axes.X;
            }

            protected override DropdownMenu CreateMenu() => base.CreateMenu().With(m => m.MaxHeight = 200);
        }
    }
}
