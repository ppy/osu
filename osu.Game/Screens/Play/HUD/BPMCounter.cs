// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class BPMCounter : RollingCounter<double>, ISkinnableDrawable
    {
        protected override double RollingDuration => 300;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        public BPMCounter()
        {
            Current.Value = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }

        protected override LocalisableString FormatCount(double count) => count.ToString(@"0");

        public BindableDouble GetBPM()
        {
            return new BindableDouble(gameplayState.Beatmap.ControlPointInfo.TimingPointAt(GameplayClock.CurrentTime).BPM);
        }

        protected override void Update()
        {
            base.Update();
            Current.Value = GetBPM().Value;
        }

        protected override IHasText CreateText() => new TextComponent();

        public bool UsesFixedAnchor { get; set; }

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(3, 0),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                        },
                        new OsuSpriteText
                        {
                            Text = "BPM",
                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        },
                    },
                };
            }
        }
    }
}