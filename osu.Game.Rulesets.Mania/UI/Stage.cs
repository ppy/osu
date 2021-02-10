// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// A collection of <see cref="Column"/>s.
    /// </summary>
    public class Stage : ScrollingPlayfield
    {
        public const float COLUMN_SPACING = 1;

        public const float HIT_TARGET_POSITION = 110;

        public IReadOnlyList<Column> Columns => columnFlow.Content;
        private readonly ColumnFlow<Column> columnFlow;

        private readonly JudgementContainer<DrawableManiaJudgement> judgements;
        private readonly DrawablePool<DrawableManiaJudgement> judgementPool;

        private readonly Drawable barLineContainer;

        private readonly Dictionary<ColumnType, Color4> columnColours = new Dictionary<ColumnType, Color4>
        {
            { ColumnType.Even, new Color4(6, 84, 0, 255) },
            { ColumnType.Odd, new Color4(94, 0, 57, 255) },
            { ColumnType.Special, new Color4(0, 48, 63, 255) }
        };

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Columns.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

        private readonly int firstColumnIndex;

        public Stage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
        {
            this.firstColumnIndex = firstColumnIndex;

            Name = "Stage";

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Container topLevelContainer;

            InternalChildren = new Drawable[]
            {
                judgementPool = new DrawablePool<DrawableManiaJudgement>(2),
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.StageBackground, stageDefinition: definition), _ => new DefaultStageBackground())
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        columnFlow = new ColumnFlow<Column>(definition)
                        {
                            RelativeSizeAxes = Axes.Y,
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
                            Child = barLineContainer = new HitObjectArea(HitObjectContainer)
                            {
                                Name = "Bar lines",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                            }
                        },
                        new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.StageForeground, stageDefinition: definition), _ => null)
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        judgements = new JudgementContainer<DrawableManiaJudgement>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Y = HIT_TARGET_POSITION + 150
                        },
                        topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
                    }
                }
            };

            for (int i = 0; i < definition.Columns; i++)
            {
                var columnType = definition.GetTypeOfColumn(i);

                var column = new Column(firstColumnIndex + i)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1,
                    ColumnType = columnType,
                    AccentColour = columnColours[columnType],
                    Action = { Value = columnType == ColumnType.Special ? specialColumnStartAction++ : normalColumnStartAction++ }
                };

                topLevelContainer.Add(column.TopLevelContainer.CreateProxy());
                columnFlow.SetContentForColumn(i, column);
                AddNested(column);
            }
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

            // Tick judgements should not display text.
            if (judgedObject is DrawableHoldNoteTick)
                return;

            judgements.Clear(false);
            judgements.Add(judgementPool.Get(j =>
            {
                j.Apply(result, judgedObject);

                j.Anchor = Anchor.Centre;
                j.Origin = Anchor.Centre;
            }));
        }

        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            barLineContainer.Width = columnFlow.Width;
        }
    }
}
