// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Storyboards.Commands;

namespace osu.Game.Storyboards.Drawables
{
    public partial class StoryboardTriggerController : Component
    {
        public Bindable<bool> Passing
        {
            get => passing.Current;
            set => passing.Current = value;
        }

        private readonly BindableWithCurrent<bool> passing = new BindableWithCurrent<bool>();

        private readonly IBindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        [BackgroundDependencyLoader]
        private void load(GameplayState? gameplayState)
        {
            if (gameplayState != null)
                lastJudgementResult.BindTo(gameplayState.LastJudgementResult);
        }

        public void Bind<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            switch (triggerGroup.TriggerName)
            {
                case @"Passing":
                    bindPassing(drawable, triggerGroup, true);
                    break;

                case @"Failing":
                    bindPassing(drawable, triggerGroup, false);
                    break;

                case @"HitObjectHit":
                    lastJudgementResult.BindValueChanged(val =>
                    {
                        if (val.NewValue.IsNotNull() && val.NewValue.IsHit && val.NewValue.Type.IsScorable())
                            playTrigger(drawable, triggerGroup);
                    });
                    break;
            }
        }

        private void bindPassing<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup, bool passing)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            this.passing.BindValueChanged(val =>
            {
                if (val.NewValue != passing)
                    return;

                playTrigger(drawable, triggerGroup);
            });
        }

        private static void playTrigger<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            using (drawable.BeginDelayedSequence(0))
            {
                foreach (var command in triggerGroup.AllCommands.OrderBy(c => c.StartTime))
                    command.ApplyTransforms(drawable);
            }
        }
    }
}
