// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// A collection of <see cref="Column"/>s.
    /// </summary>
    public partial class Stage : ScrollingPlayfield
    {
        [Cached]
        public readonly StageDefinition Definition;

        public const float COLUMN_SPACING = 1;

        public const float HIT_TARGET_POSITION = 110;

        public IReadOnlyList<Column> Columns => columnFlow.Content;
        private readonly ColumnFlow<Column> columnFlow;

        private readonly JudgementContainer<DrawableManiaJudgement> judgements;
        private readonly JudgementPooler<DrawableManiaJudgement> judgementPooler;

        private readonly Drawable barLineContainer;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Columns.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

        private readonly int firstColumnIndex;

        private ISkinSource currentSkin = null!;

        public Stage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
        {
            this.firstColumnIndex = firstColumnIndex;
            Definition = definition;

            Name = "Stage";

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Container columnBackgrounds;
            Container topLevelContainer;

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
                        new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.StageBackground), _ => new DefaultStageBackground())
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        columnBackgrounds = new Container
                        {
                            Name = "Column backgrounds",
                            RelativeSizeAxes = Axes.Both,
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
                        columnFlow = new ColumnFlow<Column>(definition)
                        {
                            RelativeSizeAxes = Axes.Y,
                        },
                        new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.StageForeground))
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
                bool isSpecial = definition.IsSpecialColumn(i);

                var column = new Column(firstColumnIndex + i, isSpecial)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1,
                    Action = { Value = isSpecial ? specialColumnStartAction++ : normalColumnStartAction++ }
                };

                topLevelContainer.Add(column.TopLevelContainer.CreateProxy());
                columnBackgrounds.Add(column.BackgroundContainer.CreateProxy());
                columnFlow.SetContentForColumn(i, column);
                AddNested(column);
            }

            var hitWindows = new ManiaHitWindows();

            AddInternal(judgementPooler = new JudgementPooler<DrawableManiaJudgement>(Enum.GetValues<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r))));

            RegisterPool<BarLine, DrawableBarLine>(50, 200);
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            currentSkin = skin;

            skin.SourceChanged += onSkinChanged;
            onSkinChanged();
        }

        private void onSkinChanged()
        {
            float paddingTop = currentSkin.GetConfig<ManiaSkinConfigurationLookup, float>(new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.StagePaddingTop))?.Value ?? 0;
            float paddingBottom = currentSkin.GetConfig<ManiaSkinConfigurationLookup, float>(new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.StagePaddingBottom))?.Value ?? 0;

            Padding = new MarginPadding
            {
                Top = paddingTop,
                Bottom = paddingBottom,
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            // must happen before children are disposed in base call to prevent illegal accesses to the judgement pool.
            NewResult -= OnNewResult;

            base.Dispose(isDisposing);

            if (currentSkin.IsNotNull())
                currentSkin.SourceChanged -= onSkinChanged;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            NewResult += OnNewResult;
        }

        public override void Add(HitObject hitObject) => Columns.ElementAt(((ManiaHitObject)hitObject).Column - firstColumnIndex).Add(hitObject);

        public override bool Remove(HitObject hitObject) => Columns.ElementAt(((ManiaHitObject)hitObject).Column - firstColumnIndex).Remove(hitObject);

        public override void Add(DrawableHitObject h) => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column - firstColumnIndex).Add(h);

        public override bool Remove(DrawableHitObject h) => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column - firstColumnIndex).Remove(h);

        public void Add(BarLine barLine) => base.Add(barLine);

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            judgements.Clear(false);
            judgements.Add(judgementPooler.Get(result.Type, j =>
            {
                j.Apply(result, judgedObject);

                j.Anchor = Anchor.Centre;
                j.Origin = Anchor.Centre;
            })!);
        }

        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            barLineContainer.Width = columnFlow.Width;
        }
    }
}
