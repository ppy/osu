// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Timing
{
    internal abstract class ControlPointSection<T> : Section
        where T : ControlPoint
    {
        protected Bindable<T> ControlPoint { get; } = new Bindable<T>();

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        [Resolved]
        protected Bindable<ControlPointGroup> SelectedGroup { get; private set; }

        [Resolved(canBeNull: true)]
        protected IEditorChangeHandler ChangeHandler { get; private set; }

        protected override string SectionName { get; } = typeof(T).Name.Replace(nameof(Beatmaps.ControlPoints.ControlPoint), string.Empty);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Checkbox.Current.BindValueChanged(selected =>
            {
                if (selected.NewValue)
                {
                    if (SelectedGroup.Value == null)
                    {
                        Checkbox.Current.Value = false;
                        return;
                    }

                    if (ControlPoint.Value == null)
                        SelectedGroup.Value.Add(ControlPoint.Value = CreatePoint());
                }
                else
                {
                    if (ControlPoint.Value != null)
                    {
                        SelectedGroup.Value.Remove(ControlPoint.Value);
                        ControlPoint.Value = null;
                    }
                }
            }, true);

            SelectedGroup.BindValueChanged(points =>
            {
                ControlPoint.Value = points.NewValue?.ControlPoints.OfType<T>().FirstOrDefault();
                Checkbox.Current.Value = ControlPoint.Value != null;
            }, true);

            ControlPoint.BindValueChanged(OnControlPointChanged, true);
        }

        protected abstract void OnControlPointChanged(ValueChangedEvent<T> point);

        protected abstract T CreatePoint();
    }
}
