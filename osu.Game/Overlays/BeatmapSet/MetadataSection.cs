// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class MetadataSection : Container
    {
        private readonly FillFlowContainer textContainer;
        private readonly MetadataType type;
        private TextFlowContainer textFlow;

        private readonly Action<string> searchAction;

        private const float transition_duration = 250;

        public MetadataSection(MetadataType type, Action<string> searchAction = null)
        {
            this.type = type;
            this.searchAction = searchAction;

            Alpha = 0;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = textContainer = new FillFlowContainer
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,

                Margin = new MarginPadding { Top = 15 },
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new OsuSpriteText
                        {
                            Text = this.type.GetLocalisableDescription(),
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                        },
                    },
                },
            };
        }

        public string Text
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.FadeOut(transition_duration);
                    return;
                }

                this.FadeIn(transition_duration);

                setTextAsync(value);
            }
        }

        private void setTextAsync(string text)
        {
            LoadComponentAsync(new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Colour = Color4.White.Opacity(0.75f),
            }, loaded =>
            {
                textFlow?.Expire();

                switch (type)
                {
                    case MetadataType.Tags:
                        string[] tags = text.Split(" ");

                        for (int i = 0; i <= tags.Length - 1; i++)
                        {
                            string tag = tags[i];

                            if (searchAction != null)
                                loaded.AddLink(tag, () => searchAction(tag));
                            else
                                loaded.AddLink(tag, LinkAction.SearchBeatmapSet, tag);

                            if (i != tags.Length - 1)
                                loaded.AddText(" ");
                        }

                        break;

                    case MetadataType.Source:
                        if (searchAction != null)
                            loaded.AddLink(text, () => searchAction(text));
                        else
                            loaded.AddLink(text, LinkAction.SearchBeatmapSet, text);

                        break;

                    default:
                        loaded.AddText(text);
                        break;
                }

                textContainer.Add(textFlow = loaded);

                // fade in if we haven't yet.
                textContainer.FadeIn(transition_duration);
            });
        }
    }
}
