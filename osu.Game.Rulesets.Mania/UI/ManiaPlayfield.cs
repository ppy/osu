// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Framework.Graphics.Containers;
using System;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using OpenTK.Input;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mania.Timing;
using osu.Framework.Input;
using osu.Game.Beatmaps.Timing;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : Playfield<ManiaHitObject, ManiaJudgement>
    {
        public const float HIT_TARGET_POSITION = 50;

        private const float time_span_default = 5000;
        private const float time_span_min = 10;
        private const float time_span_max = 50000;
        private const float time_span_step = 200;

        /// <summary>
        /// Default column keys, expanding outwards from the middle as more column are added.
        /// E.g. 2 columns use FJ, 4 columns use DFJK, 6 use SDFJKL, etc...
        /// </summary>
        private static readonly Key[] default_keys = { Key.A, Key.S, Key.D, Key.F, Key.J, Key.K, Key.L, Key.Semicolon };

        private SpecialColumnPosition specialColumnPosition;
        /// <summary>
        /// The style to use for the special column.
        /// </summary>
        public SpecialColumnPosition SpecialColumnPosition
        {
            get { return specialColumnPosition; }
            set
            {
                if (IsLoaded)
                    throw new InvalidOperationException($"Setting {nameof(SpecialColumnPosition)} after the playfield is loaded requires re-creating the playfield.");
                specialColumnPosition = value;
            }
        }

        public readonly FlowContainer<Column> Columns;

        private readonly ControlPointContainer barlineContainer;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        private readonly int columnCount;

        public ManiaPlayfield(int columnCount, IEnumerable<ControlPoint> timingChanges)
        {
            this.columnCount = columnCount;

            if (columnCount <= 0)
                throw new ArgumentException("Can't have zero or fewer columns.");

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black
                        },
                        Columns = new FillFlowContainer<Column>
                        {
                            Name = "Columns",
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = 1, Right = 1 },
                            Spacing = new Vector2(1, 0)
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = HIT_TARGET_POSITION },
                            Children = new[]
                            {
                                barlineContainer = new ControlPointContainer(timingChanges)
                                {
                                    Name = "Bar lines",
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        }
                    }
                }
            };

            for (int i = 0; i < columnCount; i++)
                Columns.Add(new Column(timingChanges));

            TimeSpan = time_span_default;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalColumnColours = new List<Color4>
            {
                colours.RedDark,
                colours.GreenDark
            };

            specialColumnColour = colours.BlueDark;

            // Set the special column + colour + key
            for (int i = 0; i < columnCount; i++)
            {
                Column column = Columns.Children.ElementAt(i);
                column.IsSpecial = isSpecialColumn(i);

                if (!column.IsSpecial)
                    continue;

                column.Key = Key.Space;
                column.AccentColour = specialColumnColour;
            }

            var nonSpecialColumns = Columns.Children.Where(c => !c.IsSpecial).ToList();

            // We'll set the colours of the non-special columns in a separate loop, because the non-special
            // column colours are mirrored across their centre and special styles mess with this
            for (int i = 0; i < Math.Ceiling(nonSpecialColumns.Count / 2f); i++)
            {
                Color4 colour = normalColumnColours[i % normalColumnColours.Count];
                nonSpecialColumns[i].AccentColour = colour;
                nonSpecialColumns[nonSpecialColumns.Count - 1 - i].AccentColour = colour;
            }

            // We'll set the keys for non-special columns in another separate loop because it's not mirrored like the above colours
            // Todo: This needs to go when we get to bindings and use Button1, ..., ButtonN instead
            for (int i = 0; i < nonSpecialColumns.Count; i++)
            {
                Column column = nonSpecialColumns[i];

                int keyOffset = default_keys.Length / 2 - nonSpecialColumns.Count / 2 + i;
                if (keyOffset >= 0 && keyOffset < default_keys.Length)
                    column.Key = default_keys[keyOffset];
                else
                    // There is no default key defined for this column. Let's set this to Unknown for now
                    // however note that this will be gone after bindings are in place
                    column.Key = Key.Unknown;
            }
        }

        /// <summary>
        /// Whether the column index is a special column for this playfield.
        /// </summary>
        /// <param name="column">The 0-based column index.</param>
        /// <returns>Whether the column is a special column.</returns>
        private bool isSpecialColumn(int column)
        {
            switch (SpecialColumnPosition)
            {
                default:
                case SpecialColumnPosition.Normal:
                    return columnCount % 2 == 1 && column == columnCount / 2;
                case SpecialColumnPosition.Left:
                    return column == 0;
                case SpecialColumnPosition.Right:
                    return column == columnCount - 1;
            }
        }

        public override void Add(DrawableHitObject<ManiaHitObject, ManiaJudgement> h) => Columns.Children.ElementAt(h.HitObject.Column).Add(h);

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (state.Keyboard.ControlPressed)
            {
                switch (args.Key)
                {
                    case Key.Minus:
                        transformTimeSpanTo(TimeSpan + time_span_step, 200, EasingTypes.OutQuint);
                        break;
                    case Key.Plus:
                        transformTimeSpanTo(TimeSpan - time_span_step, 200, EasingTypes.OutQuint);
                        break;
                }
            }

            return false;
        }

        private double timeSpan;
        /// <summary>
        /// The amount of time which the length of the playfield spans.
        /// </summary>
        public double TimeSpan
        {
            get { return timeSpan; }
            set
            {
                if (timeSpan == value)
                    return;
                timeSpan = value;

                timeSpan = MathHelper.Clamp(timeSpan, time_span_min, time_span_max);

                barlineContainer.TimeSpan = value;
                Columns.Children.ForEach(c => c.ControlPointContainer.TimeSpan = value);
            }
        }

        private void transformTimeSpanTo(double newTimeSpan, double duration = 0, EasingTypes easing = EasingTypes.None)
        {
            TransformTo(() => TimeSpan, newTimeSpan, duration, easing, new TransformTimeSpan());
        }

        private class TransformTimeSpan : Transform<double>
        {
            public override double CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);

                var p = (ManiaPlayfield)d;
                p.TimeSpan = CurrentValue;
            }
        }
    }
}
