using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.OnlinePicture
{
    public class UpdateableOnlinePictureContainer : Container
    {
        protected string Url;

        private DelayedLoadWrapper delayedLoadWrapper;
        private UpdateableOnlinePicture pict;

        public UpdateableOnlinePictureContainer(string Url)
        {
            this.Url = Url;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            UpdateImage(Url);
        }

        public void UpdateImage(string NewUrl)
        {
            Url = NewUrl;

            Remove(delayedLoadWrapper);
            delayedLoadWrapper = null;
            LoadComponentAsync(delayedLoadWrapper = new DelayedLoadWrapper(
                pict = new UpdateableOnlinePicture(Url)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit
                })
            );

            pict.OnLoadComplete += d =>
            {
                d.Hide();
                d.FadeInFromZero(500, Easing.Out);
            };
        }
    }
}