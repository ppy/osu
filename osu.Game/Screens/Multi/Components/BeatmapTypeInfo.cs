// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTypeInfo : CompositeDrawable
    {
        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();
        public readonly IBindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        public readonly IBindable<GameType> Type = new Bindable<GameType>();

        public BeatmapTypeInfo()
        {
            AutoSizeAxes = Axes.Both;

            BeatmapTitle beatmapTitle;
            ModeTypeInfo modeTypeInfo;
            LinkFlowContainer beatmapAuthor;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                LayoutDuration = 100,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    modeTypeInfo = new ModeTypeInfo(),
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = 30,
                        Margin = new MarginPadding { Left = 5 },
                        Children = new Drawable[]
                        {
                            beatmapTitle = new BeatmapTitle(),
                            beatmapAuthor = new LinkFlowContainer(s => s.TextSize = 14)
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                AutoSizeAxes = Axes.Both
                            },
                        },
                    },
                }
            };

            modeTypeInfo.Beatmap.BindTo(Beatmap);
            modeTypeInfo.Ruleset.BindTo(Ruleset);
            modeTypeInfo.Type.BindTo(Type);

            beatmapTitle.Beatmap.BindTo(Beatmap);

            Beatmap.BindValueChanged(v =>
            {
                beatmapAuthor.Clear();

                if (v != null)
                {
                    beatmapAuthor.AddText("mapped by ", s => s.Colour = OsuColour.Gray(0.8f));
                    beatmapAuthor.AddLink(v.Metadata.Author.Username, null, LinkAction.OpenUserProfile, v.Metadata.Author.Id.ToString(), "View Profile");
                }
            });
        }
    }
}
