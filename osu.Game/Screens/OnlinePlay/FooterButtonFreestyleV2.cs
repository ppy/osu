// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreestyleV2 : ScreenFooterButton
    {
        public readonly Bindable<bool> Freestyle = new Bindable<bool>();

        public new Action Action
        {
            set => throw new NotSupportedException("The click action is handled by the button itself.");
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FooterButtonFreestyleV2()
        {
            // Overwrite any external behaviour as we delegate the main toggle action to a sub-button.
            base.Action = () => Freestyle.Value = !Freestyle.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text = "Freestyle";
            Icon = FontAwesome.Solid.ExchangeAlt;
            AccentColour = colours.Lime1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Freestyle.BindValueChanged(active =>
            {
                OverlayState.Value = active.NewValue ? Visibility.Visible : Visibility.Hidden;
            }, true);
        }
    }
}
