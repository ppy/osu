// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        public override bool StaysOnPlate => false;

        protected Container ScaleContainer;

        public DrawableDroplet(Droplet h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Framework.Graphics.Drawable[]
            {
                ScaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new SkinnableDrawable(
                            new CatchSkinComponent(CatchSkinComponents.Droplet), _ => new Pulp
                            {
                                Size = Size,
                                AccentColour = { Value = Color4.White }
                            })
                    }
                }
            });

            ScaleContainer.Scale = new Vector2(HitObject.Scale);
        }

        protected override void UpdateComboColour(Color4 proposedColour, IReadOnlyList<Color4> comboColours)
        {
            // ignore the incoming combo colour as we use a custom lookup
            AccentColour.Value = comboColours[(HitObject.IndexInBeatmap + 1) % comboColours.Count];
        }
    }
}
