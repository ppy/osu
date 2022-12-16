using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.OnlinePicture
{
    [LongRunningLoad]
    public partial class UpdateableOnlinePicture : Sprite
    {
        private string TargetURI;

        public UpdateableOnlinePicture(string target)
        {
            this.TargetURI = target;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            if ( TargetURI != null )
                Texture = textures.Get(TargetURI);
        }
    }
}
