// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
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
                    var textureName = _getStateTextureName(textureIndex);
                    Texture texture = skin.GetTexture(textureName);

                    if (texture == null)
                        break;

                    AddFrame(texture);
                }
            }
            else
            {
                for (int i = 0; true; i++)
                {
                    var textureName = _getStateTextureName(i);
                    Texture texture = skin.GetTexture(textureName);

                    if (texture == null)
                        break;

                    AddFrame(texture);
                }
            }
        }

        /// <summary>Advances the current frame by one.</summary>
        public void Move()
        {
            if (FrameCount == 0) // Frames are apparently broken
                return;

            if (FrameCount <= currentFrame)
                currentFrame = 0;

            GotoFrame(currentFrame);

            currentFrame += 1;
        }

        private string _getStateTextureName(int i) => $"pippidon{_getStateString(State)}{i}";

        private string _getStateString(TaikoMascotAnimationState state)
        {
            return state switch
            {
                TaikoMascotAnimationState.Clear => "clear",
                TaikoMascotAnimationState.Fail => "fail",
                TaikoMascotAnimationState.Idle => "idle",
                TaikoMascotAnimationState.Kiai => "kiai",
                _ => null
            };
        }
    }
}
