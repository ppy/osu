// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class MetadataDisplay : FillFlowContainer
        {
            private readonly OsuSpriteText labelText;
            private readonly OsuSpriteText contentText;
            private readonly OsuSpriteText contentLinkText;
            private readonly OsuHoverContainer contentLink;
            private readonly DrawableDate contentDate;
            private readonly TagsLine contentTags;
            private readonly LoadingSpinner contentLoading;

            private (LocalisableString value, Action? linkAction)? data;

            public (LocalisableString value, Action? linkAction)? Data
            {
                get => data;
                set
                {
                    data = value;

                    if (value?.linkAction != null)
                        setLink(value.Value.value, value.Value.linkAction);
                    else if (value.HasValue)
                        setText(value.Value.value);
                    else
                        setLoading();
                }
            }

            public DateTimeOffset? Date
            {
                set
                {
                    if (value != null)
                        setDate(value.Value);
                    else
                        setText("-");
                }
            }

            public (string[] tags, Action<string> searchAction)? Tags
            {
                set
                {
                    if (value != null)
                        setTags(value.Value.tags, value.Value.searchAction);
                    else
                        setLoading();
                }
            }

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
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = OsuFont.Style.Caption1.Size,
                        Children = new Drawable[]
                        {
                            contentText = new TruncatingSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Font = OsuFont.Style.Caption1,
                            },
                            contentLink = new OsuHoverContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Child = contentLinkText = new TruncatingSpriteText
                                {
                                    Font = OsuFont.Style.Caption1,
                                },
                            },
                            contentDate = new DrawableDate(default, OsuFont.Style.Caption1.Size, false),
                            contentTags = new TagsLine(),
                            contentLoading = new LoadingSpinner
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Size = new Vector2(10),
                                Margin = new MarginPadding { Top = 3f },
                                State = { Value = Visibility.Visible },
                            }
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                labelText.Colour = colourProvider.Content1;
                contentText.Colour = colourProvider.Content2;
                contentLink.IdleColour = colourProvider.Light2;
            }

            protected override void Update()
            {
                base.Update();
                contentLinkText.MaxWidth = ChildSize.X;
            }

            private void clear()
            {
                contentText.Text = string.Empty;
                contentLinkText.Text = string.Empty;
                contentDate.Hide();
                contentTags.Tags = Array.Empty<string>();
                contentLoading.Hide();
            }

            private void setText(LocalisableString text)
            {
                clear();

                contentText.Text = text;
            }

            private void setLink(LocalisableString text, Action action) => Schedule(() =>
            {
                clear();

                contentLinkText.Text = text;
                contentLink.Action = action;
            });

            private void setDate(DateTimeOffset date)
            {
                clear();

                contentDate.Show();
                contentDate.Date = date;
            }

            private void setTags(string[] tags, Action<string> searchAction)
            {
                clear();

                contentTags.PerformSearch = searchAction;
                contentTags.Tags = tags;
            }

            private void setLoading()
            {
                clear();

                contentLoading.Show();
            }
        }
    }
}
