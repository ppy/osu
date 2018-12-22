// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTitle : CompositeDrawable
    {
        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        private readonly LinkFlowContainer textFlow;

        public BeatmapTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = textFlow = new LinkFlowContainer { AutoSizeAxes = Axes.Both };

            Beatmap.BindValueChanged(v => updateText());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateText();
        }

        private float textSize = OsuSpriteText.FONT_SIZE;

        public float TextSize
        {
            get => textSize;
            set
            {
                if (textSize == value)
                    return;
                textSize = value;

                updateText();
            }
        }

        private void updateText()
        {
            if (!IsLoaded)
                return;

            textFlow.Clear();

            if (Beatmap.Value == null)
                textFlow.AddText("Changing map", s => s.TextSize = TextSize);
            else
            {
                textFlow.AddLink(new[]
                {
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((Beatmap.Value.Metadata.ArtistUnicode, Beatmap.Value.Metadata.Artist)),
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = " - ",
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((Beatmap.Value.Metadata.TitleUnicode, Beatmap.Value.Metadata.Title)),
                        TextSize = TextSize,
                    }
                }, null, LinkAction.OpenBeatmap, Beatmap.Value.OnlineBeatmapID.ToString(), "Open beatmap");
            }
        }
    }
}
