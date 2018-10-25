using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Mods.Multi.Networking.Settings
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

        private class BetterSettingsEnumDropdown<J> : SettingsEnumDropdown<J>
            where J : struct
        {
            protected override Drawable CreateControl() => new BetterOsuEnumDropdown<J>
            {
                Margin = new MarginPadding { Top = 5 },
                RelativeSizeAxes = Axes.X,
            };

            private class BetterOsuEnumDropdown<I> : OsuEnumDropdown<I>
                where I : struct
            {
                public BetterOsuEnumDropdown()
                {
                    Menu.MaxHeight = 160;
                }
            }
        }
    }
}
