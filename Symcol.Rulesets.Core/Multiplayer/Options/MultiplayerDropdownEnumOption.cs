using osu.Framework.Configuration;
using osu.Game.Overlays.Settings;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace Symcol.Rulesets.Core.Multiplayer.Options
{
    public class MultiplayerDropdownEnumOption<T> : MultiplayerOption
        where T : struct
    {
        public readonly Bindable<T> BindableEnum;

        public MultiplayerDropdownEnumOption(Bindable<T> bindable, string name, int quadrant, bool sync = true) : base(name, quadrant, sync)
        {
            BindableEnum = bindable;

            OptionContainer.Child = new BetterSettingsEnumDropdown<T>
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.X,
                Bindable = bindable,
            };
        }

        private class BetterSettingsEnumDropdown<T> : SettingsEnumDropdown<T>
        {
            protected override Drawable CreateControl() => new BetterOsuEnumDropdown<T>
            {
                Margin = new MarginPadding { Top = 5 },
                RelativeSizeAxes = Axes.X,
            };

            private class BetterOsuEnumDropdown<T> : OsuEnumDropdown<T>
            {
                public BetterOsuEnumDropdown()
                {
                    Menu.MaxHeight = 160;
                }
            }
        }
    }
}
