// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.IO;

namespace osu.Game.Tournament.Screens.Setup
{
    internal partial class TournamentSwitcher : ActionableInfo
    {
        private OsuDropdown<string> dropdown = null!;
        private OsuButton folderButton = null!;
        private OsuButton reloadTournamentsButton = null!;

        [Resolved]
        private TournamentGameBase game { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(TournamentStorage storage)
        {
            string startupTournament = storage.CurrentTournament.Value;

            dropdown.Current = storage.CurrentTournament;
            dropdown.Items = storage.ListTournaments();
            dropdown.Current.BindValueChanged(v => Button.Enabled.Value = v.NewValue != startupTournament, true);

            reloadTournamentsButton.Action = () => dropdown.Items = storage.ListTournaments();

            Action = () =>
            {
                game.RestartAppWhenExited();
                game.AttemptExit();
            };
            folderButton.Action = () => storage.PresentExternally();

            ButtonText = "Close osu!";
        }

        protected override Drawable CreateComponent()
        {
            var drawable = base.CreateComponent();

            FlowContainer.Insert(-1, folderButton = new RoundedButton
            {
                Text = "Open folder",
                Width = BUTTON_SIZE
            });

            FlowContainer.Insert(-2, reloadTournamentsButton = new RoundedButton
            {
                Text = "Refresh",
                Width = BUTTON_SIZE
            });

            FlowContainer.Insert(-3, dropdown = new OsuDropdown<string>
            {
                Width = 510
            });

            return drawable;
        }
    }
}
