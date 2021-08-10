// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogSupporterPromo : CompositeDrawable
    {
        private const float image_container_width = 164;

        private readonly LinkFlowContainer supportLinkText;
        private readonly TextFlowContainer supportNoteText;
        private readonly Container imageContainer;

        public ChangelogSupporterPromo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Vertical = 20,
                Horizontal = 50,
            };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 6,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Offset = new Vector2(0, 1),
                        Radius = 3,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.3f),
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                            Padding = new MarginPadding { Horizontal = 75 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Padding = new MarginPadding { Right = 50 + image_container_width },
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = ChangelogStrings.SupportHeading,
                                            Font = OsuFont.GetFont(size: 22, weight: FontWeight.Light),
                                            Margin = new MarginPadding { Bottom = 20 },
                                        },
                                        supportLinkText = new LinkFlowContainer(t =>
                                        {
                                            t.Font = t.Font.With(size: 17.5f);
                                        })
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        },
                                        supportNoteText = new TextFlowContainer(t =>
                                        {
                                            t.Font = t.Font.With(size: 15);
                                        })
                                        {
                                            Margin = new MarginPadding { Top = 10 },
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                        }
                                    },
                                },
                                imageContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = image_container_width,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                }
                            }
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, TextureStore textures)
        {
            void fontPinkColour(SpriteText t) => t.Colour = colour.PinkLighter;

            supportLinkText.AddText("Support further development of osu! and ", fontPinkColour);
            supportLinkText.AddLink("become an osu!supporter", "https://osu.ppy.sh/home/support", t =>
            {
                t.Colour = colour.PinkDark;
                t.Font = t.Font.With(weight: FontWeight.Bold);
            });
            supportLinkText.AddText(" today!", fontPinkColour);

            supportNoteText.AddText("Not only will you help speed development, but you will also get some extra features and customisations!", fontPinkColour);

            imageContainer.Children = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Online/supporter-pippi"),
                },
                new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 75,
                    Height = 75,
                    Margin = new MarginPadding { Top = 70 },
                    Texture = textures.Get(@"Online/supporter-heart"),
                },
            };
        }
    }
}
