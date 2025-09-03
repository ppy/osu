// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Tournament.Screens.Setup
{
    internal partial class LanguageSwitcher : ActionableInfo
    {
        private OsuEnumDropdown<Language> dropdown = null!;

        [Resolved]
        private TournamentGameBase game { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            dropdown.Current = game.CurrentLanguage;
        }

        protected override Drawable CreateComponent()
        {
            var drawable = base.CreateComponent();

            FlowContainer.Remove(FlowContainer.Children.Last(), true);
            FlowContainer.Insert(-1, dropdown = new OsuEnumDropdown<Language>
            {
                Width = 510
            });

            return drawable;
        }
    }
}
