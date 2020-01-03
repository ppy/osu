// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Overlays
{
    public class OverlayRulesetTabItem : TabItem<RulesetInfo>, IHasAccentColour
    {
        protected readonly OsuSpriteText Text;
        private readonly FillFlowContainer content;

        public override bool PropagatePositionalInputSubTree => Enabled.Value && !Active.Value && base.PropagatePositionalInputSubTree;

        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();
        private readonly Bindable<Color4> currentColour = new Bindable<Color4>();

        public Color4 AccentColour
        {
            get => accentColour.Value;
            set => accentColour.Value = value;
        }

        protected override Container<Drawable> Content => content;

        public OverlayRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(3, 0),
                    Child = Text = new OsuSpriteText
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Text = value.Name,
                    }
                },
                new HoverClickSounds()
            });

            Enabled.Value = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentColour.BindValueChanged(OnCurrentColourChanged);
            accentColour.BindValueChanged(_ => updateState());
            Enabled.BindValueChanged(_ => updateState(), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        private void updateState()
        {
            Text.Font = Text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);

            currentColour.Value = IsHovered || Active.Value
                ? Color4.White
                : Enabled.Value ? AccentColour : Color4.DimGray;
        }

        protected virtual void OnCurrentColourChanged(ValueChangedEvent<Color4> colour) => Text.FadeColour(colour.NewValue, 120, Easing.OutQuint);
    }
}
