// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.IO;

namespace osu.Game.Tournament.Screens.Setup
{
    internal class TournamentSwitcher : ActionableInfo
    {
        private OsuDropdown<string> dropdown;

        [Resolved]
        private TournamentGameBase game { get; set; }

        [BackgroundDependencyLoader]
        private void load(TournamentStorage storage)
        {
            string startupTournament = storage.CurrentTournament.Value;

            dropdown.Current = storage.CurrentTournament;
            dropdown.Items = storage.ListTournaments();
            dropdown.Current.BindValueChanged(v => Button.Enabled.Value = v.NewValue != startupTournament, true);

            Action = () => game.GracefullyExit();

            ButtonText = "Close osu!";
        }

        protected override Drawable CreateComponent()
        {
            var drawable = base.CreateComponent();

            FlowContainer.Insert(-1, dropdown = new OsuDropdown<string>
            {
                Width = 510
            });

            return drawable;
        }
    }
}
