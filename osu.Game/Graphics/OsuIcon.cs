// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace osu.Game.Graphics
{
    public static class OsuIcon
    {
        public const string FONT_NAME = @"Icons";

        // ruleset icons
        public static IconUsage RulesetOsu => get(OsuIconMapping.RulesetOsu);
        public static IconUsage RulesetMania => get(OsuIconMapping.RulesetMania);
        public static IconUsage RulesetCatch => get(OsuIconMapping.RulesetCatch);
        public static IconUsage RulesetTaiko => get(OsuIconMapping.RulesetTaiko);

        public static IconUsage Logo => get(OsuIconMapping.Logo);
        public static IconUsage EditCircle => get(OsuIconMapping.EditCircle);
        public static IconUsage LeftCircle => get(OsuIconMapping.LeftCircle);
        public static IconUsage RightCircle => get(OsuIconMapping.RightCircle);

        public static IconUsage Audio => get(OsuIconMapping.Audio);
        public static IconUsage Beatmap => get(OsuIconMapping.Beatmap);
        public static IconUsage Calendar => get(OsuIconMapping.Calendar);
        public static IconUsage ChangelogA => get(OsuIconMapping.ChangelogA);
        public static IconUsage ChangelogB => get(OsuIconMapping.ChangelogB);
        public static IconUsage Chat => get(OsuIconMapping.Chat);
        public static IconUsage CheckCircle => get(OsuIconMapping.CheckCircle);
        public static IconUsage Clock => get(OsuIconMapping.Clock);
        public static IconUsage CollapseA => get(OsuIconMapping.CollapseA);
        public static IconUsage Collections => get(OsuIconMapping.Collections);
        public static IconUsage Cross => get(OsuIconMapping.Cross);
        public static IconUsage CrossCircle => get(OsuIconMapping.CrossCircle);
        public static IconUsage Crown => get(OsuIconMapping.Crown);
        public static IconUsage DailyChallenge => get(OsuIconMapping.DailyChallenge);
        public static IconUsage Debug => get(OsuIconMapping.Debug);
        public static IconUsage Delete => get(OsuIconMapping.Delete);
        public static IconUsage Details => get(OsuIconMapping.Details);
        public static IconUsage Discord => get(OsuIconMapping.Discord);
        public static IconUsage EllipsisHorizontal => get(OsuIconMapping.EllipsisHorizontal);
        public static IconUsage EllipsisVertical => get(OsuIconMapping.EllipsisVertical);
        public static IconUsage ExpandA => get(OsuIconMapping.ExpandA);
        public static IconUsage ExpandB => get(OsuIconMapping.ExpandB);
        public static IconUsage FeaturedArtist => get(OsuIconMapping.FeaturedArtist);
        public static IconUsage FeaturedArtistCircle => get(OsuIconMapping.FeaturedArtistCircle);
        public static IconUsage GameplayA => get(OsuIconMapping.GameplayA);
        public static IconUsage GameplayB => get(OsuIconMapping.GameplayB);
        public static IconUsage GameplayC => get(OsuIconMapping.GameplayC);
        public static IconUsage Global => get(OsuIconMapping.Global);
        public static IconUsage Graphics => get(OsuIconMapping.Graphics);
        public static IconUsage Heart => get(OsuIconMapping.Heart);
        public static IconUsage Home => get(OsuIconMapping.Home);
        public static IconUsage Input => get(OsuIconMapping.Input);
        public static IconUsage Maintenance => get(OsuIconMapping.Maintenance);
        public static IconUsage Megaphone => get(OsuIconMapping.Megaphone);
        public static IconUsage Metronome => get(OsuIconMapping.Metronome);
        public static IconUsage Music => get(OsuIconMapping.Music);
        public static IconUsage News => get(OsuIconMapping.News);
        public static IconUsage Next => get(OsuIconMapping.Next);
        public static IconUsage NextCircle => get(OsuIconMapping.NextCircle);
        public static IconUsage Notification => get(OsuIconMapping.Notification);
        public static IconUsage Online => get(OsuIconMapping.Online);
        public static IconUsage Play => get(OsuIconMapping.Play);
        public static IconUsage Player => get(OsuIconMapping.Player);
        public static IconUsage PlayerFollow => get(OsuIconMapping.PlayerFollow);
        public static IconUsage Prev => get(OsuIconMapping.Prev);
        public static IconUsage PrevCircle => get(OsuIconMapping.PrevCircle);
        public static IconUsage Ranking => get(OsuIconMapping.Ranking);
        public static IconUsage Rulesets => get(OsuIconMapping.Rulesets);
        public static IconUsage Search => get(OsuIconMapping.Search);
        public static IconUsage Settings => get(OsuIconMapping.Settings);
        public static IconUsage SkinA => get(OsuIconMapping.SkinA);
        public static IconUsage SkinB => get(OsuIconMapping.SkinB);
        public static IconUsage Star => get(OsuIconMapping.Star);
        public static IconUsage Storyboard => get(OsuIconMapping.Storyboard);
        public static IconUsage Team => get(OsuIconMapping.Team);
        public static IconUsage ThumbsUp => get(OsuIconMapping.ThumbsUp);
        public static IconUsage Tournament => get(OsuIconMapping.Tournament);
        public static IconUsage Twitter => get(OsuIconMapping.Twitter);
        public static IconUsage UserInterface => get(OsuIconMapping.UserInterface);
        public static IconUsage Wiki => get(OsuIconMapping.Wiki);
        public static IconUsage EditorAddControlPoint => get(OsuIconMapping.EditorAddControlPoint);
        public static IconUsage EditorConvertToStream => get(OsuIconMapping.EditorConvertToStream);
        public static IconUsage EditorDistanceSnap => get(OsuIconMapping.EditorDistanceSnap);
        public static IconUsage EditorFinish => get(OsuIconMapping.EditorFinish);
        public static IconUsage EditorGridSnap => get(OsuIconMapping.EditorGridSnap);
        public static IconUsage EditorNewComboA => get(OsuIconMapping.EditorNewComboA);
        public static IconUsage EditorNewComboB => get(OsuIconMapping.EditorNewComboB);
        public static IconUsage EditorSelect => get(OsuIconMapping.EditorSelect);
        public static IconUsage EditorSound => get(OsuIconMapping.EditorSound);
        public static IconUsage EditorWhistle => get(OsuIconMapping.EditorWhistle);
        public static IconUsage Tortoise => get(OsuIconMapping.Tortoise);
        public static IconUsage Hare => get(OsuIconMapping.Hare);

        // mod icons

        public static IconUsage ModNoMod => get(OsuIconMapping.ModNoMod);

        /*
         can be regenerated semi-automatically using osu-web's mod database via

         $ jq -r '.[].Mods[].Name' mods.json | sort | uniq | \
           sed 's/ //g' | \
           awk '{print "public static IconUsage Mod" $0 " => get(OsuIconMapping.Mod" $0 ");"}' | pbcopy
         */

        public static IconUsage ModAccuracyChallenge => get(OsuIconMapping.ModAccuracyChallenge);
        public static IconUsage ModAdaptiveSpeed => get(OsuIconMapping.ModAdaptiveSpeed);
        public static IconUsage ModAlternate => get(OsuIconMapping.ModAlternate);
        public static IconUsage ModApproachDifferent => get(OsuIconMapping.ModApproachDifferent);
        public static IconUsage ModAutopilot => get(OsuIconMapping.ModAutopilot);
        public static IconUsage ModAutoplay => get(OsuIconMapping.ModAutoplay);
        public static IconUsage ModBarrelRoll => get(OsuIconMapping.ModBarrelRoll);
        public static IconUsage ModBlinds => get(OsuIconMapping.ModBlinds);
        public static IconUsage ModBloom => get(OsuIconMapping.ModBloom);
        public static IconUsage ModBubbles => get(OsuIconMapping.ModBubbles);
        public static IconUsage ModCinema => get(OsuIconMapping.ModCinema);
        public static IconUsage ModClassic => get(OsuIconMapping.ModClassic);
        public static IconUsage ModConstantSpeed => get(OsuIconMapping.ModConstantSpeed);
        public static IconUsage ModCover => get(OsuIconMapping.ModCover);
        public static IconUsage ModDaycore => get(OsuIconMapping.ModDaycore);
        public static IconUsage ModDeflate => get(OsuIconMapping.ModDeflate);
        public static IconUsage ModDepth => get(OsuIconMapping.ModDepth);
        public static IconUsage ModDifficultyAdjust => get(OsuIconMapping.ModDifficultyAdjust);
        public static IconUsage ModDoubleTime => get(OsuIconMapping.ModDoubleTime);
        public static IconUsage ModDualStages => get(OsuIconMapping.ModDualStages);
        public static IconUsage ModEasy => get(OsuIconMapping.ModEasy);
        public static IconUsage ModEightKeys => get(OsuIconMapping.ModEightKeys);
        public static IconUsage ModFadeIn => get(OsuIconMapping.ModFadeIn);
        public static IconUsage ModFiveKeys => get(OsuIconMapping.ModFiveKeys);
        public static IconUsage ModFlashlight => get(OsuIconMapping.ModFlashlight);
        public static IconUsage ModFloatingFruits => get(OsuIconMapping.ModFloatingFruits);
        public static IconUsage ModFourKeys => get(OsuIconMapping.ModFourKeys);
        public static IconUsage ModFreezeFrame => get(OsuIconMapping.ModFreezeFrame);
        public static IconUsage ModGrow => get(OsuIconMapping.ModGrow);
        public static IconUsage ModHalfTime => get(OsuIconMapping.ModHalfTime);
        public static IconUsage ModHardRock => get(OsuIconMapping.ModHardRock);
        public static IconUsage ModHidden => get(OsuIconMapping.ModHidden);
        public static IconUsage ModHoldOff => get(OsuIconMapping.ModHoldOff);
        public static IconUsage ModInvert => get(OsuIconMapping.ModInvert);
        public static IconUsage ModMagnetised => get(OsuIconMapping.ModMagnetised);
        public static IconUsage ModMirror => get(OsuIconMapping.ModMirror);
        public static IconUsage ModMovingFast => get(OsuIconMapping.ModMovingFast);
        public static IconUsage ModMuted => get(OsuIconMapping.ModMuted);
        public static IconUsage ModNightcore => get(OsuIconMapping.ModNightcore);
        public static IconUsage ModNineKeys => get(OsuIconMapping.ModNineKeys);
        public static IconUsage ModNoFail => get(OsuIconMapping.ModNoFail);
        public static IconUsage ModNoRelease => get(OsuIconMapping.ModNoRelease);
        public static IconUsage ModNoScope => get(OsuIconMapping.ModNoScope);
        public static IconUsage ModOneKey => get(OsuIconMapping.ModOneKey);
        public static IconUsage ModPerfect => get(OsuIconMapping.ModPerfect);
        public static IconUsage ModRandom => get(OsuIconMapping.ModRandom);
        public static IconUsage ModRelax => get(OsuIconMapping.ModRelax);
        public static IconUsage ModRepel => get(OsuIconMapping.ModRepel);
        public static IconUsage ModScoreV2 => get(OsuIconMapping.ModScoreV2);
        public static IconUsage ModSevenKeys => get(OsuIconMapping.ModSevenKeys);
        public static IconUsage ModSimplifiedRhythm => get(OsuIconMapping.ModSimplifiedRhythm);
        public static IconUsage ModSingleTap => get(OsuIconMapping.ModSingleTap);
        public static IconUsage ModSixKeys => get(OsuIconMapping.ModSixKeys);
        public static IconUsage ModSpinIn => get(OsuIconMapping.ModSpinIn);
        public static IconUsage ModSpunOut => get(OsuIconMapping.ModSpunOut);
        public static IconUsage ModStrictTracking => get(OsuIconMapping.ModStrictTracking);
        public static IconUsage ModSuddenDeath => get(OsuIconMapping.ModSuddenDeath);
        public static IconUsage ModSwap => get(OsuIconMapping.ModSwap);
        public static IconUsage ModSynesthesia => get(OsuIconMapping.ModSynesthesia);
        public static IconUsage ModTargetPractice => get(OsuIconMapping.ModTargetPractice);
        public static IconUsage ModTenKeys => get(OsuIconMapping.ModTenKeys);
        public static IconUsage ModThreeKeys => get(OsuIconMapping.ModThreeKeys);
        public static IconUsage ModTouchDevice => get(OsuIconMapping.ModTouchDevice);
        public static IconUsage ModTraceable => get(OsuIconMapping.ModTraceable);
        public static IconUsage ModTransform => get(OsuIconMapping.ModTransform);
        public static IconUsage ModTwoKeys => get(OsuIconMapping.ModTwoKeys);
        public static IconUsage ModWiggle => get(OsuIconMapping.ModWiggle);
        public static IconUsage ModWindDown => get(OsuIconMapping.ModWindDown);
        public static IconUsage ModWindUp => get(OsuIconMapping.ModWindUp);

        private static IconUsage get(OsuIconMapping glyph) => new IconUsage((char)glyph, FONT_NAME);

        private enum OsuIconMapping
        {
            [Description(@"Logo")]
            Logo,

            [Description(@"RulesetOsu")]
            RulesetOsu,

            [Description(@"RulesetMania")]
            RulesetMania,

            [Description(@"RulesetCatch")]
            RulesetCatch,

            [Description(@"RulesetTaiko")]
            RulesetTaiko,

            [Description(@"EditCircle")]
            EditCircle,

            [Description(@"LeftCircle")]
            LeftCircle,

            [Description(@"RightCircle")]
            RightCircle,

            [Description(@"audio")]
            Audio,

            [Description(@"beatmap")]
            Beatmap,

            [Description(@"calendar")]
            Calendar,

            [Description(@"changelog-a")]
            ChangelogA,

            [Description(@"changelog-b")]
            ChangelogB,

            [Description(@"chat")]
            Chat,

            [Description(@"check-circle")]
            CheckCircle,

            [Description(@"clock")]
            Clock,

            [Description(@"collapse-a")]
            CollapseA,

            [Description(@"collections")]
            Collections,

            [Description(@"cross")]
            Cross,

            [Description(@"cross-circle")]
            CrossCircle,

            [Description(@"crown")]
            Crown,

            [Description(@"daily-challenge")]
            DailyChallenge,

            [Description(@"debug")]
            Debug,

            [Description(@"delete")]
            Delete,

            [Description(@"details")]
            Details,

            [Description(@"discord")]
            Discord,

            [Description(@"ellipsis-horizontal")]
            EllipsisHorizontal,

            [Description(@"ellipsis-vertical")]
            EllipsisVertical,

            [Description(@"expand-a")]
            ExpandA,

            [Description(@"expand-b")]
            ExpandB,

            [Description(@"featured-artist")]
            FeaturedArtist,

            [Description(@"featured-artist-circle")]
            FeaturedArtistCircle,

            [Description(@"gameplay-a")]
            GameplayA,

            [Description(@"gameplay-b")]
            GameplayB,

            [Description(@"gameplay-c")]
            GameplayC,

            [Description(@"global")]
            Global,

            [Description(@"graphics")]
            Graphics,

            [Description(@"heart")]
            Heart,

            [Description(@"home")]
            Home,

            [Description(@"input")]
            Input,

            [Description(@"maintenance")]
            Maintenance,

            [Description(@"megaphone")]
            Megaphone,

            [Description(@"metronome")]
            Metronome,

            [Description(@"music")]
            Music,

            [Description(@"news")]
            News,

            [Description(@"next")]
            Next,

            [Description(@"next-circle")]
            NextCircle,

            [Description(@"notification")]
            Notification,

            [Description(@"online")]
            Online,

            [Description(@"play")]
            Play,

            [Description(@"player")]
            Player,

            [Description(@"player-follow")]
            PlayerFollow,

            [Description(@"prev")]
            Prev,

            [Description(@"prev-circle")]
            PrevCircle,

            [Description(@"ranking")]
            Ranking,

            [Description(@"rulesets")]
            Rulesets,

            [Description(@"search")]
            Search,

            [Description(@"settings")]
            Settings,

            [Description(@"skin-a")]
            SkinA,

            [Description(@"skin-b")]
            SkinB,

            [Description(@"star")]
            Star,

            [Description(@"storyboard")]
            Storyboard,

            [Description(@"team")]
            Team,

            [Description(@"thumbs-up")]
            ThumbsUp,

            [Description(@"tournament")]
            Tournament,

            [Description(@"twitter")]
            Twitter,

            [Description(@"user-interface")]
            UserInterface,

            [Description(@"wiki")]
            Wiki,

            [Description(@"Editor/add-control-point")]
            EditorAddControlPoint = 1000,

            [Description(@"Editor/convert-to-stream")]
            EditorConvertToStream,

            [Description(@"Editor/distance-snap")]
            EditorDistanceSnap,

            [Description(@"Editor/finish")]
            EditorFinish,

            [Description(@"Editor/grid-snap")]
            EditorGridSnap,

            [Description(@"Editor/new-combo-a")]
            EditorNewComboA,

            [Description(@"Editor/new-combo-b")]
            EditorNewComboB,

            [Description(@"Editor/select")]
            EditorSelect,

            [Description(@"Editor/sound")]
            EditorSound,

            [Description(@"Editor/whistle")]
            EditorWhistle,

            [Description(@"tortoise")]
            Tortoise,

            [Description(@"hare")]
            Hare,

            // mod icons

            [Description(@"Mods/mod-no-mod")]
            ModNoMod,

            /*
             rest can be regenerated semi-automatically using osu-web's mod database via
             $ jq -r '.[].Mods[].Name' mods.json | sort | uniq | \
               awk '{kebab = $0; gsub(" ", "-", kebab); pascal = $0; gsub(" ", "", pascal); print "[Description(@\"Mods/mod-" tolower(kebab) "\")]\nMod" pascal ",\n" }' | pbcopy
             */

            [Description(@"Mods/mod-accuracy-challenge")]
            ModAccuracyChallenge,

            [Description(@"Mods/mod-adaptive-speed")]
            ModAdaptiveSpeed,

            [Description(@"Mods/mod-alternate")]
            ModAlternate,

            [Description(@"Mods/mod-approach-different")]
            ModApproachDifferent,

            [Description(@"Mods/mod-autopilot")]
            ModAutopilot,

            [Description(@"Mods/mod-autoplay")]
            ModAutoplay,

            [Description(@"Mods/mod-barrel-roll")]
            ModBarrelRoll,

            [Description(@"Mods/mod-blinds")]
            ModBlinds,

            [Description(@"Mods/mod-bloom")]
            ModBloom,

            [Description(@"Mods/mod-bubbles")]
            ModBubbles,

            [Description(@"Mods/mod-cinema")]
            ModCinema,

            [Description(@"Mods/mod-classic")]
            ModClassic,

            [Description(@"Mods/mod-constant-speed")]
            ModConstantSpeed,

            [Description(@"Mods/mod-cover")]
            ModCover,

            [Description(@"Mods/mod-daycore")]
            ModDaycore,

            [Description(@"Mods/mod-deflate")]
            ModDeflate,

            [Description(@"Mods/mod-depth")]
            ModDepth,

            [Description(@"Mods/mod-difficulty-adjust")]
            ModDifficultyAdjust,

            [Description(@"Mods/mod-double-time")]
            ModDoubleTime,

            [Description(@"Mods/mod-dual-stages")]
            ModDualStages,

            [Description(@"Mods/mod-easy")]
            ModEasy,

            [Description(@"Mods/mod-eight-keys")]
            ModEightKeys,

            [Description(@"Mods/mod-fade-in")]
            ModFadeIn,

            [Description(@"Mods/mod-five-keys")]
            ModFiveKeys,

            [Description(@"Mods/mod-flashlight")]
            ModFlashlight,

            [Description(@"Mods/mod-floating-fruits")]
            ModFloatingFruits,

            [Description(@"Mods/mod-four-keys")]
            ModFourKeys,

            [Description(@"Mods/mod-freeze-frame")]
            ModFreezeFrame,

            [Description(@"Mods/mod-grow")]
            ModGrow,

            [Description(@"Mods/mod-half-time")]
            ModHalfTime,

            [Description(@"Mods/mod-hard-rock")]
            ModHardRock,

            [Description(@"Mods/mod-hidden")]
            ModHidden,

            [Description(@"Mods/mod-hold-off")]
            ModHoldOff,

            [Description(@"Mods/mod-invert")]
            ModInvert,

            [Description(@"Mods/mod-magnetised")]
            ModMagnetised,

            [Description(@"Mods/mod-mirror")]
            ModMirror,

            [Description(@"Mods/mod-moving-fast")]
            ModMovingFast,

            [Description(@"Mods/mod-muted")]
            ModMuted,

            [Description(@"Mods/mod-nightcore")]
            ModNightcore,

            [Description(@"Mods/mod-nine-keys")]
            ModNineKeys,

            [Description(@"Mods/mod-no-fail")]
            ModNoFail,

            [Description(@"Mods/mod-no-release")]
            ModNoRelease,

            [Description(@"Mods/mod-no-scope")]
            ModNoScope,

            [Description(@"Mods/mod-one-key")]
            ModOneKey,

            [Description(@"Mods/mod-perfect")]
            ModPerfect,

            [Description(@"Mods/mod-random")]
            ModRandom,

            [Description(@"Mods/mod-relax")]
            ModRelax,

            [Description(@"Mods/mod-repel")]
            ModRepel,

            [Description(@"Mods/mod-score-v2")]
            ModScoreV2,

            [Description(@"Mods/mod-seven-keys")]
            ModSevenKeys,

            [Description(@"Mods/mod-simplified-rhythm")]
            ModSimplifiedRhythm,

            [Description(@"Mods/mod-single-tap")]
            ModSingleTap,

            [Description(@"Mods/mod-six-keys")]
            ModSixKeys,

            [Description(@"Mods/mod-spin-in")]
            ModSpinIn,

            [Description(@"Mods/mod-spun-out")]
            ModSpunOut,

            [Description(@"Mods/mod-strict-tracking")]
            ModStrictTracking,

            [Description(@"Mods/mod-sudden-death")]
            ModSuddenDeath,

            [Description(@"Mods/mod-swap")]
            ModSwap,

            [Description(@"Mods/mod-synesthesia")]
            ModSynesthesia,

            [Description(@"Mods/mod-target-practice")]
            ModTargetPractice,

            [Description(@"Mods/mod-ten-keys")]
            ModTenKeys,

            [Description(@"Mods/mod-three-keys")]
            ModThreeKeys,

            [Description(@"Mods/mod-touch-device")]
            ModTouchDevice,

            [Description(@"Mods/mod-traceable")]
            ModTraceable,

            [Description(@"Mods/mod-transform")]
            ModTransform,

            [Description(@"Mods/mod-two-keys")]
            ModTwoKeys,

            [Description(@"Mods/mod-wiggle")]
            ModWiggle,

            [Description(@"Mods/mod-wind-down")]
            ModWindDown,

            [Description(@"Mods/mod-wind-up")]
            ModWindUp,
        }

        public class OsuIconStore : ITextureStore, ITexturedGlyphLookupStore
        {
            private readonly TextureStore textures;

            public OsuIconStore(TextureStore textures)
            {
                this.textures = textures;
            }

            public ITexturedCharacterGlyph? Get(string? fontName, char character)
            {
                if (fontName == FONT_NAME)
                    return new Glyph(textures.Get($@"{fontName}/{((OsuIconMapping)character).GetDescription()}"));

                return null;
            }

            public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

            public Texture? Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public Texture Get(string name) => throw new NotImplementedException();

            public Task<Texture> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Stream GetStream(string name) => throw new NotImplementedException();

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public Task<Texture?> GetAsync(string name, WrapMode wrapModeS, WrapMode wrapModeT, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public class Glyph : ITexturedCharacterGlyph
            {
                public float XOffset => default;
                public float YOffset => default;
                public float XAdvance => default;
                public float Baseline => default;
                public char Character => default;

                public float GetKerning<T>(T lastGlyph) where T : ICharacterGlyph => throw new NotImplementedException();

                public Texture Texture { get; }
                public float Width => Texture.Width;
                public float Height => Texture.Height;

                public Glyph(Texture texture)
                {
                    Texture = texture;
                }
            }

            public void Dispose()
            {
                textures.Dispose();
            }
        }
    }
}
