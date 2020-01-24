// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        private const float playfield_size_adjust = 0.8f;

        public OsuPlayfieldAdjustmentContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            // Calculated from osu!stable as 512 (default gamefield size) / 640 (default window size)
            Size = new Vector2(playfield_size_adjust);

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 4f / 3,
                Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both }
            };
        }

        /// <summary>
        /// A <see cref="Container"/> which scales its content relative to a target width.
        /// </summary>
        private class ScalingContainer : Container
        {
            protected override void Update()
            {
                base.Update();

                // The following calculation results in a constant of 1.6 when OsuPlayfieldAdjustmentContainer
                // is consuming the full game_size. This matches the osu-stable "magic ratio".
                //
                // game_size = DrawSizePreservingFillContainer.TargetSize = new Vector2(1024, 768)
                //
                // Parent is a 4:3 aspect enforced, using height as the constricting dimension
                // Parent.ChildSize.X = min(game_size.X, game_size.Y * (4 / 3)) * playfield_size_adjust
                // Parent.ChildSize.X = 819.2
                //
                // Scale = 819.2 / 512
                // Scale = 1.6
                Scale = new Vector2(Parent.ChildSize.X / OsuPlayfield.BASE_SIZE.X);
                // Size = 0.625
                Size = Vector2.Divide(Vector2.One, Scale);
            }
        }
    }
}
