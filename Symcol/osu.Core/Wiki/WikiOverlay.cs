using System.Collections.Generic;
using System.Linq;
using osu.Core.Config;
using osu.Core.Wiki.Header;
using osu.Core.Wiki.Sections;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using OpenTK.Graphics;

namespace osu.Core.Wiki
{
    //The wiki is my GREATEST POWER!
    public class WikiOverlay : WaveOverlayContainer
    {
        public const float CONTENT_X_MARGIN = 80;

        private readonly Bindable<WikiSet> currentWikiSet = new Bindable<WikiSet>();

        private List<WikiSection> sections = new List<WikiSection>();

        private WikiSection lastSection;

        private OsuGame game;

        public WikiOverlay()
        {
            Waves.FirstWaveColour = OsuColour.Gray(0.4f);
            Waves.SecondWaveColour = OsuColour.Gray(0.3f);
            Waves.ThirdWaveColour = OsuColour.Gray(0.2f);
            Waves.FourthWaveColour = OsuColour.Gray(0.1f);

            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Width = 0.85f;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Masking = true;
            AlwaysPresent = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Radius = 10
            };

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f)
            });

            WikiHeader header = new WikiHeader();
            WikiTabControl tabs = new WikiTabControl();
            SectionsContainer<WikiSection> sectionsContainer;

            Add(sectionsContainer = new SectionsContainer<WikiSection>
            {
                RelativeSizeAxes = Axes.Both,
                ExpandableHeader = header,
                FixedHeader = tabs,
                HeaderBackground = new Box
                {
                    Colour = OsuColour.Gray(34),
                    RelativeSizeAxes = Axes.Both
                }
            });

            sectionsContainer.SelectedSection.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s;
                    tabs.Current.Value = lastSection;
                }
            };

            tabs.Current.ValueChanged += s =>
            {
                if (lastSection == null)
                {
                    lastSection = sectionsContainer.Children.FirstOrDefault();
                    if (lastSection != null)
                        tabs.Current.Value = lastSection;
                    return;
                }
                if (lastSection != s)
                {
                    lastSection = s;
                    sectionsContainer.ScrollTo(lastSection);
                }
            };

            currentWikiSet.BindTo(header.CurrentWikiSet);

            currentWikiSet.ValueChanged += value =>
            {
                foreach (WikiSection s in sections)
                {
                    sectionsContainer.Remove(s);
                }
                sections = new List<WikiSection>();

                lastSection = null;
                sectionsContainer.FixedHeader = null;
                tabs = new WikiTabControl();
                sectionsContainer.FixedHeader = tabs;

                tabs.Current.ValueChanged += s =>
                {
                    if (lastSection == null)
                    {
                        lastSection = sectionsContainer.Children.FirstOrDefault();
                        if (lastSection != null)
                            tabs.Current.Value = lastSection;
                        return;
                    }
                    if (lastSection != s)
                    {
                        lastSection = s;
                        sectionsContainer.ScrollTo(lastSection);
                    }
                };

                if (value.GetSections() != null)
                    foreach (WikiSection s in value.GetSections())
                    {
                        sections.Add(s);
                        sectionsContainer.Add(s);
                        tabs.AddItem(s);
                    }
                else
                    Logger.Log("\"" + value.Name + "\"" + " wiki sections are null, please report to the mod/ruleset creator!", LoggingTarget.Runtime, LogLevel.Error);
            };

            currentWikiSet.Value = header.Home;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game)
        {
            this.game = game;
        }

        private double doit;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (SymcolOsuModSet.SymcolConfigManager.Get<bool>(SymcolSetting.FreshInstall))
            {
                doit = Time.Current + 8000;
                SymcolOsuModSet.SymcolConfigManager.Set<bool>(SymcolSetting.FreshInstall, false);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= doit)
            {
                doit = double.MaxValue;
                Show();
            }
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.5f, WaveContainer.APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, WaveContainer.DISAPPEAR_DURATION, Easing.Out);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Padding = new MarginPadding { Top = game.ToolbarOffset };
        }

        private class WikiTabControl : PageTabControl<WikiSection>
        {
            public WikiTabControl()
            {
                RelativeSizeAxes = Axes.X;
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                Height = 30;

                TabContainer.RelativeSizeAxes &= ~Axes.X;
                TabContainer.AutoSizeAxes |= Axes.X;
                TabContainer.Anchor |= Anchor.x1;
                TabContainer.Origin |= Anchor.x1;
            }

            protected override TabItem<WikiSection> CreateTabItem(WikiSection value) => new WikiTabItem(value);

            protected override Dropdown<WikiSection> CreateDropdown() => null;

            private class WikiTabItem : PageTabItem
            {
                public WikiTabItem(WikiSection value) : base(value)
                {
                    Text.Text = value.Title;
                }
            }
        }
    }
}
