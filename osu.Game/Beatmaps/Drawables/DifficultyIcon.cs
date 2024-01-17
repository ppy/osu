// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class DifficultyIcon : CompositeDrawable, IHasCustomTooltip<DifficultyIconTooltipContent>, IHasCurrentValue<StarDifficulty>
    {
        /// <summary>
        /// Size of this difficulty icon.
        /// </summary>
        public new Vector2 Size
        {
            get => iconContainer.Size;
            set => iconContainer.Size = value;
        }

        /// <summary>
        /// Whether to display a tooltip on hover. Only works if a beatmap was provided at construction time.
        /// </summary>
        public bool ShowTooltip { get; set; } = true;

        /// <summary>
        /// Whether to include the difficulty stats in the tooltip or not. Defaults to false. Has no effect if <see cref="ShowTooltip"/> is false.
        /// </summary>
        public bool ShowExtendedTooltip { get; set; }

        private readonly IBeatmapInfo? beatmap;

        private readonly IRulesetInfo ruleset;

        private readonly Mod[]? mods;

        private Drawable background = null!;

        private readonly Container iconContainer;

        private readonly BindableWithCurrent<StarDifficulty> difficulty = new BindableWithCurrent<StarDifficulty>();

        public virtual Bindable<StarDifficulty> Current
        {
            get => difficulty.Current;
            set => difficulty.Current = value;
        }

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/>. Will use provided beatmap's <see cref="BeatmapInfo.StarRating"/> for initial value.
        /// </summary>
        /// <param name="beatmap">The beatmap to be displayed in the tooltip, and to be used for the initial star rating value.</param>
        /// <param name="mods">An array of mods to account for in the calculations</param>
        /// <param name="ruleset">An optional ruleset to be used for the icon display, in place of the beatmap's ruleset.</param>
        public DifficultyIcon(IBeatmapInfo beatmap, IRulesetInfo? ruleset = null, Mod[]? mods = null)
            : this(ruleset ?? beatmap.Ruleset)
        {
            this.beatmap = beatmap;
            this.mods = mods;

            Current.Value = new StarDifficulty(beatmap.StarRating, 0);
        }

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/> without an associated beatmap.
        /// </summary>
        /// <param name="ruleset">The ruleset to be used for the icon display.</param>
        public DifficultyIcon(IRulesetInfo ruleset)
        {
            this.ruleset = ruleset;

            AutoSizeAxes = Axes.Both;
            InternalChild = iconContainer = new Container { Size = new Vector2(20f) };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            iconContainer.Children = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.06f),

                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new ConstrainedIconContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    // the null coalesce here is only present to make unit tests work (ruleset dlls aren't copied correctly for testing at the moment)
                    Icon = getRulesetIcon()
                },
            };

            Current.BindValueChanged(difficulty => background.Colour = colours.ForStarDifficulty(difficulty.NewValue.Stars), true);
        }

        private Drawable getRulesetIcon()
        {
            int? onlineID = ruleset.OnlineID;

            if (onlineID >= 0 && rulesets.GetRuleset(onlineID.Value)?.CreateInstance() is Ruleset rulesetInstance)
                return rulesetInstance.CreateIcon();

            return new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };
        }

        ITooltip<DifficultyIconTooltipContent> IHasCustomTooltip<DifficultyIconTooltipContent>.
            GetCustomTooltip() => new DifficultyIconTooltip();

        DifficultyIconTooltipContent IHasCustomTooltip<DifficultyIconTooltipContent>.
            TooltipContent => (ShowTooltip && beatmap != null ? new DifficultyIconTooltipContent(beatmap, Current, ruleset, mods, ShowExtendedTooltip) : null)!;
    }
}
