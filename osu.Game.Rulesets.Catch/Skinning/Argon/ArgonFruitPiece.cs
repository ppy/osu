// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal partial class ArgonFruitPiece : CatchHitObjectPiece
    {
        protected override Drawable HyperBorderPiece => hyperBorderPiece;

        private Drawable hyperBorderPiece = null!;

        private Container layers = null!;

        private float rotationRandomness;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            int largeBlobSeed = RNG.Next();

            InternalChildren = new[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                },
                layers = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new CircularBlob
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.15f,
                            InnerRadius = 0.5f,
                            Size = new Vector2(1.1f),
                            Seed = largeBlobSeed,
                        },
                        new CircularBlob
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            InnerRadius = 0.2f,
                            Alpha = 0.5f,
                            Seed = RNG.Next(),
                        },
                        new CircularBlob
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            InnerRadius = 0.05f,
                            Seed = RNG.Next(),
                        },
                    }
                },
                hyperBorderPiece = new CircularBlob
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    InnerRadius = 0.08f,
                    Size = new Vector2(1.15f),
                    Seed = largeBlobSeed
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindValueChanged(colour =>
            {
                foreach (var sprite in layers)
                    sprite.Colour = colour.NewValue;
            }, true);

            rotationRandomness = RNG.NextSingle(0.2f, 1) * (RNG.NextBool() ? -1 : 1);
        }

        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Rotation +=
                    // Layers are ordered from largest to smallest. Smaller layers should rotate more.
                    (i * 2)
                    * (float)Clock.ElapsedFrameTime
                    * 0.02f * rotationRandomness
                    // Each layer should alternate rotation direction.
                    * (i % 2 == 1 ? 1 : -1);
            }
        }
    }
}
