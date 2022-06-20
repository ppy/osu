// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings
{
    public class SettingsButton : RoundedButton, IHasTooltip
    {
        public SettingsButton()
        {
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS };
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OverlayColourProvider overlayColourProvider, OsuColour colours)
        {
            DefaultBackgroundColour = overlayColourProvider?.Highlight1 ?? colours.Blue3;
        }

        public LocalisableString TooltipText { get; set; }

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
