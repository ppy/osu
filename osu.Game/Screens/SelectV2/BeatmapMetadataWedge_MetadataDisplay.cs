// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class MetadataDisplay : FillFlowContainer
        {
            private readonly OsuSpriteText labelText;
            private readonly Container contentContainer;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public MetadataDisplay(LocalisableString label)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Padding = new MarginPadding { Right = 10 };

                InternalChildren = new Drawable[]
                {
                    labelText = new OsuSpriteText
                    {
                        Text = label,
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                    },
                    contentContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = OsuFont.Style.Caption1.Size,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                labelText.Colour = colourProvider.Content1;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                SetHyphen();
            }

            public void SetHyphen()
            {
                contentContainer.Child = new TruncatingSpriteText
                {
                    RelativeSizeAxes = Axes.X,
                    Font = OsuFont.Style.Caption1,
                    Text = "-",
                    Colour = colourProvider.Content1,
                };
            }

            public void SetText(string? text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    SetHyphen();
                    return;
                }

                contentContainer.Child = new MetadataLinkContainer
                {
                    Font = OsuFont.Style.Caption1,
                    Text = text,
                };
            }

            public void SetUser(IUser? user)
            {
                if (string.IsNullOrEmpty(user?.Username))
                {
                    SetHyphen();
                    return;
                }

                contentContainer.Child = new UserLinkContainer
                {
                    Font = OsuFont.Style.Caption1,
                    User = user,
                };
            }

            public void SetDate(DateTimeOffset? date)
            {
                if (!date.HasValue)
                {
                    SetHyphen();
                    return;
                }

                contentContainer.Child = new DrawableDate(date.Value, OsuFont.Style.Caption1.Size, false);
            }

            public void SetTags(string[] tags)
            {
                if (!tags.Any())
                {
                    SetHyphen();
                    return;
                }

                contentContainer.Child = new TagsLine { Tags = tags };
            }

            public void SetLoading()
            {
                contentContainer.Child = new LoadingSpinner
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(10),
                    Margin = new MarginPadding { Top = 3f },
                    State = { Value = Visibility.Visible },
                };
            }

            protected override void Update()
            {
                base.Update();

                if (contentContainer.Child is TruncatingSpriteText text)
                    text.MaxWidth = ChildSize.X;
                else if (contentContainer.Child is MetadataLinkContainer link)
                    link.MaxWidth = ChildSize.X;
            }
        }
    }
}
