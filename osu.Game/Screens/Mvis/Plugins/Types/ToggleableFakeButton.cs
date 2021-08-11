using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osuTK;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    public class ToggleableFakeButton : IToggleableFunctionProvider
    {
        public Vector2 Size { get; set; } = new Vector2(30);
        public Action Action { get; set; }
        public IconUsage Icon { get; set; }
        public LocalisableString Title { get; set; }
        public LocalisableString Description { get; set; }
        public FunctionType Type { get; set; }

        public void Active()
        {
            if (!Bindable.Disabled)
                Bindable.Toggle();
            else
                Logger.Log($"无法更改 \"{Title}\" 的值, 因为此Bindable已禁用", level: LogLevel.Important);
        }

        public void Active(bool performAction)
        {
            if (performAction)
                Action?.Invoke();
            else
                Active();
        }

        public BindableBool Bindable { get; set; } = new BindableBool();
    }
}
