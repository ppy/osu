// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// A collection of <see cref="Column"/>s.
    /// </summary>
    [Cached]
    public class ManiaStage : ScrollingPlayfield
    {
        public const float COLUMN_SPACING = 1;

        public const float HIT_TARGET_POSITION = 50;

        public IReadOnlyList<Column> Columns => columnFlow.Children;
        private readonly FillFlowContainer<Column> columnFlow;

        private readonly Container barLineContainer;

        public Container<DrawableManiaJudgement> Judgements => judgements;
        private readonly JudgementContainer<DrawableManiaJudgement> judgements;

        private readonly Container topLevelContainer;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Columns.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

        private readonly int firstColumnIndex;

        public ManiaStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
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
                            CornerRadius = 5,
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
                                    Padding = new MarginPadding { Left = COLUMN_SPACING, Right = COLUMN_SPACING },
                                    Spacing = new Vector2(COLUMN_SPACING, 0)
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
                            Child = barLineContainer = new Container
                            {
                                Name = "Bar lines",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Child = HitObjectContainer
                            }
                        },
                        judgements = new JudgementContainer<DrawableManiaJudgement>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Y = HIT_TARGET_POSITION + 150,
                        },
                        topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
                    }
                }
            };

            for (int i = 0; i < definition.Columns; i++)
            {
                var isSpecial = definition.IsSpecialColumn(i);
                var column = new Column(firstColumnIndex + i)
                {
                    IsSpecial = isSpecial,
                    Action = { Value = isSpecial ? specialColumnStartAction++ : normalColumnStartAction++ }
                };

                AddColumn(column);
            }

            Direction.BindValueChanged(dir =>
            {
                barLineContainer.Padding = new MarginPadding
                {
                    Top = dir.NewValue == ScrollingDirection.Up ? HIT_TARGET_POSITION : 0,
                    Bottom = dir.NewValue == ScrollingDirection.Down ? HIT_TARGET_POSITION : 0,
                };
            }, true);
        }

        public void AddColumn(Column c)
        {
            topLevelContainer.Add(c.TopLevelContainer.CreateProxy());
            columnFlow.Add(c);
            AddNested(c);
        }

        public override void Add(DrawableHitObject h)
        {
            var maniaObject = (ManiaHitObject)h.HitObject;

            int columnIndex = -1;

            maniaObject.ColumnBindable.BindValueChanged(_ =>
            {
                if (columnIndex != -1)
                    Columns.ElementAt(columnIndex).Remove(h);

                columnIndex = maniaObject.Column - firstColumnIndex;
                Columns.ElementAt(columnIndex).Add(h);
            }, true);

            h.OnNewResult += OnNewResult;
        }

        public override bool Remove(DrawableHitObject h)
        {
            var maniaObject = (ManiaHitObject)h.HitObject;
            int columnIndex = maniaObject.Column - firstColumnIndex;
            Columns.ElementAt(columnIndex).Remove(h);

            h.OnNewResult -= OnNewResult;
            return true;
        }

        public void Add(BarLine barline) => base.Add(new DrawableBarLine(barline));

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            judgements.Clear();
            judgements.Add(new DrawableManiaJudgement(result, judgedObject)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            normalColumnColours = new List<Color4>
            {
                new Color4(94, 0, 57, 255),
                new Color4(6, 84, 0, 255)
            };

            specialColumnColour = new Color4(0, 48, 63, 255);

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
            barLineContainer.Width = columnFlow.Width;
        }
    }
}
