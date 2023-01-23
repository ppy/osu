// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class LevelBadge : CompositeDrawable, IHasTooltip
    {
        public readonly Bindable<UserStatistics.LevelInfo?> LevelInfo = new Bindable<UserStatistics.LevelInfo?>();

        public LocalisableString TooltipText { get; private set; }

        private OsuSpriteText levelText = null!;
        private Sprite sprite = null!;

        [Resolved]
        private OsuColour osuColour { get; set; } = null!;

        public LevelBadge()
        {
            TooltipText = UsersStrings.ShowStatsLevel("0");
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            InternalChildren = new Drawable[]
            {
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get("Profile/levelbadge"),
                    Colour = colours.Yellow,
                },
                levelText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 20)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LevelInfo.BindValueChanged(level => updateLevel(level.NewValue), true);
        }

        private void updateLevel(UserStatistics.LevelInfo? levelInfo)
        {
            int level = levelInfo?.Current ?? 0;

            levelText.Text = level.ToString();
            TooltipText = UsersStrings.ShowStatsLevel(level.ToString());

            sprite.Colour = mapLevelToTierColour(level);
        }

        private ColourInfo mapLevelToTierColour(int level)
        {
            var tier = RankingTier.Iron;

            if (level > 0)
            {
                tier = (RankingTier)(level / 20);
            }

            if (level >= 105)
            {
                tier = RankingTier.Radiant;
            }

            if (level >= 110)
            {
                tier = RankingTier.Lustrous;
            }

            return osuColour.ForRankingTier(tier);
        }
    }
}
