// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TabletSettings : SettingsSubsection
    {
        private readonly ITabletHandler tabletHandler;

        private readonly BindableSize areaOffset = new BindableSize();
        private readonly BindableSize areaSize = new BindableSize();
        private readonly BindableSize tabletSize = new BindableSize();

        private readonly BindableNumber<int> offsetX = new BindableNumber<int> { MinValue = 0 };
        private readonly BindableNumber<int> offsetY = new BindableNumber<int> { MinValue = 0 };

        private readonly BindableNumber<int> sizeX = new BindableNumber<int> { MinValue = 0 };
        private readonly BindableNumber<int> sizeY = new BindableNumber<int> { MinValue = 0 };

        protected override string Header => "Tablet";

        public TabletSettings(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            areaOffset.BindTo(tabletHandler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                offsetX.Value = val.NewValue.Width;
                offsetY.Value = val.NewValue.Height;
            }, true);

            offsetX.BindValueChanged(val => areaOffset.Value = new Size(val.NewValue, areaOffset.Value.Height));
            offsetY.BindValueChanged(val => areaOffset.Value = new Size(areaOffset.Value.Width, val.NewValue));

            areaSize.BindTo(tabletHandler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                sizeX.Value = val.NewValue.Width;
                sizeY.Value = val.NewValue.Height;
            }, true);

            sizeX.BindValueChanged(val => areaSize.Value = new Size(val.NewValue, areaSize.Value.Height));
            sizeY.BindValueChanged(val => areaSize.Value = new Size(areaSize.Value.Width, val.NewValue));

            ((IBindable<Size>)tabletSize).BindTo(tabletHandler.TabletSize);
            tabletSize.BindValueChanged(val =>
            {
                if (tabletSize.Value == System.Drawing.Size.Empty)
                    return;

                // todo: these should propagate from a TabletChanged event or similar.
                offsetX.MaxValue = val.NewValue.Width;
                sizeX.Default = sizeX.MaxValue = val.NewValue.Width;

                offsetY.MaxValue = val.NewValue.Height;
                sizeY.Default = sizeY.MaxValue = val.NewValue.Height;

                updateDisplay();
            }, true);
        }

        private void updateDisplay()
        {
            if (Children.Count > 0)
                return;

            Children = new Drawable[]
            {
                new SettingsSlider<int>
                {
                    LabelText = "Offset X",
                    Current = offsetX
                },
                new SettingsSlider<int>
                {
                    LabelText = "Offset Y",
                    Current = offsetY
                },
                new SettingsSlider<int>
                {
                    LabelText = "Size X",
                    Current = sizeX
                },
                new SettingsSlider<int>
                {
                    LabelText = "Size Y",
                    Current = sizeY
                },
                new TabletAreaSelection(tabletHandler)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 300,
                }
            };
        }
    }
}
