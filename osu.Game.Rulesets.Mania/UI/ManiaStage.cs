// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// A collection of <see cref="Column"/>s.
    /// </summary>
    internal class ManiaStage : ScrollingPlayfield
    {
        public const float HIT_TARGET_POSITION = 50;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        public IReadOnlyList<Column> Columns => columnFlow.Children;
        private readonly FillFlowContainer<Column> columnFlow;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public Container<DrawableManiaJudgement> Judgements => judgements;
        private readonly JudgementContainer<DrawableManiaJudgement> judgements;

        private readonly Container topLevelContainer;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        private readonly int firstColumnIndex;

        public ManiaStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            : base(ScrollingDirection.Up)
        {
            this.firstColumnIndex = firstColumnIndex;

            Name = "Stage";

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new Container
                {
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
                                columnFlow = new FillFlowContainer<Column>
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
                        judgements = new JudgementContainer<DrawableManiaJudgement>
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

            for (int i = 0; i < definition.Columns; i++)
            {
                var isSpecial = definition.IsSpecialColumn(i);
                var column = new Column
                {
                    IsSpecial = isSpecial,
                    Action = isSpecial ? specialColumnStartAction++ : normalColumnStartAction++
                };

                AddColumn(column);
            }

            Inverted.ValueChanged += invertedChanged;
            Inverted.TriggerChange();
        }

        private void invertedChanged(bool newValue)
        {
            Scale = new Vector2(1, newValue ? -1 : 1);
            Judgements.Scale = Scale;
        }

        public void AddColumn(Column c)
        {
            c.VisibleTimeRange.BindTo(VisibleTimeRange);

            topLevelContainer.Add(c.TopLevelContainer.CreateProxy());
            columnFlow.Add(c);
            AddNested(c);
        }

        public override void Add(DrawableHitObject h)
        {
            var maniaObject = (ManiaHitObject)h.HitObject;
            int columnIndex = maniaObject.Column - firstColumnIndex;
            Columns.ElementAt(columnIndex).Add(h);
            h.OnJudgement += OnJudgement;
        }

        public void Add(BarLine barline) => base.Add(new DrawableBarLine(barline));

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgedObject.DisplayJudgement)
                return;

            judgements.Clear();
            judgements.Add(new DrawableManiaJudgement(judgement, judgedObject)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
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

        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            content.Width = columnFlow.Width;
        }
    }
}
