using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class DefaultTaikoDonTextureAnimation : TextureAnimation
    {
        private readonly TaikoDonAnimationState _state;
        private int currentFrame;

        public DefaultTaikoDonTextureAnimation(TaikoDonAnimationState state) : base(false)
        {
            _state = state;
            this.Stop();

            Origin = Anchor.BottomLeft;
            Anchor = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            for (int i = 0;; i++)
            {
                var textureName = $"pippidon{_getStateString(_state)}{i}";
                Texture texture = skin.GetTexture(textureName);

                if (texture == null)
                    break;

                AddFrame(texture);
            }
        }

        /// <summary>
        /// Advances the current frame by one.
        /// </summary>
        public void Move()
        {
            if (FrameCount <= currentFrame)
                currentFrame = 0;

            GotoFrame(currentFrame);

            currentFrame++;
        }

        private string _getStateString(TaikoDonAnimationState state) => state switch
        {
            TaikoDonAnimationState.Clear => "clear",
            TaikoDonAnimationState.Fail  => "fail",
            TaikoDonAnimationState.Idle  => "idle",
            TaikoDonAnimationState.Kiai  => "kiai",
            _ => null
        };
    }
}
