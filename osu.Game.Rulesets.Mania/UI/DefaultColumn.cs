// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI.Components;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DefaultColumn : Column, IHasAccentColour
    {
        private readonly ColumnBackground background;
        private readonly ColumnKeyArea keyArea;
        private readonly ColumnHitObjectArea hitObjectArea;

        // Pull up more stuff when needed.
        protected override Container HitTargetContainer { get; }
        protected override Drawable KeyArea => keyArea;
        protected override Container ExplosionContainer { get; }
        protected internal override Container TopLevelContainer { get; }

        public DefaultColumn(int index)
            : base(index)
        {
            background = new ColumnBackground { RelativeSizeAxes = Axes.Both };

            InternalChildren = new[]
            {
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                HitTargetContainer = new Container
                {
                    Name = "Hit target + hit objects",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        hitObjectArea = new ColumnHitObjectArea(HitObjectContainer)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        ExplosionContainer = new Container
                        {
                            Name = "Hit explosions",
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
                keyArea = new ColumnKeyArea
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ManiaStage.HIT_TARGET_POSITION,
                },
                background,
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(ExplosionContainer.CreateProxy());
        }

        public override Axes RelativeSizeAxes => Axes.Y;

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                background.AccentColour = value;
                keyArea.AccentColour = value;
                hitObjectArea.AccentColour = value;
            }
        }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="hitObject">The DrawableHitObject to add.</param>
        public override void Add(DrawableHitObject hitObject)
        {
            hitObject.AccentColour.Value = AccentColour;
            base.Add(hitObject);
        }

        public override void AddHitExplosion(DrawableHitObject judgedObject)
        {
            ExplosionContainer.Add(new HitExplosion(judgedObject.AccentColour.Value, judgedObject is DrawableHoldNoteTick)
            {
                Anchor = Direction.Value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre,
                Origin = Anchor.Centre
            });
        }
    }
}
