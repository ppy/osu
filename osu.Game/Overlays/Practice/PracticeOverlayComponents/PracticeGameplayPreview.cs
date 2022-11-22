// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Practice.PracticeOverlayComponents
{
    public class PracticeGameplayPreview : CompositeDrawable
    {
        public BindableDouble SeekTime = new BindableDouble();

        private EditorClock clock = null!;

        [Resolved]
        private PracticePlayer player { get; set; } = null!;

        private DrawableRuleset drawableRuleset = null!;

        private GameplayClockContainer gameplayClockContainer = null!;

        public PracticeGameplayPreview()
        {
            RelativeSizeAxes = Axes.Both;

            //Stops Catch Ruleset and OsuRuleset from clipping the header;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = gameplayClockContainer = new GameplayClockContainer(clock = new EditorClock());

            //We dont want the preview to use mods (maybe rate ones??)
            drawableRuleset = player.CurrentRuleset.CreateDrawableRulesetWith(player.PlayableBeatmap);

            var rulesetSkinProvider = new RulesetSkinProvidingContainer(player.CurrentRuleset, player.PlayableBeatmap, player.Beatmap.Value.Skin);

            gameplayClockContainer.Add(rulesetSkinProvider);

            drawableRuleset.FrameStablePlayback = false;

            rulesetSkinProvider.Add(createGameplayComponents());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SeekTime.BindValueChanged(seek =>
            {
                seekTo(seek.NewValue);
            });
        }

        private void seekTo(double seekTime)
        {
            gameplayClockContainer.Start();
            gameplayClockContainer.Seek(seekTime);
        }

        private Drawable createGameplayComponents() => new ScalingContainer(ScalingMode.Gameplay)
        {
            Child = drawableRuleset
        };
    }
}
