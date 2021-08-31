// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    [Cached]
    public class Column : ScrollingPlayfield, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        public const float COLUMN_WIDTH = 80;
        public const float SPECIAL_COLUMN_WIDTH = 70;

        /// <summary>
        /// The index of this column as part of the whole playfield.
        /// </summary>
        public readonly int Index;

        public readonly Bindable<ManiaAction> Action = new Bindable<ManiaAction>();

        public readonly ColumnHitObjectArea HitObjectArea;
        internal readonly Container TopLevelContainer;
        private readonly DrawablePool<PoolableHitExplosion> hitExplosionPool;
        private readonly OrderedHitPolicy hitPolicy;
        public Container UnderlayElements => HitObjectArea.UnderlayElements;

        private readonly GameplaySampleTriggerSource sampleTriggerSource;

        public Column(int index)
        {
            Index = index;

            RelativeSizeAxes = Axes.Y;
            Width = COLUMN_WIDTH;

            Drawable background = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.ColumnBackground), _ => new DefaultColumnBackground())
            {
                RelativeSizeAxes = Axes.Both
            };

            InternalChildren = new[]
            {
                hitExplosionPool = new DrawablePool<PoolableHitExplosion>(5),
                sampleTriggerSource = new GameplaySampleTriggerSource(HitObjectContainer),
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                HitObjectArea = new ColumnHitObjectArea(Index, HitObjectContainer) { RelativeSizeAxes = Axes.Both },
                new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.KeyArea), _ => new DefaultKeyArea())
                {
                    RelativeSizeAxes = Axes.Both
                },
                background,
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            hitPolicy = new OrderedHitPolicy(HitObjectContainer);

            TopLevelContainer.Add(HitObjectArea.Explosions.CreateProxy());

            RegisterPool<Note, DrawableNote>(10, 50);
            RegisterPool<HoldNote, DrawableHoldNote>(10, 50);
            RegisterPool<HeadNote, DrawableHoldNoteHead>(10, 50);
            RegisterPool<TailNote, DrawableHoldNoteTail>(10, 50);
            RegisterPool<HoldNoteTick, DrawableHoldNoteTick>(50, 250);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            NewResult += OnNewResult;
        }

        public ColumnType ColumnType { get; set; }

        public bool IsSpecial => ColumnType == ColumnType.Special;

        public Color4 AccentColour { get; set; }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IBindable<ManiaAction>>(Action);
            return dependencies;
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject drawableHitObject)
        {
            base.OnNewDrawableHitObject(drawableHitObject);

            DrawableManiaHitObject maniaObject = (DrawableManiaHitObject)drawableHitObject;

            maniaObject.AccentColour.Value = AccentColour;
            maniaObject.CheckHittable = hitPolicy.IsHittable;
        }

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (result.IsHit)
                hitPolicy.HandleHit(judgedObject);

            if (!result.IsHit || !judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            HitObjectArea.Explosions.Add(hitExplosionPool.Get(e => e.Apply(result)));
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            sampleTriggerSource.Play();
            return true;
        }

        public void OnReleased(ManiaAction action)
        {
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            // This probably shouldn't exist as is, but the columns in the stage are separated by a 1px border
            => DrawRectangle.Inflate(new Vector2(Stage.COLUMN_SPACING / 2, 0)).Contains(ToLocalSpace(screenSpacePos));
    }
}
