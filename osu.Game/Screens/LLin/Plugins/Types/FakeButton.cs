using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Screens.LLin.Plugins.Types
{
    public class FakeButton : IFunctionProvider
    {
        public Vector2 Size { get; set; } = new Vector2(30);
        public Action Action { get; set; }
        public IconUsage Icon { get; set; }
        public LocalisableString Title { get; set; }
        public LocalisableString Description { get; set; }
        public FunctionType Type { get; set; }

        public void Active()
        {
            Action?.Invoke();
        }
    }
}
