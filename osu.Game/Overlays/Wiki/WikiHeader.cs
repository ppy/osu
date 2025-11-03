// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiHeader : BreadcrumbControlOverlayHeader
    {
        public static LocalisableString IndexPageString => LayoutStrings.HeaderHelpIndex;

        private const string github_wiki_base = @"https://github.com/ppy/osu-wiki/blob/master/wiki";

        public readonly Bindable<APIWikiPage> WikiPageData = new Bindable<APIWikiPage>();

        public Action ShowIndexPage;
        public Action ShowParentPage;

        private readonly Bindable<string> githubPath = new Bindable<string>();

        public WikiHeader()
        {
            TabControl.AddItem(IndexPageString);
            Current.Value = IndexPageString;

            WikiPageData.BindValueChanged(onWikiPageChange);
            Current.BindValueChanged(onCurrentChange);
        }

        private void onWikiPageChange(ValueChangedEvent<APIWikiPage> e)
        {
            // Clear the path beforehand in case we got an error page.
            githubPath.Value = null;

            if (e.NewValue == null)
                return;

            TabControl.Clear();
            Current.Value = null;

            TabControl.AddItem(IndexPageString);
            githubPath.Value = $"{github_wiki_base}/{e.NewValue.Path}/{e.NewValue.Locale}.md";

            if (e.NewValue.Path == WikiOverlay.INDEX_PATH)
            {
                Current.Value = IndexPageString;
                return;
            }

            if (e.NewValue.Subtitle != null)
                TabControl.AddItem(e.NewValue.Subtitle);

            TabControl.AddItem(e.NewValue.Title);
            Current.Value = e.NewValue.Title;
        }

        protected override Drawable CreateTabControlContent()
        {
            return new FillFlowContainer
            {
                Height = 40,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new ShowOnGitHubButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(32),
                        TargetPath = { BindTarget = githubPath },
                    },
                },
            };
        }

        private void onCurrentChange(ValueChangedEvent<LocalisableString?> e)
        {
            if (e.NewValue == TabControl.Items.LastOrDefault())
                return;

            if (e.NewValue == IndexPageString)
            {
                ShowIndexPage?.Invoke();
                return;
            }

            ShowParentPage?.Invoke();
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/wiki");

        protected override OverlayTitle CreateTitle() => new WikiHeaderTitle();

        private partial class WikiHeaderTitle : OverlayTitle
        {
            public WikiHeaderTitle()
            {
                Title = PageTitleStrings.MainWikiControllerDefault;
                Description = NamedOverlayComponentStrings.WikiDescription;
                Icon = OsuIcon.Wiki;
            }
        }

        private partial class ShowOnGitHubButton : RoundedButton
        {
            public override LocalisableString TooltipText => WikiStrings.ShowEditLink;

            public readonly Bindable<string> TargetPath = new Bindable<string>();

            [BackgroundDependencyLoader(true)]
            private void load([CanBeNull] ILinkHandler linkHandler)
            {
                Width = 42;

                Add(new SpriteIcon
                {
                    Size = new Vector2(12),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Brands.Github,
                });

                Action = () => linkHandler?.HandleLink(TargetPath.Value);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                TargetPath.BindValueChanged(e =>
                {
                    this.FadeTo(e.NewValue != null ? 1 : 0);
                    Enabled.Value = e.NewValue != null;
                }, true);
            }
        }
    }
}
