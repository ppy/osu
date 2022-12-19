// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Tablet;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    internal partial class RotationPresetButtons : CompositeDrawable
    {
        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        private readonly ITabletHandler tabletHandler;

        private Bindable<float> rotation;
        private readonly RotationButton[] rotationPresets = new RotationButton[preset_count];

        private const int preset_count = 4;
        private const int height = 50;

        public RotationPresetButtons(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;

            RelativeSizeAxes = Axes.X;
            Height = height;

            IEnumerable<Dimension> createColumns(int count)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (i > 0)
                        yield return new Dimension(GridSizeMode.Absolute, 10);

                    yield return new Dimension();
                }
            }

            GridContainer grid;

            InternalChild = grid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = createColumns(preset_count).ToArray()
            };

            grid.Content = new[] { new Drawable[preset_count * 2 - 1] };

            for (int i = 0; i < preset_count; i++)
            {
                int rotationValue = i * 90;

                var rotationPreset = new RotationButton(rotationValue)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 1,
                    Text = $@"{rotationValue}º",
                    Action = () => tabletHandler.Rotation.Value = rotationValue,
                };
                grid.Content[0][2 * i] = rotationPresets[i] = rotationPreset;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rotation = tabletHandler.Rotation.GetBoundCopy();
            rotation.BindValueChanged(val =>
            {
                foreach (var b in rotationPresets)
                    b.IsSelected = b.Preset == val.NewValue;
            }, true);
        }

        public partial class RotationButton : RoundedButton
        {
            [Resolved]
            private OsuColour colours { get; set; }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            public readonly int Preset;

            public RotationButton(int preset)
            {
                Preset = preset;
            }

            private bool isSelected;

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (value == isSelected)
                        return;

                    isSelected = value;

                    if (IsLoaded)
                        updateColour();
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateColour();
            }

            private void updateColour()
            {
                BackgroundColour = isSelected ? colours.Blue3 : colourProvider.Background3;
            }
        }
    }
}
