// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.MfMenu;
using osu.Game.Overlays.MfMenu.Header;
using osu.Game.Overlays.MfMenu.Sections;

namespace osu.Game.Overlays
{
    public class MfMenuOverlay : FullscreenOverlay<MfMenuHeader>
    {
        private MfMenuSectionsContainer sectionContainer;
        private MfMenuHeaderTabControl tabControl;

        public MfMenuOverlay()
            : base(OverlayColourScheme.Blue1, new MfMenuHeader())
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background6
                },
                sectionContainer = new MfMenuSectionsContainer
                {
                    ExpandableHeader = Header,
                    FixedHeader = tabControl = new MfMenuHeaderTabControl
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    HeaderBackground = new Box
                    {
                        Colour = ColourProvider.Background6,
                        RelativeSizeAxes = Axes.Both
                    },
                },
            };
        }

        public string[] SectionsOrder =
        {
            "Introduce",
            "Faq"
        };

        public MfMenuSection[] Sections =
        {
            new MfMenuIntroduceSection(),
            new MfMenuFaqSection(),
        };

        protected override void LoadComplete()
        {
            foreach (var o in SectionsOrder)
            {
                var sec = Sections.FirstOrDefault(s => s.SectionId == o);
                this.Schedule(() =>
                {
                    tabControl.AddItem(sec);
                    sectionContainer.Add(sec);
                });
            }

            tabControl.Current.BindValueChanged(OnSelectedTabTypeChanged);
            sectionContainer.SelectedSection.BindValueChanged(OnSelectedSectionChanged);
            base.LoadComplete();
        }

        private MfMenuSection last;

        private void OnSelectedTabTypeChanged(ValueChangedEvent<MfMenuSection> tab)
        {
            if ( last != tab.NewValue )
            {
                last = tab.NewValue;
                sectionContainer.ScrollTo(tab.NewValue);
            }
        }

        private void OnSelectedSectionChanged(ValueChangedEvent<MfMenuSection> s)
        {
            last = s.NewValue;
            tabControl.Current.Value = s.NewValue;
        }
    }
}
