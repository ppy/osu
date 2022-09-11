// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Represents a component that displays a skinned <see cref="ICatchComboCounter"/> and handles combo judgement results for updating it accordingly.
    /// </summary>
    public class CatchComboDisplay : SkinnableDrawable
    {
        private int currentCombo;

        [CanBeNull]
        public ICatchComboCounter ComboCounter => Drawable as ICatchComboCounter;

        private Bindable<HUDVisibilityMode> hudVisibilityMode = null!;

        private readonly BindableBool replayLoaded = new BindableBool();

        private readonly BindableBool showCombo = new BindableBool();

        [Resolved]
        private OsuConfigManager config { get; set; }

        public CatchComboDisplay()
            : base(new CatchSkinComponent(CatchSkinComponents.CatchComboCounter), _ => Empty())
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load(DrawableRuleset drawableRuleset)
        {
            hudVisibilityMode = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode);

            hudVisibilityMode.BindValueChanged(s =>
            {
                updateVisibilityState();
            });

            if (drawableRuleset != null)
                replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);

            replayLoaded.BindValueChanged(s =>
            {
                updateVisibilityState();
            });

            showCombo.BindValueChanged(s =>
            {
                if (ComboCounter == null) return;

                if (!s.NewValue)
                {
                    ComboCounter.Hide();
                }
            });

            updateVisibilityState();

            void updateVisibilityState()
            {
                switch (hudVisibilityMode.Value)
                {
                    case HUDVisibilityMode.Never:
                        showCombo.Value = false;
                        break;

                    case HUDVisibilityMode.HideDuringGameplay:
                        showCombo.Value = replayLoaded.Value;
                        break;

                    case HUDVisibilityMode.Always:
                        showCombo.Value = true;
                        break;
                }
            }
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            ComboCounter?.UpdateCombo(currentCombo);
        }

        public void OnNewResult(DrawableCatchHitObject judgedObject, JudgementResult result)
        {
            if (!result.Type.AffectsCombo() || !result.HasResult)
                return;

            if (!result.IsHit)
            {
                updateCombo(0, null);
                return;
            }

            updateCombo(result.ComboAtJudgement + 1, judgedObject.AccentColour.Value);
        }

        public void OnRevertResult(DrawableCatchHitObject judgedObject, JudgementResult result)
        {
            if (!result.Type.AffectsCombo() || !result.HasResult)
                return;

            updateCombo(result.ComboAtJudgement, judgedObject.AccentColour.Value);
        }

        private void updateCombo(int newCombo, Color4? hitObjectColour)
        {
            if (!showCombo.Value) return;

            currentCombo = newCombo;
            ComboCounter?.UpdateCombo(newCombo, hitObjectColour);
        }
    }
}
