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

namespace osu.Game.Rulesets.Mania.UI
{
    [Cached]
    public class Column : ScrollingPlayfield, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        public const float COLUMN_WIDTH = 80;
        private const float special_column_width = 70;

        /// <summary>
        /// The index of this column as part of the whole playfield.
        /// </summary>
        public readonly int Index;

        public readonly Bindable<ManiaAction> Action = new Bindable<ManiaAction>();

        private readonly ColumnKeyArea keyArea;

        internal readonly Container TopLevelContainer;

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
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                hitObjectArea = new ColumnHitObjectArea(HitObjectContainer) { RelativeSizeAxes = Axes.Both },
                keyArea = new ColumnKeyArea
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ManiaStage.HIT_TARGET_POSITION,
                },
                background,
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(hitObjectArea.Explosions.CreateProxy());

            Direction.BindValueChanged(dir =>
            {
                keyArea.Anchor = keyArea.Origin = dir.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
            }, true);
        }

        public override Axes RelativeSizeAxes => Axes.Y;

        private bool isSpecial;

        public bool IsSpecial
        {
            get => isSpecial;
            set
            {
                if (isSpecial == value)
                    return;

                isSpecial = value;

                Width = isSpecial ? special_column_width : COLUMN_WIDTH;
            }
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                keyArea.AccentColour = value;
            }
        }

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

            hitObjectArea.Explosions.Add(new HitExplosion(judgedObject.AccentColour.Value, judgedObject is DrawableHoldNoteTick)
            {
                Anchor = Direction.Value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre,
                Origin = Anchor.Centre
            });
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
            => DrawRectangle.Inflate(new Vector2(ManiaStage.COLUMN_SPACING / 2, 0)).Contains(ToLocalSpace(screenSpacePos));
    }
}
