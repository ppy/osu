// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    internal class KiaiFlashingSprite : BeatSyncedContainer
    {
        private readonly Sprite mainSprite;
        private readonly Sprite flashingSprite;

        public Texture Texture
        {
            set
            {
                mainSprite.Texture = value;
                flashingSprite.Texture = value;
            }
        }

        private const float flash_opacity = 0.3f;

        public KiaiFlashingSprite()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                mainSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                flashingSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                }
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            flashingSprite
                .FadeTo(flash_opacity)
                .Then()
                .FadeOut(timingPoint.BeatLength * 0.75f);
        }
    }
}
