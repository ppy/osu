// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Timing
{
    public class ControlPointTable : EditorTable
    {
        [Resolved]
        private Bindable<ControlPointGroup> selectedGroup { get; set; }

        [Resolved]
        private EditorClock clock { get; set; }

        public IEnumerable<ControlPointGroup> ControlGroups
        {
            set
            {
                Content = null;
                BackgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                foreach (var group in value)
                {
                    BackgroundFlow.Add(new RowBackground(group)
                    {
                        Action = () =>
                        {
                            selectedGroup.Value = group;
                            clock.SeekSmoothlyTo(group.Time);
                        }
                    });
                }

                Columns = createHeaders();
                Content = value.Select((g, i) => createContent(i, g)).ToArray().ToRectangular();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedGroup.BindValueChanged(group =>
            {
                foreach (var b in BackgroundFlow) b.Selected = b.Item == group.NewValue;
            }, true);
        }

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Time", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn(),
                new TableColumn("Attributes", Anchor.CentreLeft),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, ControlPointGroup group) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold),
                Margin = new MarginPadding(10)
            },
            new OsuSpriteText
            {
                Text = group.Time.ToEditorFormattedString(),
                Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.Bold)
            },
            null,
            new ControlGroupAttributes(group),
        };

        private class ControlGroupAttributes : CompositeDrawable
        {
            private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

            private readonly FillFlowContainer fill;

            public ControlGroupAttributes(ControlPointGroup group)
            {
                RelativeSizeAxes = Axes.Both;
                InternalChild = fill = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2)
                };

                controlPoints.BindTo(group.ControlPoints);
            }

            [Resolved]
            private OsuColour colours { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                createChildren();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                controlPoints.CollectionChanged += (_, __) => createChildren();
            }

            private void createChildren()
            {
                fill.ChildrenEnumerable = controlPoints.Select(createAttribute).Where(c => c != null);
            }

            private Drawable createAttribute(ControlPoint controlPoint)
            {
                Color4 colour = controlPoint.GetRepresentingColour(colours);

                switch (controlPoint)
                {
                    case TimingControlPoint timing:
                        return new RowAttribute("timing", () => $"{60000 / timing.BeatLength:n1}bpm {timing.TimeSignature}", colour);

                    case DifficultyControlPoint difficulty:

                        return new RowAttribute("difficulty", () => $"{difficulty.SpeedMultiplier:n2}x", colour);

                    case EffectControlPoint effect:
                        return new RowAttribute("effect", () => string.Join(" ",
                            effect.KiaiMode ? "Kiai" : string.Empty,
                            effect.OmitFirstBarLine ? "NoBarLine" : string.Empty
                        ).Trim(), colour);

                    case SampleControlPoint sample:
                        return new RowAttribute("sample", () => $"{sample.SampleBank} {sample.SampleVolume}%", colour);
                }

                return null;
            }
        }
    }
}
