// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
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
    public partial class BeatmapDetailsWedgeStatistic : FillFlowContainer
    {
        private (LocalisableString value, Action? linkAction)? data;

        public (LocalisableString value, Action? linkAction)? Data
        {
            get => data;
            set
            {
                data = value;

                valueText.Clear();

                if (value.HasValue)
                {
                    string valueString = value.Value.value.ToString();
                    // todo: this logic is not perfect but we don't have a way to truncate text in TextFlowContainer yet.
                    string truncatedValueString = valueString.Truncate(24);

                    if (value.Value.linkAction != null)
                        valueText.AddLink(truncatedValueString, value.Value.linkAction, valueString != truncatedValueString ? valueString : null);
                    else
                        valueText.AddText(truncatedValueString);
                }
                else
                {
                    valueText.AddArbitraryDrawable(new LoadingSpinner
                    {
                        Size = new Vector2(10),
                        Margin = new MarginPadding { Top = 3f },
                        State = { Value = Visibility.Visible },
                    });
                }
            }
        }

        public DateTimeOffset? Date
        {
            set
            {
                valueText.Clear();

                if (value != null)
                    valueText.AddArbitraryDrawable(new DrawableDate(value.Value, textSize: OsuFont.Caption.Size, italic: false));
                else
                    valueText.AddText("-");
            }
        }

        public (string[] tags, Action<string> linkAction) Tags
        {
            set
            {
                valueText.Clear();
                int total = 0;

                foreach (string tag in value.tags)
                {
                    total += tag.Length + 1;

                    // todo: this logic is not perfect but we don't have a way to truncate text in TextFlowContainer yet.
                    if (total > 80)
                    {
                        valueText.AddArbitraryDrawable(new TagsOverflowButton(value.tags));
                        break;
                    }

                    valueText.AddLink(tag, () => value.linkAction(tag));
                    valueText.AddText(" ");
                }
            }
        }

        private readonly OsuSpriteText labelText;
        private readonly LinkFlowContainer valueText;

        public BeatmapDetailsWedgeStatistic(LocalisableString label)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                labelText = new OsuSpriteText
                {
                    Text = label,
                    Font = OsuFont.Caption.With(weight: FontWeight.SemiBold),
                },
                valueText = new LinkFlowContainer(t => t.Font = OsuFont.Caption)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = OsuFont.Caption.Size,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            labelText.Colour = colourProvider.Content1;
            valueText.Colour = colourProvider.Content2;
        }
    }
}
