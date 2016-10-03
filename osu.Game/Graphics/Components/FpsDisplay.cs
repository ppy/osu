//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Graphics.Components
{
    class FpsDisplay : OsuComponent
    {
        SpriteText fpsText;
        public override void Load()
        {
            base.Load();

            Add(fpsText = new SpriteText());

            fpsText.Text = "...";
        }

        protected override void Update()
        {
            fpsText.Text = ((int)(1000 / Clock.ElapsedFrameTime)).ToString();
            base.Update();
        }
    }
}
