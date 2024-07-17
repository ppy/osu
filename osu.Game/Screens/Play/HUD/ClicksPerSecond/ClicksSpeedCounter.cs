// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osu.Framework.Bindables;
using osuTK;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;

namespace osu.Game.Screens.Play.HUD.ClicksPerSecond
{
    public partial class ClicksSpeedCounter : RollingCounter<int>, ISerialisableDrawable
    {
        [Resolved]
        private ClicksPerSecondController controller { get; set; } = null!;

        [SettingSource(typeof(ClicksSpeedCounterDisplayString), nameof(ClicksSpeedCounterDisplayString.ClicksSpeedDisplay), nameof(ClicksSpeedCounterDisplayString.ClicksSpeedDisplayDescription))]
        public Bindable<ClicksSpeedDisplayUnit> DisplayUnit { get; } = new Bindable<ClicksSpeedDisplayUnit>();

        private readonly Bindable<string> displayUnitString = new Bindable<string>();
        protected override double RollingDuration => 175;

        public bool UsesFixedAnchor { get; set; }

        public ClicksSpeedCounter()
        {
            Current.Value = 0;

            DisplayUnit.BindValueChanged(v =>
            {
                if (v.NewValue == ClicksSpeedDisplayUnit.Bpm)
                    displayUnitString.Value = "BPM";
                else
                    displayUnitString.Value = "/sec";
            }, true);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }

        protected override void Update()
        {
            base.Update();

            if (DisplayUnit.Value == ClicksSpeedDisplayUnit.Bpm)
                // multiply by 60 * (1 / 4) to convert CPS to BPM
                Current.Value = (int)(controller.Value * 60f * (1f / 4f));
            else
                Current.Value = controller.Value;
        }

        protected override IHasText CreateText() => new TextComponent(displayUnitString);

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent(IBindable<string> displayUnit)
            {
                OsuSpriteText unitText;

                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 6, fixedWidth: false),
                                    Text = @"clicks",
                                },
                                unitText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 6, fixedWidth: false),
                                    Text = displayUnit.Value,
                                    Padding = new MarginPadding { Bottom = 3f }, // align baseline better
                                }
                            }
                        }
                    }
                };

                displayUnit.BindValueChanged(v => unitText.Text = v.NewValue, true);
            }
        }

        public enum ClicksSpeedDisplayUnit
        {
            [LocalisableDescription(typeof(ClicksSpeedCounterDisplayString), nameof(ClicksSpeedCounterDisplayString.ClicksSpeedDisplayUnitBpm))]
            Bpm,

            /// <summary>
            /// clicks per second
            /// </summary>
            [LocalisableDescription(typeof(ClicksSpeedCounterDisplayString), nameof(ClicksSpeedCounterDisplayString.ClicksSpeedDisplayUnitCps))]
            Cps,
        }
    }
}
