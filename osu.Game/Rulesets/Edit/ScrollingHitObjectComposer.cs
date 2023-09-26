// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class ScrollingHitObjectComposer<TObject> : HitObjectComposer<TObject>
        where TObject : HitObject
    {
        private readonly Bindable<TernaryState> showSpeedChanges = new Bindable<TernaryState>();

        protected ScrollingHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (DrawableRuleset is ISupportConstantAlgorithmToggle toggleRuleset)
            {
                LeftToolbox.Add(new EditorToolboxGroup("playfield")
                {
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new[]
                        {
                            new DrawableTernaryButton(new TernaryButton(showSpeedChanges, "Show speed changes", () => new SpriteIcon { Icon = FontAwesome.Solid.TachometerAlt }))
                        }
                    },
                });

                showSpeedChanges.BindValueChanged(state => toggleRuleset.ShowSpeedChanges.Value = state.NewValue == TernaryState.True, true);
            }
        }
    }
}
