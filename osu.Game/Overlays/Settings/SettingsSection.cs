// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsSection : Container, IHasFilterableChildren
    {
        protected FillFlowContainer FlowContent;
        protected override Container<Drawable> Content => FlowContent;

        private IBindable<SettingsSection> selectedSection;

        private Container content;

        public abstract Drawable CreateIcon();
        public abstract LocalisableString Header { get; }

        public IEnumerable<IFilterable> FilterableChildren => Children.OfType<IFilterable>();
        public virtual IEnumerable<string> FilterTerms => new[] { Header.ToString() };

        private const int header_size = 26;
        private const int margin = 20;
        private const int border_size = 2;

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        [Resolved]
        private SettingsPanel settingsPanel { get; set; }

        protected SettingsSection()
        {
            Margin = new MarginPadding { Top = margin };
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            FlowContent = new FillFlowContainer
            {
                Margin = new MarginPadding
                {
                    Top = header_size
                },
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.X,
                    Height = border_size,
                },
                content = new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = margin + border_size,
                        Bottom = 10,
                    },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: header_size),
                            Text = Header,
                            Colour = colours.Yellow,
                            Margin = new MarginPadding
                            {
                                Left = SettingsPanel.CONTENT_MARGINS,
                                Right = SettingsPanel.CONTENT_MARGINS
                            }
                        },
                        FlowContent
                    }
                },
            });

            selectedSection = settingsPanel.CurrentSection.GetBoundCopy();
            selectedSection.BindValueChanged(selected =>
            {
                if (selected.NewValue == this)
                    content.FadeIn(500, Easing.OutQuint);
                else
                    content.FadeTo(0.25f, 500, Easing.OutQuint);
            }, true);
        }

        private bool isCurrentSection => selectedSection.Value == this;

        protected override bool OnHover(HoverEvent e)
        {
            if (!isCurrentSection)
                content.FadeTo(0.6f, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!isCurrentSection)
                content.FadeTo(0.25f, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!isCurrentSection)
                settingsPanel.SectionsContainer.ScrollTo(this);

            return base.OnClick(e);
        }

        protected override bool ShouldBeConsideredForInput(Drawable child)
        {
            return isCurrentSection;
        }
    }
}
