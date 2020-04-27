// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class TaikoMascotTextureAnimation : TextureAnimation
    {
        private const float clear_animation_speed = 1000 / 10f;
        private static readonly int[] clear_animation_sequence = { 0, 1, 2, 3, 4, 5, 6, 5, 6, 5, 4, 3, 2, 1, 0 };
        private int currentFrame;

        public TaikoMascotAnimationState State { get; }

        public TaikoMascotTextureAnimation(TaikoMascotAnimationState state)
            : base(true)
        {
            State = state;

            // We're animating on beat if it's not the clear animation
            if (state == TaikoMascotAnimationState.Clear)
                DefaultFrameLength = clear_animation_speed;
            else
                this.Stop();

            Origin = Anchor.BottomLeft;
            Anchor = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            if (State == TaikoMascotAnimationState.Clear)
            {
                foreach (var textureIndex in clear_animation_sequence)
                {
                    if (!addFrame(skin, textureIndex))
                        break;
                }
            }
            else
            {
                for (int i = 0; true; i++)
                {
                    if (!addFrame(skin, i))
                        break;
                }
            }
        }

        private bool addFrame(ISkinSource skin, int textureIndex)
        {
            var textureName = getStateTextureName(textureIndex);
            var texture = skin.GetTexture(textureName);

            if (texture == null)
                return false;

            AddFrame(texture);

            return true;
        }

        /// <summary>
        /// Advances the current frame by one.
        /// </summary>
        public void Move()
        {
            // Check whether there are frames before causing a crash.
            if (FrameCount == 0)
                return;

            if (currentFrame >= FrameCount)
                currentFrame = 0;

            GotoFrame(currentFrame);

            currentFrame += 1;
        }

        private string getStateTextureName(int i) => $"pippidon{getStateString(State)}{i}";

        private string getStateString(TaikoMascotAnimationState state)
        {
            switch (state)
            {
                case TaikoMascotAnimationState.Clear:
                    return "clear";

                case TaikoMascotAnimationState.Fail:
                    return "fail";

                case TaikoMascotAnimationState.Idle:
                    return "idle";

                case TaikoMascotAnimationState.Kiai:
                    return "kiai";

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), $"There's no case for animation state {state} available");
            }
        }
    }
}
