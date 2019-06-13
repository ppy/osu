// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Details
{
    public class MetadataSection : FillFlowContainer
    {
        private readonly float transitionDuration;
        private readonly MetadataType type;
        private readonly OsuSpriteText header;
        private readonly LinkFlowContainer linkFlow;

        public Color4 TextColour
        {
            get => linkFlow.Colour;
            set => linkFlow.Colour = value;
        }

        public Color4 HeaderColour
        {
            get => header.Colour;
            set => header.Colour = value;
        }

        public MarginPadding HeaderMargin
        {
            get => header.Margin;
            set => header.Margin = value;
        }

        public string Text
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.FadeOut(transitionDuration);
                    return;
                }

                this.FadeIn(transitionDuration);
                linkFlow.Clear();
                addMetadataLinks(value, type);
            }
        }

        public MetadataSection(MetadataType type, float transitionDuration)
        {
            this.type = type;
            this.transitionDuration = transitionDuration;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(5f);
            InternalChildren = new Drawable[]
            {
                header = new OsuSpriteText
                {
                    Text = type.ToString(),
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                    Shadow = false,
                    Margin = new MarginPadding { Top = 5 },
                },
                linkFlow = new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
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
                        linkFlow.AddLink(individualTag + " ", null, LinkAction.OpenTextSearch, individualTag, "Open search");
                    break;

                case MetadataType.Source:
                    linkFlow.AddLink(text, null, LinkAction.OpenTextSearch, text, "Open search");
                    break;

                case MetadataType.Description:
                    linkFlow.AddText(text);
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