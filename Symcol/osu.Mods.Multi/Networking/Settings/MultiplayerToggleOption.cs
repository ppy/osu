using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using OpenTK;

namespace osu.Mods.Multi.Networking.Settings
{
    public class MultiplayerToggleOption : MultiplayerOption
    {
        public readonly Bindable<bool> BindableBool;

        public MultiplayerToggleOption(Bindable<bool> bindable, string name, int quadrant, bool sync = true) : base(name, quadrant, sync)
        {
            BindableBool = bindable;

            Child = new SettingsCheckbox
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.X,
                Bindable = bindable,
                LabelText = " " + name,
                Position = new Vector2(-16, 24),
            };
        }
    }
}
