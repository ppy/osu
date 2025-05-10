// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Settings
{
    public abstract partial class SettingsSection : Container, IFilterable
    {
        protected FillFlowContainer FlowContent;
        protected override Container<Drawable> Content => FlowContent;

        private IBindable<SettingsSection> selectedSection;

        private Box dim;

        private const float inactive_alpha = 0.8f;

        public abstract Drawable CreateIcon();
        public abstract LocalisableString Header { get; }

        public virtual IEnumerable<LocalisableString> FilterTerms => new[] { Header };

        public const int ITEM_SPACING = 14;

        private const int header_size = 24;
        private const int border_size = 4;

        private bool matchingFilter = true;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                bool wasPresent = IsPresent;

                matchingFilter = value;

                if (IsPresent != wasPresent)
                    Invalidate(Invalidation.Presence);
            }
        }

        public override bool IsPresent => base.IsPresent && MatchingFilter;

        public bool FilteringActive { get; set; }

        [Resolved(canBeNull: true)]
        private SettingsPanel settingsPanel { get; set; }

        protected SettingsSection()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            FlowContent = new FillFlowContainer
            {
                Margin = new MarginPadding
                {
                    Top = 36
                },
                Spacing = new Vector2(0, ITEM_SPACING),
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Name = "separator",
                    Colour = colourProvider.Background6,
                    RelativeSizeAxes = Axes.X,
                    Height = border_size,
                },
                new Container
                {
                    Padding = new MarginPadding { Top = border_size },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding
                            {
                                Top = 24,
                                Bottom = 40,
                            },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = OsuFont.TorusAlternate.With(size: header_size),
                                    Text = Header,
                                    Margin = new MarginPadding
                                    {
                                        Horizontal = SettingsPanel.CONTENT_MARGINS
                                    }
                                },
                                FlowContent
                            }
                        },
                        dim = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                            Alpha = inactive_alpha,
                        },
                    }
                },
            });

            selectedSection = settingsPanel?.CurrentSection.GetBoundCopy() ?? new Bindable<SettingsSection>(this);
            selectedSection.BindValueChanged(_ => updateContentFade(), true);
        }

        private bool isCurrentSection => selectedSection.Value == this;

        protected override bool OnHover(HoverEvent e)
        {
            updateContentFade();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateContentFade();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!isCurrentSection)
            {
                Debug.Assert(settingsPanel != null);
                settingsPanel.SectionsContainer.ScrollTo(this);
            }

            return base.OnClick(e);
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) =>
            // only the current section should accept input.
            // this provides the behaviour of the first click scrolling the target section to the centre of the screen.
            isCurrentSection;

        private void updateContentFade()
        {
            float dimFade = 0;

            if (!isCurrentSection)
            {
                dimFade = IsHovered ? 0.5f : inactive_alpha;
            }

            dim.FadeTo(dimFade, 300, Easing.OutQuint);
        }
    }
}
