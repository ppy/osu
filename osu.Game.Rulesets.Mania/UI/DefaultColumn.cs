// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DefaultColumn : Column, IHasAccentColour
    {
        private readonly ColumnBackground background;
        private readonly ColumnKeyArea keyArea;
        private readonly ColumnHitObjectArea hitObjectArea;

        private readonly Container hitTargetContainer;
        private readonly Container topLevelContainer;
        private readonly Container explosionContainer;

        // Push up more stuff when needed.
        protected override Container HitTargetContainer => hitTargetContainer;
        protected override Drawable KeyArea => keyArea;
        protected override Container ExplosionContainer => explosionContainer;
        protected internal override Container TopLevelContainer => topLevelContainer;

        public DefaultColumn(int index)
            : base(index)
        {
            RelativeSizeAxes = Axes.Y;
            Width = COLUMN_WIDTH;

            background = new ColumnBackground { RelativeSizeAxes = Axes.Both };

            InternalChildren = new[]
            {
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                hitTargetContainer = new Container
                {
                    Name = "Hit target + hit objects",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        hitObjectArea = new ColumnHitObjectArea(HitObjectContainer)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        explosionContainer = new Container
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
                topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            topLevelContainer.Add(explosionContainer.CreateProxy());
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
    }
}
