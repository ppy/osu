// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonReverseArrow : BeatSyncedContainer
    {
        [Resolved]
        private DrawableHitObject drawableRepeat { get; set; } = null!;

        private Bindable<Color4> accentColour = null!;

        private SpriteIcon icon = null!;

        private Container main = null!;
        private Sprite side = null!;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, DrawableHitObject hitObject)
        {
            Divisor = 2;
            MinimumBeatLength = 120;
            EarlyActivationMilliseconds = 30;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            InternalChildren = new Drawable[]
            {
                main = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Size = new Vector2(40, 20),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        icon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.AngleDoubleRight,
                            Size = new Vector2(16),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                side = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("Gameplay/osu/repeat-edge-piece"),
                    Size = new Vector2(ArgonMainCirclePiece.OUTER_GRADIENT_SIZE),
                }
            };

            accentColour = hitObject.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(accent => icon.Colour = accent.NewValue.Darken(4), true);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!drawableRepeat.Judged)
            {
                main.ScaleTo(1.3f, 30, Easing.Out)
                    .Then()
                    .ScaleTo(1f, timingPoint.BeatLength / 2, Easing.Out);
                side
                    .MoveToX(-12, 30, Easing.Out)
                    .Then()
                    .MoveToX(0, timingPoint.BeatLength / 2, Easing.Out);
            }
        }
    }
}
