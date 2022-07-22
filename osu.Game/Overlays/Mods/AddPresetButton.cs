// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class AddPresetButton : ShearedToggleButton
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public AddPresetButton()
            : base(1)
        {
            RelativeSizeAxes = Axes.X;
            Height = ModSelectPanel.HEIGHT;

            // shear will be applied at a higher level in `ModPresetColumn`.
            Content.Shear = Vector2.Zero;
            Padding = new MarginPadding();

            Text = "+";
            TextSize = 30;
        }

        protected override void UpdateActiveState()
        {
            DarkerColour = Active.Value ? colours.Orange1 : ColourProvider.Background3;
            LighterColour = Active.Value ? colours.Orange0 : ColourProvider.Background1;
            TextColour = Active.Value ? ColourProvider.Background6 : ColourProvider.Content1;
        }
    }
}
