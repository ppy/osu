// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal class ArgonFruitPiece : CatchHitObjectPiece
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override Drawable HyperBorderPiece => hyperBorderPiece;

        private Drawable hyperBorderPiece = null!;

        private Container layers = null!;

        private float rotationRandomness;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;

            Texture largeTexture = getTexture("A");

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
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.15f,
                            Texture = largeTexture
                        },
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.5f,
                            Texture = getTexture("B")
                        },
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Blending = BlendingParameters.Additive,
                            Texture = getTexture("C")
                        },
                    }
                },
                hyperBorderPiece = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.15f,
                    Texture = largeTexture,
                },
            };

            Texture getTexture(string type) => textures.Get($"Gameplay/catch/blob-{type}{RNG.Next(1, 7)}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IndexInBeatmap.BindValueChanged(index =>
            {
                VisualRepresentation.Value = Fruit.GetVisualRepresentation(index.NewValue);
            }, true);

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
