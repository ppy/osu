// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Mania.Beatmaps;

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

        private readonly ColumnHitObjectArea hitObjectArea;

        internal readonly Container TopLevelContainer;

        public Container UnderlayElements => hitObjectArea.UnderlayElements;

        public Column(int index)
        {
            Index = index;

            RelativeSizeAxes = Axes.Y;
            Width = COLUMN_WIDTH;

            Drawable background = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.ColumnBackground, Index), _ => new DefaultColumnBackground())
            {
                RelativeSizeAxes = Axes.Both
            };

            InternalChildren = new[]
            {
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                hitObjectArea = new ColumnHitObjectArea(Index, HitObjectContainer) { RelativeSizeAxes = Axes.Both },
                new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.KeyArea, Index), _ => new DefaultKeyArea())
                {
                    RelativeSizeAxes = Axes.Both
                },
                background,
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(hitObjectArea.Explosions.CreateProxy());
        }

        public override Axes RelativeSizeAxes => Axes.Y;

        public ColumnType ColumnType { get; set; }

        public bool IsSpecial => ColumnType == ColumnType.Special;

        public Color4 AccentColour { get; set; }

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
            hitObject.AccentColour.Value = AccentColour;
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

            var explosion = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitExplosion, Index), _ =>
                new DefaultHitExplosion(judgedObject.AccentColour.Value, judgedObject is DrawableHoldNoteTick))
            {
                RelativeSizeAxes = Axes.Both
            };

            hitObjectArea.Explosions.Add(explosion);

            explosion.Delay(200).Expire(true);
        }

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

        public void OnReleased(ManiaAction action)
        {
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            // This probably shouldn't exist as is, but the columns in the stage are separated by a 1px border
            => DrawRectangle.Inflate(new Vector2(Stage.COLUMN_SPACING / 2, 0)).Contains(ToLocalSpace(screenSpacePos));
    }
}
