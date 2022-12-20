// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    public partial class OsuSpriteText : SpriteText
    {
        public bool NoFontAutoUpdate;

        public OsuSpriteText()
        {
            Shadow = true;
            Font = OsuFont.Default;
        }

        /*
        [Resolved]
        private CustomFontHelper helper { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (helper != null && !NoFontAutoUpdate)
                helper.OnFontChanged += updateTypeface;
        }

        private void updateTypeface()
        {
            if (Font.Family != "Venera")
                Font = new FontUsage(OsuFont.GetCustomTypeface(), Font.Size, Font.Weight, Font.Italics, Font.FixedWidth);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (helper != null)
                helper.OnFontChanged -= updateTypeface;

            base.Dispose(isDisposing);
        }
        */
    }
}
