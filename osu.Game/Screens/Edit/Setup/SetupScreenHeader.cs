// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Setup
{
    internal class SetupScreenHeader : OverlayHeader
    {
        public SetupScreenHeaderBackground Background { get; private set; }

        [Resolved]
        private SectionsContainer<SetupSection> sections { get; set; }

        private SetupScreenTabControl tabControl;

        protected override OverlayTitle CreateTitle() => new SetupScreenTitle();

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            // reverse flow is used to ensure that the tab control's expandable bars extend over the background chooser.
            Child = new ReverseChildIDFillFlowContainer<Drawable>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    tabControl = new SetupScreenTabControl
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 30
                    },
                    Background = new SetupScreenHeaderBackground
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 120
                    }
                }
            }
        };

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            tabControl.AccentColour = colourProvider.Highlight1;
            tabControl.BackgroundColour = colourProvider.Dark5;

            foreach (var section in sections)
                tabControl.AddItem(section);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sections.SelectedSection.BindValueChanged(section => tabControl.Current.Value = section.NewValue);
            tabControl.Current.BindValueChanged(section =>
            {
                if (section.NewValue != sections.SelectedSection.Value)
                    sections.ScrollTo(section.NewValue);
            });
        }

        private class SetupScreenTitle : OverlayTitle
        {
            public SetupScreenTitle()
            {
                Title = "beatmap setup";
                Description = "change general settings of your beatmap";
                IconTexture = "Icons/Hexacons/social";
            }
        }

        internal class SetupScreenTabControl : OverlayTabControl<SetupSection>
        {
            private readonly Box background;

            public Color4 BackgroundColour
            {
                get => background.Colour;
                set => background.Colour = value;
            }

            public SetupScreenTabControl()
            {
                TabContainer.Margin = new MarginPadding { Horizontal = RoundedContentEditorScreen.HORIZONTAL_PADDING };

                AddInternal(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1
                });
            }

            protected override TabItem<SetupSection> CreateTabItem(SetupSection value) => new SetupScreenTabItem(value)
            {
                AccentColour = AccentColour
            };

            private class SetupScreenTabItem : OverlayTabItem
            {
                public SetupScreenTabItem(SetupSection value)
                    : base(value)
                {
                    Text.Text = value.Title;
                }
            }
        }
    }
}
