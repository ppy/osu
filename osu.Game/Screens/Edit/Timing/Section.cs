// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    internal abstract partial class Section<T> : CompositeDrawable
        where T : ControlPoint
    {
        protected OsuCheckbox Checkbox { get; private set; } = null!;
        private Container content = null!;

        protected FillFlowContainer Flow { get; private set; } = null!;

        protected Bindable<T?> ControlPoint { get; } = new Bindable<T?>();

        private const float header_height = 50;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        [Resolved]
        protected Bindable<ControlPointGroup?> SelectedGroup { get; private set; } = null!;

        [Resolved]
        protected IEditorChangeHandler? ChangeHandler { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeDuration = 200;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        Checkbox = new OsuCheckbox
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            LabelText = typeof(T).Name.Replace(nameof(Beatmaps.ControlPoints.ControlPoint), string.Empty)
                        }
                    }
                },
                content = new Container
                {
                    Y = header_height,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        Flow = new FillFlowContainer
                        {
                            Padding = new MarginPadding(10) { Top = 0 },
                            Spacing = new Vector2(20),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        },
                    }
                }
            };
        }

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
                        SelectedGroup.Value.Add(ControlPoint.Value = CreatePoint(SelectedGroup.Value));
                }
                else
                {
                    if (ControlPoint.Value != null)
                    {
                        SelectedGroup.Value!.Remove(ControlPoint.Value);
                        ControlPoint.Value = null;
                    }
                }

                content.BypassAutoSizeAxes = selected.NewValue ? Axes.None : Axes.Y;
            }, true);

            SelectedGroup.BindValueChanged(OnSelectedGroupChanged, true);
            ControlPoint.BindValueChanged(OnControlPointChanged, true);
        }

        protected virtual void OnSelectedGroupChanged(ValueChangedEvent<ControlPointGroup?> group)
        {
            ControlPoint.Value = group.NewValue?.ControlPoints.OfType<T>().FirstOrDefault();
            Checkbox.Current.Value = ControlPoint.Value != null;
        }

        protected abstract void OnControlPointChanged(ValueChangedEvent<T?> point);

        protected abstract T CreatePoint(ControlPointGroup selectedGroup);
    }
}
