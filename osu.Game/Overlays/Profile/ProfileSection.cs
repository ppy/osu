// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile
{
    public abstract partial class ProfileSection : Container
    {
        public abstract LocalisableString Title { get; }

        public abstract string Identifier { get; }

        private readonly FillFlowContainer<Drawable> content;
        private readonly Box background;
        private readonly Box underscore;

        protected override Container<Drawable> Content => content;

        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private const float outer_gutter_width = 10;

        protected ProfileSection()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0, 1),
                        Radius = 3,
                        Colour = Colour4.Black.Opacity(0.25f)
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding
                            {
                                Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING - outer_gutter_width,
                                Top = 20,
                                Bottom = 20,
                            },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = Title,
                                    Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                                },
                                underscore = new Box
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding { Top = 4 },
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2,
                                }
                            }
                        },
                        // reverse ID flow is required for correct Z-ordering of the content (last item should be front-most).
                        // particularly important in BeatmapsSection, as it uses beatmap cards, which have expandable overhanging content.
                        content = new ReverseChildIDFillFlowContainer<Drawable>
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Padding = new MarginPadding
                            {
                                Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING - outer_gutter_width,
                                Bottom = 20
                            }
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background4;
            underscore.Colour = colourProvider.Highlight1;
        }
    }
}
