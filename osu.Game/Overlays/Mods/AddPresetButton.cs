// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class AddPresetButton : ShearedToggleButton, IHasPopover
    {
        protected override bool PlayToggleSamples => false;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(mods => Enabled.Value = mods.NewValue.Any(), true);
            Enabled.BindValueChanged(enabled =>
            {
                if (!enabled.NewValue)
                    Active.Value = false;
            });
        }

        protected override void UpdateActiveState()
        {
            DarkerColour = Active.Value ? colours.Orange1 : ColourProvider.Background3;
            LighterColour = Active.Value ? colours.Orange0 : ColourProvider.Background1;
            TextColour = Active.Value ? ColourProvider.Background6 : ColourProvider.Content1;

            if (Active.Value)
                this.ShowPopover();
            else
                this.HidePopover();
        }

        public Popover GetPopover() => new AddPresetPopover(this);
    }
}
