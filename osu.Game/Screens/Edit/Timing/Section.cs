// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Timing
{
    internal abstract class Section<T> : CompositeDrawable
        where T : ControlPoint
    {
        private OsuCheckbox checkbox;
        private Container content;

        protected FillFlowContainer Flow { get; private set; }

        protected Bindable<T> ControlPoint { get; } = new Bindable<T>();

        private const float header_height = 20;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        [Resolved]
        protected Bindable<ControlPointGroup> SelectedGroup { get; private set; }

        [Resolved(canBeNull: true)]
        protected IEditorChangeHandler ChangeHandler { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeDuration = 200;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeAxes = Axes.Y;

            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray1,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Children = new Drawable[]
                    {
                        checkbox = new OsuCheckbox
                        {
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
                        new Box
                        {
                            Colour = colours.Gray2,
                            RelativeSizeAxes = Axes.Both,
                        },
                        Flow = new FillFlowContainer
                        {
                            Padding = new MarginPadding(10),
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

            checkbox.Current.BindValueChanged(selected =>
            {
                if (selected.NewValue)
                {
                    if (SelectedGroup.Value == null)
                    {
                        checkbox.Current.Value = false;
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

                content.BypassAutoSizeAxes = selected.NewValue ? Axes.None : Axes.Y;
            }, true);

            SelectedGroup.BindValueChanged(points =>
            {
                ControlPoint.Value = points.NewValue?.ControlPoints.OfType<T>().FirstOrDefault();
                checkbox.Current.Value = ControlPoint.Value != null;
            }, true);

            ControlPoint.BindValueChanged(OnControlPointChanged, true);
        }

        protected abstract void OnControlPointChanged(ValueChangedEvent<T> point);

        protected abstract T CreatePoint();
    }
}
