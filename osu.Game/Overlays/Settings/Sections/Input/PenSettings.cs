// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Pen;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class PenSettings : InputSubsection
    {
        private readonly PenHandler penHandler;

        protected override LocalisableString Header => PenSettingsStrings.TabletExternal;

        private Bindable<double> handlerSensitivity = null!;

        public PenSettings(PenHandler penHandler)
            : base(penHandler)
        {
            this.penHandler = penHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            handlerSensitivity = penHandler.Sensitivity.GetBoundCopy();

            AddRange(new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = PenSettingsStrings.PenSensitivity,
                    Current = handlerSensitivity,
                    KeyboardStep = 0.01f,
                    TransferValueOnCommit = true,
                    LabelFormat = v => $@"{v:0.##}x",
                    TooltipFormat = v => $@"{v:0.##}x",
                })
                {
                    Keywords = new[] { "speed", "velocity" },
                }
            });
        }
    }
}
