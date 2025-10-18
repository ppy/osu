// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Users.Drawables;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiLanguageDropdown : OsuDropdown<Language>
    {
        private readonly BindableList<Language> availableLanguages = new BindableList<Language>();
        public event Action? DropDownUpdated;

        public WikiLanguageDropdown()
        {
            ItemSource = availableLanguages;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(l =>
            {
                if (Header is WikiLanguageDropdownHeader wikiHeader)
                {
                    wikiHeader.Language = l.NewValue;
                }
            }, true);
        }

        public void UpdateDropdown(string[] availableLocales)
        {
            availableLanguages.Clear();

            foreach (string locale in availableLocales)
            {
                if (LanguageExtensions.TryParseCultureCode(locale, out var language))
                {
                    availableLanguages.Add(language);
                }
            }

            DropDownUpdated?.Invoke();
        }

        protected override DropdownMenu CreateMenu() => new WikiLanguageDropdownMenu();

        protected override DropdownHeader CreateHeader() => new WikiLanguageDropdownHeader();

        private partial class WikiLanguageDropdownHeader : OsuDropdownHeader
        {
            private Language? language;

            public Language? Language
            {
                get => language;
                set
                {
                    if (language == value)
                        return;

                    language = value;

                    if (language == null)
                    {
                        flagContainer.Clear();
                        return;
                    }

                    flagContainer.Child = new DrawableFlag(language.Value.ToCountryCode())
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        FillMode = FillMode.Fit,
                        RelativeSizeAxes = Axes.Both
                    };
                }
            }

            private readonly Container flagContainer;

            public WikiLanguageDropdownHeader()
            {
                Foreground.Add(flagContainer = new Container
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                });

                Text.Padding = new MarginPadding { Left = 30f };
            }
        }

        private partial class WikiLanguageDropdownMenu : OsuDropdownMenu
        {
            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new WikiLanguageDropdownMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour
            };

            private partial class WikiLanguageDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                private MenuItemContent? menuItem;

                public WikiLanguageDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    if (item is DropdownMenuItem<Language> dropdownItem && menuItem != null)
                    {
                        menuItem.Language = dropdownItem.Value;
                    }
                }

                protected override Drawable CreateContent() => menuItem = new MenuItemContent();

                private partial class MenuItemContent : Content
                {
                    private readonly Container flagContainer;

                    private Language? language;

                    public Language? Language
                    {
                        get => language;
                        set
                        {
                            if (language == value)
                                return;

                            language = value;

                            if (language == null)
                            {
                                flagContainer.Clear();
                                return;
                            }

                            flagContainer.Child = new DrawableFlag(language.Value.ToCountryCode())
                            {
                                Origin = Anchor.CentreRight,
                                Anchor = Anchor.CentreRight,
                                FillMode = FillMode.Fit,
                                RelativeSizeAxes = Axes.Both
                            };
                        }
                    }

                    public MenuItemContent()
                    {
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        AddInternal(flagContainer = new Container
                        {
                            Origin = Anchor.CentreRight,
                            Anchor = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Horizontal = 5 }
                        });
                    }
                }
            }
        }
    }
}
