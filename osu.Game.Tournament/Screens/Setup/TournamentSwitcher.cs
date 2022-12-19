// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.IO;

namespace osu.Game.Tournament.Screens.Setup
{
    internal partial class TournamentSwitcher : ActionableInfo
    {
        private OsuDropdown<string> dropdown;
        private OsuButton folderButton;

        [Resolved]
        private TournamentGameBase game { get; set; }

        [BackgroundDependencyLoader]
        private void load(TournamentStorage storage)
        {
            string startupTournament = storage.CurrentTournament.Value;

            dropdown.Current = storage.CurrentTournament;
            dropdown.Items = storage.ListTournaments();
            dropdown.Current.BindValueChanged(v => Button.Enabled.Value = v.NewValue != startupTournament, true);

            Action = () => game.AttemptExit();
            folderButton.Action = () => storage.PresentExternally();

            ButtonText = "Close osu!";
        }

        protected override Drawable CreateComponent()
        {
            var drawable = base.CreateComponent();

            FlowContainer.Insert(-1, folderButton = new RoundedButton
            {
                Text = "Open folder",
                Width = 100
            });

            FlowContainer.Insert(-2, dropdown = new OsuDropdown<string>
            {
                Width = 510
            });

            return drawable;
        }
    }
}
