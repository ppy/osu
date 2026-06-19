// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class GroupSection : CompositeDrawable
    {
        private FormDiscreteAdjustmentControl<double> adjustmentControl = null!;

        private OsuButton button = null!;

        [Resolved]
        protected Bindable<ControlPointGroup> SelectedGroup { get; private set; } = null!;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

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
                        adjustmentControl = new FormDiscreteAdjustmentControl<double>(1)
                        {
                            Caption = "Time",
                            LabelFormat = v => v.ToLocalisableString(@"N0"),
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

            SelectedGroup.BindValueChanged(group =>
            {
                if (group.NewValue == null)
                {
                    adjustmentControl.Current.Disabled = true;
                    button.Enabled.Value = false;
                    return;
                }

                adjustmentControl.Current.Disabled = false;
                button.Enabled.Value = true;

                adjustmentControl.Current.Value = group.NewValue.Time;
            }, true);

            adjustmentControl.Current.BindValueChanged(_ => changeSelectedGroupTime(adjustmentControl.Current.Value));
        }

        private void changeSelectedGroupTime(in double time)
        {
            if (SelectedGroup.Value == null || time == SelectedGroup.Value.Time)
                return;

            changeHandler?.BeginChange();

            var currentGroupItems = SelectedGroup.Value.ControlPoints.ToArray();

            Beatmap.ControlPointInfo.RemoveGroup(SelectedGroup.Value);

            foreach (var cp in currentGroupItems)
            {
                // Only adjust hit object offsets if the group contains a timing control point
                if (cp is TimingControlPoint tp && configManager.Get<bool>(OsuSetting.EditorAdjustExistingObjectsOnTimingChanges))
                {
                    TimingSectionAdjustments.AdjustHitObjectOffset(Beatmap, tp, time - SelectedGroup.Value.Time);
                    Beatmap.UpdateAllHitObjects();
                }

                Beatmap.ControlPointInfo.Add(time, cp);
            }

            // the control point might not necessarily exist yet, if currentGroupItems was empty.
            SelectedGroup.Value = Beatmap.ControlPointInfo.GroupAt(time, true);

            changeHandler?.EndChange();
        }
    }
}
