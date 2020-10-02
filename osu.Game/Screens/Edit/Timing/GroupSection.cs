// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    internal class GroupSection : CompositeDrawable
    {
        private LabelledTextBox textBox;

        [Resolved]
        protected Bindable<ControlPointGroup> SelectedGroup { get; private set; }

        [Resolved]
        protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

        [Resolved]
        private EditorClock clock { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        textBox = new LabelledTextBox
                        {
                            Label = "Time"
                        },
                        new TriangleButton
                        {
                            Text = "Use current time",
                            RelativeSizeAxes = Axes.X,
                            Action = () => changeSelectedGroupTime(clock.CurrentTime)
                        }
                    }
                },
            };

            textBox.OnCommit += (sender, isNew) =>
            {
                if (double.TryParse(sender.Text, out var newTime))
                {
                    changeSelectedGroupTime(newTime);
                }
            };

            SelectedGroup.BindValueChanged(group =>
            {
                if (group.NewValue == null)
                {
                    textBox.Text = string.Empty;
                    textBox.Current.Disabled = true;
                    return;
                }

                textBox.Current.Disabled = false;
                textBox.Text = $"{group.NewValue.Time:n0}";
            }, true);
        }

        private void changeSelectedGroupTime(in double time)
        {
            var currentGroupItems = SelectedGroup.Value.ControlPoints.ToArray();

            Beatmap.Value.Beatmap.ControlPointInfo.RemoveGroup(SelectedGroup.Value);

            foreach (var cp in currentGroupItems)
                Beatmap.Value.Beatmap.ControlPointInfo.Add(time, cp);

            SelectedGroup.Value = Beatmap.Value.Beatmap.ControlPointInfo.GroupAt(time);
        }
    }
}
