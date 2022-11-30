﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsButton : RoundedButton, IHasTooltip, IConditionalFilterable
    {
        public SettingsButton()
        {
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS };
        }

        public LocalisableString TooltipText { get; set; }

        public BindableBool CanBeShown { get; } = new BindableBool(true);
        IBindable<bool> IConditionalFilterable.CanBeShown => CanBeShown;

        public override IEnumerable<LocalisableString> FilterTerms
        {
            get
            {
                if (TooltipText != default)
                    return base.FilterTerms.Append(TooltipText);

                return base.FilterTerms;
            }
        }
    }
}
