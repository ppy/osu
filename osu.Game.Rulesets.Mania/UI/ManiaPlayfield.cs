// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ScrollingPlayfield
    {
        public const float HIT_TARGET_POSITION = 50;

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

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        private readonly FlowContainer<Column> columns;
        public IEnumerable<Column> Columns => columns.Children;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        private readonly Container<DrawableManiaJudgement> judgements;

        private readonly int columnCount;

        public ManiaPlayfield(int columnCount)
            : base(Axes.Y)
        {
            this.columnCount = columnCount;

            if (columnCount <= 0)
                throw new ArgumentException("Can't have zero or fewer columns.");

            Inverted.Value = true;

            Container topLevelContainer;
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Playfield elements",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Columns mask",
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black
                                },
                                columns = new FillFlowContainer<Column>
                                {
                                    Name = "Columns",
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding { Left = 1, Right = 1 },
                                    Spacing = new Vector2(1, 0)
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Barlines mask",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Y,
                            Width = 1366, // Bar lines should only be masked on the vertical axis
                            BypassAutoSizeAxes = Axes.Both,
                            Masking = true,
                            Child = content = new Container
                            {
                                Name = "Bar lines",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Top = HIT_TARGET_POSITION }
                            }
                        },
                        judgements = new Container<DrawableManiaJudgement>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Y = HIT_TARGET_POSITION + 150,
                            BypassAutoSizeAxes = Axes.Both
                        },
                        topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
                    }
                }
            };

            var currentAction = ManiaAction.Key1;
            for (int i = 0; i < columnCount; i++)
            {
                var c = new Column();
                c.VisibleTimeRange.BindTo(VisibleTimeRange);

                c.IsSpecial = isSpecialColumn(i);
                c.Action = c.IsSpecial ? ManiaAction.Special : currentAction++;

                topLevelContainer.Add(c.TopLevelContainer.CreateProxy());

                columns.Add(c);
                AddNested(c);
            }

            Inverted.ValueChanged += invertedChanged;
            Inverted.TriggerChange();
        }

        private void invertedChanged(bool newValue)
        {
            Scale = new Vector2(1, newValue ? -1 : 1);
            judgements.Scale = Scale;
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
            foreach (var column in Columns)
            {
                if (!column.IsSpecial)
                    continue;

                column.AccentColour = specialColumnColour;
            }

            var nonSpecialColumns = Columns.Where(c => !c.IsSpecial).ToList();

            // We'll set the colours of the non-special columns in a separate loop, because the non-special
            // column colours are mirrored across their centre and special styles mess with this
            for (int i = 0; i < Math.Ceiling(nonSpecialColumns.Count / 2f); i++)
            {
                Color4 colour = normalColumnColours[i % normalColumnColours.Count];
                nonSpecialColumns[i].AccentColour = colour;
                nonSpecialColumns[nonSpecialColumns.Count - 1 - i].AccentColour = colour;
            }
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var maniaObject = (ManiaHitObject)judgedObject.HitObject;
            columns[maniaObject.Column].OnJudgement(judgedObject, judgement);

            judgements.Clear();
            judgements.Add(new DrawableManiaJudgement(judgement)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
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

        public override void Add(DrawableHitObject h) => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column).Add(h);

        public void Add(DrawableBarLine barline) => HitObjects.Add(barline);

        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            content.Width = columns.Width;
        }
    }
}
