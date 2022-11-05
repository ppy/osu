// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableContainer : DrawableList
    {
        private readonly OsuCheckbox button;
        private readonly Box box;
        private readonly Container<Drawable> gridContainer;
        private readonly BindableBool enabled = new BindableBool(true);

        public DrawableContainer()
        {
            gridContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    box = new Box
                    {
                        Colour = new Colour4(255, 255, 0, 0.25f),
                    },
                    button = new OsuCheckbox
                    {
                        LabelText = @"SkinnableContainer",
                        Current = enabled
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 10),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                null,
                                Container
                            }
                        }
                    }
                }
            };
            enabled.BindValueChanged(v => SetShown(v.NewValue), true);
        }

        public void Toggle() => SetShown(!enabled.Value, true);

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) Show();
            else Hide();

            if (setValue) enabled.Value = value;
        }

        public override void Select(bool value)
        {
            if (value)
            {
                box.Show();
                box.Width = button.Width;
                box.Height = button.Height;
            }
            else
            {
                box.Hide();
                box.Width = button.Width;
                box.Height = button.Height;
            }

            base.Select(value);
        }

        public void Hide() => Container.Hide();

        public void Show() => Container.Show();

        public override Drawable GetDrawableListItem() => gridContainer;
    }
}
