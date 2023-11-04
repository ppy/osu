// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public partial class ArgonPerformancePointsCounter : GameplayPerformancePointsCounter, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override IHasText CreateText() => new TextComponent();

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            private readonly OsuSpriteText text;

            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = BeatmapsetsStrings.ShowScoreboardHeaderspp.ToUpper(),
                            Font = OsuFont.Torus.With(size: 12, weight: FontWeight.Bold),
                        },
                        text = new OsuSpriteText
                        {
                            Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.Regular),
                        }
                    }
                };
            }
        }
    }
}
