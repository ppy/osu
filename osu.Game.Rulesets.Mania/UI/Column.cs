// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public abstract class Column : ScrollingPlayfield, IKeyBindingHandler<ManiaAction>
    {
        public const float COLUMN_WIDTH = 80;

        /// <summary>
        /// The index of this column as part of the whole playfield.
        /// </summary>
        public readonly int Index;

        public readonly Bindable<ManiaAction> Action = new Bindable<ManiaAction>();

        protected abstract Container HitTargetContainer { get; }
        protected abstract Drawable KeyArea { get; }
        protected abstract Container ExplosionContainer { get; }
        protected internal abstract Container TopLevelContainer { get; }

        protected Column(int index)
        {
            Index = index;

            RelativeSizeAxes = Axes.Y;
            Width = COLUMN_WIDTH;
        }

        public static Column CreateColumnFromType(ColumnType type, int index)
        {
            switch (type)
            {
                case ColumnType.Odd:
                    return new OddColumn(index);

                case ColumnType.Even:
                    return new EvenColumn(index);

                case ColumnType.Special:
                    return new SpecialColumn(index);
            }

            return null;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Direction.BindValueChanged(dir =>
            {
                HitTargetContainer.Padding = new MarginPadding
                {
                    Top = dir.NewValue == ScrollingDirection.Up ? ManiaStage.HIT_TARGET_POSITION : 0,
                    Bottom = dir.NewValue == ScrollingDirection.Down ? ManiaStage.HIT_TARGET_POSITION : 0
                };

                ExplosionContainer.Padding = new MarginPadding
                {
                    Top = dir.NewValue == ScrollingDirection.Up ? NotePiece.NOTE_HEIGHT / 2 : 0,
                    Bottom = dir.NewValue == ScrollingDirection.Down ? NotePiece.NOTE_HEIGHT / 2 : 0
                };

                KeyArea.Anchor = KeyArea.Origin = dir.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
            }, true);
        }

        public virtual bool IsSpecial => false;

        public override Axes RelativeSizeAxes => Axes.Y;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IBindable<ManiaAction>>(Action);
            return dependencies;
        }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="hitObject">The DrawableHitObject to add.</param>
        public override void Add(DrawableHitObject hitObject)
        {
            hitObject.OnNewResult += OnNewResult;

            HitObjectContainer.Add(hitObject);
        }

        public override bool Remove(DrawableHitObject h)
        {
            if (!base.Remove(h))
                return false;

            h.OnNewResult -= OnNewResult;
            return true;
        }

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!result.IsHit || !judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            AddHitExplosion(judgedObject);
        }

        public abstract void AddHitExplosion(DrawableHitObject judgedObject);

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            var nextObject =
                HitObjectContainer.AliveObjects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                // fallback to non-alive objects to find next off-screen object
                HitObjectContainer.Objects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                HitObjectContainer.Objects.LastOrDefault();

            nextObject?.PlaySamples();

            return true;
        }

        public bool OnReleased(ManiaAction action) => false;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            // This probably shouldn't exist as is, but the columns in the stage are separated by a 1px border
            => DrawRectangle.Inflate(new Vector2(ManiaStage.COLUMN_SPACING / 2, 0)).Contains(ToLocalSpace(screenSpacePos));
    }
}
