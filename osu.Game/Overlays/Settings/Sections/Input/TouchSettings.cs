// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Touch;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class TouchSettings : SettingsSubsection
    {
        private readonly TouchHandler handler;

        public TouchSettings(TouchHandler handler)
        {
            this.handler = handler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = CommonStrings.Enabled,
                    Current = handler.Enabled
                },
            };
        }

        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(new LocalisableString[] { @"touchscreen" });

        protected override LocalisableString Header => handler.Description;
    }
}
