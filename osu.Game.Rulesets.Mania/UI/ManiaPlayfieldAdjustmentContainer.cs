// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class ManiaPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        protected override Container<Drawable> Content { get; }

        private readonly DrawSizePreservingFillContainer scalingContainer;

        public ManiaPlayfieldAdjustmentContainer()
        {
            InternalChild = scalingContainer = new DrawSizePreservingFillContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [Resolved]
        private DrawableManiaRuleset drawableManiaRuleset { get; set; } = null!;

        protected override void Update()
        {
            base.Update();

            float aspectRatio = DrawWidth / DrawHeight;
            bool isPortrait = aspectRatio < 4 / 3f;

            if (isPortrait && drawableManiaRuleset.Beatmap.Stages.Count == 1)
            {
                // Scale playfield up by 25% to become playable on mobile devices,
                // and leave a 10% horizontal gap if the playfield is scaled down due to being too wide.
                const float base_scale = 1.25f;
                const float base_width = 768f / base_scale;
                const float side_gap = 0.9f;

                scalingContainer.Strategy = DrawSizePreservationStrategy.Maximum;
                float stageWidth = drawableManiaRuleset.Playfield.Stages[0].DrawWidth;
                scalingContainer.TargetDrawSize = new Vector2(1024, base_width * Math.Max(stageWidth / aspectRatio / (base_width * side_gap), 1f));
            }
            else
            {
                scalingContainer.Strategy = DrawSizePreservationStrategy.Minimum;
                scalingContainer.Scale = new Vector2(1f);
                scalingContainer.Size = new Vector2(1f);
                scalingContainer.TargetDrawSize = new Vector2(1024, 768);
            }
        }
    }
}
