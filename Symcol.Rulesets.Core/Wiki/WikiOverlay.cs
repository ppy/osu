using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using System.Linq;

namespace Symcol.Rulesets.Core.Wiki
{
    public abstract class WikiOverlay : WaveOverlayContainer
    {
        protected abstract WikiHeader Header { get; }
        protected abstract WikiSection[] Sections { get; }

        private WikiSection lastSection;
        private SectionsContainer<WikiSection> sectionsContainer;
        private WikiTabControl tabs;

        public const float CONTENT_X_MARGIN = 100;

        public WikiOverlay()
        {
            FirstWaveColour = OsuColour.Gray(0.4f);
            SecondWaveColour = OsuColour.Gray(0.3f);
            ThirdWaveColour = OsuColour.Gray(0.2f);
            FourthWaveColour = OsuColour.Gray(0.1f);

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

            tabs = new WikiTabControl
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Height = 30
            };

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f)
            });

            Add(sectionsContainer = new SectionsContainer<WikiSection>
            {
                RelativeSizeAxes = Axes.Both,
                ExpandableHeader = Header,
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

            foreach (WikiSection sec in Sections)
            {
                if (sec != null)
                {
                    sectionsContainer.Add(sec);
                    tabs.AddItem(sec);
                }
            }

            sectionsContainer.ScrollToTop();
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.5f, APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, DISAPPEAR_DURATION, Easing.Out);
        }

        private class WikiTabControl : PageTabControl<WikiSection>
        {
            public WikiTabControl()
            {
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
