// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class GroupSection : CompositeDrawable
    {
        private LabelledTextBox textBox = null!;

        private OsuButton button = null!;

        [Resolved]
        protected Bindable<ControlPointGroup> SelectedGroup { get; private set; } = null!;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10) { Bottom = 0 };

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
                        button = new RoundedButton
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
                if (!isNew)
                    return;

                if (double.TryParse(sender.Text, out double newTime))
                {
                    changeSelectedGroupTime(newTime);
                }
                else
                {
                    SelectedGroup.TriggerChange();
                }
            };

            SelectedGroup.BindValueChanged(group =>
            {
                if (group.NewValue == null)
                {
                    textBox.Text = string.Empty;

                    // cannot use textBox.Current.Disabled due to https://github.com/ppy/osu-framework/issues/3919
                    textBox.ReadOnly = true;
                    button.Enabled.Value = false;
                    return;
                }

                textBox.ReadOnly = false;
                button.Enabled.Value = true;

                textBox.Text = $"{group.NewValue.Time:n0}";
            }, true);
        }

        private void changeSelectedGroupTime(in double time)
        {
            if (SelectedGroup.Value == null || time == SelectedGroup.Value.Time)
                return;

            changeHandler?.BeginChange();

            var currentGroupItems = SelectedGroup.Value.ControlPoints.ToArray();

            Beatmap.ControlPointInfo.RemoveGroup(SelectedGroup.Value);

            foreach (var cp in currentGroupItems)
                Beatmap.ControlPointInfo.Add(time, cp);

            // the control point might not necessarily exist yet, if currentGroupItems was empty.
            SelectedGroup.Value = Beatmap.ControlPointInfo.GroupAt(time, true);

            changeHandler?.EndChange();
        }
    }
}
