// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiLanguageDropdown : OsuDropdown<Language>
    {
        public BindableList<Language> AvailableLanguages = new BindableList<Language>();
        public event Action? DropDownUpdated;

        public WikiLanguageDropdown()
        {
            Width = 300;
            AutoSizeAxes = Axes.Y;
            ItemSource = AvailableLanguages;
        }

        public void UpdateDropdown(string[] availableLocales)
        {
            AvailableLanguages.Clear();

            foreach (string locale in availableLocales)
            {
                if (LanguageExtensions.TryParseCultureCode(locale, out var language))
                {
                    AvailableLanguages.Add(language);
                }
            }

            DropDownUpdated?.Invoke();
        }
    }
}
