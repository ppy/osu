using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Mvis.Skinning
{
    public class FullScreenSkinnableComponent : SkinnableComponent
    {
        protected override Vector2 DrawScale => new Vector2(Parent.DrawHeight / 768);

        public FullScreenSkinnableComponent(string textureName, Func<ISkinComponent, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.NoScaling, bool masking = false)
            : base(textureName, defaultImplementation, allowFallback, confineMode, masking)
        {
            Size = new Vector2(1366, 768);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.None;
            CentreComponent = false;
        }
    }
}
