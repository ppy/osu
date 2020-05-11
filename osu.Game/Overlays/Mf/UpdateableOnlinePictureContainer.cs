using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.Mf
{
    public class UpdateableOnlinePictureContainer : Container
    {
        protected string Uri;

        private DelayedLoadWrapper delayedLoadWrapper;

        public UpdateableOnlinePictureContainer(string uri)
        {
            this.Uri = uri;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Add(delayedLoadWrapper = new DelayedLoadWrapper(
                new UpdateableOnlinePicture(Uri)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                })
            );
        }

        public void UpdateImage(string NewUri)
        {
            Uri = NewUri;

            Remove(delayedLoadWrapper);
            delayedLoadWrapper = null;
            LoadComponentAsync(delayedLoadWrapper = new DelayedLoadWrapper(
            new UpdateableOnlinePicture(Uri)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Alpha = 0
            }), loaded =>
            {
                Add(delayedLoadWrapper);
                delayedLoadWrapper.FadeIn(500, Easing.OutQuint);
            } );
        }
    }
}