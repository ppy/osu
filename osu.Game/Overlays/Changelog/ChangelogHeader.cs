// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : OverlayHeader
    {
        public Action ListingSelected;

        private const string listing_string = "Listing";

        public ChangelogHeader()
        {
            TabControl.AddItem(listing_string);
            TabControl.Current.ValueChanged += e =>
            {
                if (e.NewValue == listing_string)
                    ListingSelected?.Invoke();
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TabControl.AccentColour = colours.Violet;
        }

        private APIChangelogBuild displayedBuild;

        private ChangelogHeaderTitle title;

        public void ShowBuild(APIChangelogBuild build)
        {
            hideBuildTab();

            displayedBuild = build;

            TabControl.AddItem(build.ToString());
            TabControl.Current.Value = build.ToString();

            title.Version = build.UpdateStream.DisplayName;
        }

        public void ShowListing()
        {
            hideBuildTab();

            title.Version = null;
        }

        private void hideBuildTab()
        {
            if (displayedBuild != null)
            {
                TabControl.RemoveItem(displayedBuild.ToString());
                displayedBuild = null;
            }
        }

        protected override Drawable CreateBackground() => new HeaderBackground();

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                // todo: move badge display here
            }
        };

        protected override ScreenTitle CreateTitle() => title = new ChangelogHeaderTitle();

        public class HeaderBackground : Sprite
        {
            public HeaderBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/changelog");
            }
        }

        private class ChangelogHeaderTitle : ScreenTitle
        {
            public string Version
            {
                set => Section = value ?? listing_string;
            }

            public ChangelogHeaderTitle()
            {
                Title = "Changelog";
                Version = null;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Violet;
            }

            protected override Drawable CreateIcon() => new ChangelogIcon();

            internal class ChangelogIcon : CompositeDrawable
            {
                private const float circle_allowance = 0.8f;

                [BackgroundDependencyLoader]
                private void load(TextureStore textures, OsuColour colours)
                {
                    Size = new Vector2(ICON_SIZE / circle_allowance);

                    InternalChildren = new Drawable[]
                    {
                        new CircularContainer
                        {
                            Masking = true,
                            BorderColour = colours.Violet,
                            BorderThickness = 3,
                            MaskingSmoothness = 1,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Texture = textures.Get(@"Icons/changelog"),
                                    Size = new Vector2(circle_allowance),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colours.Violet,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                    };
                }
            }
        }
    }
}
