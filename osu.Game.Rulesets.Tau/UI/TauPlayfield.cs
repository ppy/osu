using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Tau.Configuration;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osu.Game.Rulesets.Tau.UI.Components;
using osu.Game.Rulesets.Tau.UI.Cursor;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.UI
{
    [Cached]
    public class TauPlayfield : Playfield
    {
        private readonly Circle playfieldBackground;
        private readonly TauCursor cursor;
        private readonly JudgementContainer<DrawableTauJudgement> judgementLayer;
        private readonly Container<KiaiHitExplosion> kiaiExplosionContainer;

        public static readonly Vector2 BASE_SIZE = new Vector2(768, 768);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public TauPlayfield(BeatmapDifficulty difficulty)
        {
            RelativeSizeAxes = Axes.None;
            cursor = new TauCursor(difficulty);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(768);

            AddRangeInternal(new Drawable[]
            {
                judgementLayer = new JudgementContainer<DrawableTauJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new VisualisationContainer(),
                playfieldBackground = new Circle
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            BorderThickness = 3,
                            BorderColour = Color4.White,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Alpha = 0,
                            }
                        },
                    }
                },
                HitObjectContainer,
                cursor,
                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                {
                    Name = "Kiai hit explosions",
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Blending = BlendingParameters.Additive,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
            });
        }

        protected Bindable<float> PlayfieldDimLevel = new Bindable<float>(0.3f); // Change the default as you see fit

        [BackgroundDependencyLoader(true)]
        private void load(TauRulesetConfigManager config)
        {
            config?.BindWith(TauRulesetSettings.PlayfieldDim, PlayfieldDimLevel);
            PlayfieldDimLevel.ValueChanged += _ => updateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateVisuals();
        }

        private void updateVisuals()
        {
            playfieldBackground.FadeTo(PlayfieldDimLevel.Value, 100);
        }

        public bool CheckIfWeCanValidate(DrawableTauHitObject obj) => cursor.CheckForValidation(obj);

        public override void Add(DrawableHitObject h)
        {
            base.Add(h);

            switch (h)
            {
                case DrawableTauHitObject _:
                    var obj = (DrawableTauHitObject)h;
                    obj.CheckValidation = CheckIfWeCanValidate;

                    break;
            }

            h.OnNewResult += onNewResult;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            DrawableTauJudgement explosion = new DrawableTauJudgement(result, judgedObject)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };

            switch (judgedObject)
            {
                case DrawableBeat beat:
                    var angle = beat.HitObject.Angle;
                    explosion.Position = Extensions.GetCircularPosition(.6f, angle);
                    explosion.Rotation = angle;

                    if (judgedObject.HitObject.Kiai && result.Type != HitResult.Miss)
                        kiaiExplosionContainer.Add(new KiaiHitExplosion(judgedObject.AccentColour.Value)
                        {
                            Position = Extensions.GetCircularPosition(.5f, angle),
                            Angle = angle,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        });

                    break;

                case DrawableHardBeat _:
                    explosion.Position = Extensions.GetCircularPosition(.6f, 0);

                    if (judgedObject.HitObject.Kiai && result.Type != HitResult.Miss)
                        kiaiExplosionContainer.Add(new KiaiHitExplosion(judgedObject.AccentColour.Value, true)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        });

                    break;
            }

            judgementLayer.Add(explosion);
        }

        private class VisualisationContainer : BeatSyncedContainer
        {
            private PlayfieldVisualisation visualisation;
            private bool firstKiaiBeat = true;
            private int kiaiBeatIndex;
            private readonly Bindable<bool> showVisualisation = new Bindable<bool>(true);

            [BackgroundDependencyLoader(true)]
            private void load(TauRulesetConfigManager settings)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Child = visualisation = new PlayfieldVisualisation
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Transparent
                };

                settings?.BindWith(TauRulesetSettings.ShowVisualizer, showVisualisation);
                showVisualisation.BindValueChanged(value => { visualisation.FadeTo(value.NewValue ? 1 : 0, 500); });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                visualisation.AccentColour = Color4.White;
                showVisualisation.TriggerChange();
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                if (effectPoint.KiaiMode)
                {
                    kiaiBeatIndex += 1;

                    if (firstKiaiBeat)
                    {
                        visualisation.FlashColour(Color4.White, timingPoint.BeatLength * 4, Easing.In);
                        firstKiaiBeat = false;

                        return;
                    }

                    if (kiaiBeatIndex >= 5)
                        visualisation.FlashColour(Color4.White.Opacity(0.15f), timingPoint.BeatLength, Easing.In);
                }
                else
                {
                    firstKiaiBeat = true;
                    kiaiBeatIndex = 0;
                }
            }
        }
    }
}
