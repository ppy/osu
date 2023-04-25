// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Wiki;

public partial class WikiLanguageDropdown : OsuDropdown<Language>
{
    public BindableList<Language> AvailableLanguages = new BindableList<Language>();

    private Bindable<string> languageConfig = null!;

    public WikiLanguageDropdown()
    {
        Width = 300;
        AutoSizeAxes = Axes.Y;
        ItemSource = AvailableLanguages;
    }

    [BackgroundDependencyLoader]
    private void load(FrameworkConfigManager frameworkConfig, LocalisationManager localisation)
    {
        languageConfig = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
    }

    protected override void LoadComplete()
    {
        languageConfig.BindValueChanged(s =>
        {
            if (LanguageExtensions.TryParseCultureCode(s.NewValue, out var language))
            {
                Current.Value = language;
            }
        }, true);
    }

    public void UpdateDropDown(APIWikiPage page)
    {
        AvailableLanguages.Clear();

        foreach (string langString in page.AvailableLocales)
        {
            if (LanguageExtensions.TryParseCultureCode(langString, out var language))
            {
                AvailableLanguages.Add(language);
            }
        }

        if (LanguageExtensions.TryParseCultureCode(page.Locale, out var pageLanguage))
        {
            Current.Value = pageLanguage;
        }
    }
}
