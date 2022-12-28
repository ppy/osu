// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    public partial class ButtonWithKeyBind : SettingsButton
    {
        private readonly Key key;

        public ButtonWithKeyBind(Key key)
        {
            this.key = key;
        }

        public new LocalisableString Text
        {
            get => base.Text;
            set => base.Text = $"{value} (Press {key.ToString().Replace("Number", string.Empty)})";
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == key)
            {
                TriggerClick();
                return true;
            }

            return base.OnKeyDown(e);
        }

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Height = 100;
            SpriteText.Colour = overlayColourProvider.Background6;
            SpriteText.Font = OsuFont.TorusAlternate.With(size: 34);

            Triangles?.Hide();
        }
    }
}
