﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class TaikoMascotAnimation : BeatSyncedContainer
    {
        private readonly TextureAnimation textureAnimation;

        private int currentFrame;

        public double DisplayTime;

        public TaikoMascotAnimation(TaikoMascotAnimationState state)
        {
            InternalChild = textureAnimation = createTextureAnimation(state).With(animation =>
            {
                animation.Origin = animation.Anchor = Anchor.BottomLeft;
                animation.Scale = new Vector2(0.51f); // close enough to stable
            });

            RelativeSizeAxes = Axes.Both;
            Origin = Anchor = Anchor.BottomLeft;

            // needs to be always present to prevent the animation clock consuming time spent when not present.
            AlwaysPresent = true;
        }

        public bool Completed => !textureAnimation.IsPlaying || textureAnimation.PlaybackPosition >= textureAnimation.Duration;

        public override void Show()
        {
            base.Show();
            DisplayTime = Time.Current;
            textureAnimation.Seek(0);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            // assume that if the animation is playing on its own, it's independent from the beat and doesn't need to be touched.
            if (textureAnimation.FrameCount == 0 || textureAnimation.IsPlaying)
                return;

            textureAnimation.GotoFrame(currentFrame);
            currentFrame = (currentFrame + 1) % textureAnimation.FrameCount;
        }

        private static TextureAnimation createTextureAnimation(TaikoMascotAnimationState state)
        {
            switch (state)
            {
                case TaikoMascotAnimationState.Clear:
                    return new ClearMascotTextureAnimation();

                case TaikoMascotAnimationState.Idle:
                case TaikoMascotAnimationState.Kiai:
                case TaikoMascotAnimationState.Fail:
                    return new ManualMascotTextureAnimation(state);

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), $"Mascot animations for state {state} are not supported");
            }
        }

        private class ManualMascotTextureAnimation : TextureAnimation
        {
            private readonly TaikoMascotAnimationState state;

            public ManualMascotTextureAnimation(TaikoMascotAnimationState state)
            {
                this.state = state;

                IsPlaying = false;
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                for (int frameIndex = 0; true; frameIndex++)
                {
                    var texture = getAnimationFrame(skin, state, frameIndex);

                    if (texture == null)
                        break;

                    AddFrame(texture);
                }
            }
        }

        private class ClearMascotTextureAnimation : TextureAnimation
        {
            private const float clear_animation_speed = 1000 / 10f;

            private static readonly int[] clear_animation_sequence = { 0, 1, 2, 3, 4, 5, 6, 5, 6, 5, 4, 3, 2, 1, 0 };

            public ClearMascotTextureAnimation()
            {
                DefaultFrameLength = clear_animation_speed;
                Loop = false;
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                foreach (var frameIndex in clear_animation_sequence)
                {
                    var texture = getAnimationFrame(skin, TaikoMascotAnimationState.Clear, frameIndex);

                    if (texture == null)
                        // as per https://osu.ppy.sh/help/wiki/Skinning/osu!taiko#pippidon
                        break;

                    AddFrame(texture);
                }
            }
        }

        private static Texture getAnimationFrame(ISkin skin, TaikoMascotAnimationState state, int frameIndex)
            => skin.GetTexture($"pippidon{state.ToString().ToLower()}{frameIndex}");
    }
}
