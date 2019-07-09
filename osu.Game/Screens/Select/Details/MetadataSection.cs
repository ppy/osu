// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Screens.Select.Details
{
    public abstract class MetadataSection : FillFlowContainer
    {
        private const float transition_duration = 250;
        private readonly MetadataType type;
        protected readonly OsuSpriteText Header;
        protected readonly LinkFlowContainer LinkFlow;

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
                LinkFlow.Clear();
                addMetadataLinks(value, type);
            }
        }

        protected MetadataSection(MetadataType type)
        {
            this.type = type;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(5f);
            InternalChildren = new Drawable[]
            {
                Header = new OsuSpriteText
                {
                    Text = type.ToString(),
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                    Shadow = false,
                    Margin = new MarginPadding { Top = 5 },
                },
                LinkFlow = new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        private void addMetadataLinks(string text, MetadataType type)
        {
            switch (type)
            {
                case MetadataType.Tags:
                    foreach (string individualTag in text.Split(' '))
                        LinkFlow.AddLink(individualTag + " ", null, LinkAction.OpenDirectWithSearch, individualTag, "Open search");
                    break;

                case MetadataType.Source:
                    LinkFlow.AddLink(text, null, LinkAction.OpenDirectWithSearch, text, "Open search");
                    break;

                case MetadataType.Description:
                    LinkFlow.AddText(text);
                    break;
            }
        }
    }

    public enum MetadataType
    {
        Tags,
        Source,
        Description
    }
}