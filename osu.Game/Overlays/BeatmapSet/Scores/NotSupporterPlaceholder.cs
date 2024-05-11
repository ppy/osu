// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class NotSupporterPlaceholder : Container
    {
        public NotSupporterPlaceholder()
        {
            LinkFlowContainer text;

            AutoSizeAxes = Axes.Both;
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = BeatmapsetsStrings.ShowScoreboardSupporterOnly,
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                    },
                    text = new LinkFlowContainer(t => t.Font = t.Font.With(size: 11))
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                    }
                }
            };

            text.AddText("Click ");
            text.AddLink("here", "/home/support");
            text.AddText(" to see all the fancy features that you can get!");
        }
    }
}
