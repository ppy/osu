// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardScoreDisplay : Container, IHasCurrentValue<string>
    {
        private readonly BindableWithCurrent<string> current = new BindableWithCurrent<string>();

        public Bindable<string> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private readonly OsuSpriteText scoreText;

        public LeaderboardScoreDisplay()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, -2f),
                Children = new Drawable[]
                {
                    scoreText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        UseFullGlyphHeight = false,
                        Spacing = new Vector2(-1.5f),
                        Font = OsuFont.Style.Subtitle.With(weight: FontWeight.Light, fixedWidth: true),
                    },
                    content = new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(score => scoreText.Text = score.NewValue, true);
        }
    }
}
