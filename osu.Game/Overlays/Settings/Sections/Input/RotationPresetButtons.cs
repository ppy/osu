// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Tablet;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    internal class RotationPresetButtons : FillFlowContainer
    {
        private readonly ITabletHandler tabletHandler;

        private Bindable<float> rotation;

        private const int height = 50;

        public RotationPresetButtons(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;

            RelativeSizeAxes = Axes.X;
            Height = height;

            for (int i = 0; i < 360; i += 90)
            {
                var presetRotation = i;

                Add(new RotationButton(i)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Width = 0.25f,
                    Text = $"{presetRotation}º",
                    Action = () => tabletHandler.Rotation.Value = presetRotation,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rotation = tabletHandler.Rotation.GetBoundCopy();
            rotation.BindValueChanged(val =>
            {
                foreach (var b in Children.OfType<RotationButton>())
                    b.IsSelected = b.Preset == val.NewValue;
            }, true);
        }

        public class RotationButton : TriangleButton
        {
            [Resolved]
            private OsuColour colours { get; set; }

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
                if (isSelected)
                {
                    BackgroundColour = colours.BlueDark;
                    Triangles.ColourDark = colours.BlueDarker;
                    Triangles.ColourLight = colours.Blue;
                }
                else
                {
                    BackgroundColour = colours.Gray4;
                    Triangles.ColourDark = colours.Gray5;
                    Triangles.ColourLight = colours.Gray6;
                }
            }
        }
    }
}
