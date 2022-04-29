// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Utils;

namespace osu.Game.Beatmaps.Drawables
{
    public class BundledBeatmapDownloader : CompositeDrawable
    {
        private readonly bool shouldPostNotifications;

        public IEnumerable<BeatmapDownloadTracker> DownloadTrackers => downloadTrackers;

        private readonly List<BeatmapDownloadTracker> downloadTrackers = new List<BeatmapDownloadTracker>();

        private readonly List<string> downloadableFilenames = new List<string>();

        private BundledBeatmapModelDownloader beatmapDownloader;

        /// <summary>
        /// Construct a new beatmap downloader.
        /// </summary>
        /// <param name="onlyTutorial">Whether only the tutorial should be downloaded, instead of bundled beatmaps.</param>
        /// <param name="shouldPostNotifications">Whether downloads should create tracking notifications.</param>
        public BundledBeatmapDownloader(bool onlyTutorial, bool shouldPostNotifications = false)
        {
            this.shouldPostNotifications = shouldPostNotifications;

            if (onlyTutorial)
            {
                queueDownloads(new[] { tutorial_filename });
            }
            else
            {
                queueDownloads(always_bundled_beatmaps);

                queueDownloads(bundled_osu, 8);
                queueDownloads(bundled_taiko, 3);
                queueDownloads(bundled_catch, 3);
                queueDownloads(bundled_mania, 3);
            }
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var localDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            localDependencies.CacheAs<BeatmapModelDownloader>(beatmapDownloader = new BundledBeatmapModelDownloader(parent.Get<BeatmapManager>(), parent.Get<IAPIProvider>()));

            if (shouldPostNotifications && parent.Get<INotificationOverlay>() is INotificationOverlay notifications)
                beatmapDownloader.PostNotification = notifications.Post;

            return localDependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (string filename in downloadableFilenames)
            {
                var match = Regex.Match(filename, @"([0-9]*) (.*) - (.*)\.osz");

                var beatmapSet = new APIBeatmapSet
                {
                    OnlineID = int.Parse(match.Groups[1].Value),
                    Artist = match.Groups[2].Value,
                    Title = match.Groups[3].Value,
                };

                var beatmapDownloadTracker = new BeatmapDownloadTracker(beatmapSet);
                downloadTrackers.Add(beatmapDownloadTracker);
                AddInternal(beatmapDownloadTracker);

                beatmapDownloader.Download(beatmapSet);
            }
        }

        private void queueDownloads(string[] sourceFilenames, int? limit = null)
        {
            try
            {
                // Matches osu-stable, in order to provide new users with roughly the same randomised selection of bundled beatmaps.
                var random = new LegacyRandom(DateTime.UtcNow.Year * 1000 + (DateTime.UtcNow.DayOfYear / 7));

                downloadableFilenames.AddRange(sourceFilenames.OrderBy(x => random.NextDouble()).Take(limit ?? int.MaxValue));
            }
            catch { }
        }

        private class BundledBeatmapModelDownloader : BeatmapModelDownloader
        {
            public BundledBeatmapModelDownloader(IModelImporter<BeatmapSetInfo> beatmapImporter, IAPIProvider api)
                : base(beatmapImporter, api)
            {
            }

            protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
                => new BundledBeatmapDownloadRequest(set, minimiseDownloadSize);

            public class BundledBeatmapDownloadRequest : DownloadBeatmapSetRequest
            {
                protected override string Uri => $"https://assets.ppy.sh/client-resources/bundled/{Model.OnlineID}.osz";

                public BundledBeatmapDownloadRequest(IBeatmapSetInfo beatmapSetInfo, bool minimiseDownloadSize)
                    : base(beatmapSetInfo, minimiseDownloadSize)
                {
                }
            }
        }

        private const string tutorial_filename = "1011011 nekodex - new beginnings.osz";

        /// <summary>
        /// Contest winners or other special cases.
        /// </summary>
        private static readonly string[] always_bundled_beatmaps =
        {
            // This thing is 40mb, I'm not sure we want it here...
            @"1388906 Raphlesia & BilliumMoto - My Love.osz"
        };

        private static readonly string[] bundled_osu =
        {
            "682286 Yuyoyuppe - Emerald Galaxy.osz",
            "682287 baker - For a Dead Girl+.osz",
            "682289 Hige Driver - I Wanna Feel Your Love (feat. shully).osz",
            "682290 Hige Driver - Miracle Sugite Yabai (feat. shully).osz",
            "682416 Hige Driver - Palette.osz",
            "682595 baker - Kimi ga Kimi ga -vocanico remix-.osz",
            "716211 yuki. - Spring Signal.osz",
            "716213 dark cat - BUBBLE TEA (feat. juu & cinders).osz",
            "716215 LukHash - CLONED.osz",
            "716219 IAHN - Snowdrop.osz",
            "716249 *namirin - Senaka Awase no Kuukyo (with Kakichoco).osz",
            "716390 sakuraburst - SHA.osz",
            "716441 Fractal Dreamers - Paradigm Shift.osz",
            "729808 Thaehan - Leprechaun.osz",
            "751771 Cranky - Hanaarashi.osz",
            "751772 Cranky - Ran.osz",
            "751773 Cranky - Feline, the White....osz",
            "751774 Function Phantom - Variable.osz",
            "751779 Rin - Daishibyo set 14 ~ Sado no Futatsuiwa.osz",
            "751782 Fractal Dreamers - Fata Morgana.osz",
            "751785 Cranky - Chandelier - King.osz",
            "751846 Fractal Dreamers - Celestial Horizon.osz",
            "751866 Rin - Moriya set 08 ReEdit ~ Youkai no Yama.osz",
            "751894 Fractal Dreamers - Blue Haven.osz",
            "751896 Cranky - Rave 2 Rave.osz",
            "751932 Cranky - La fuite des jours.osz",
            "751972 Cranky - CHASER.osz",
            "779173 Thaehan - Superpower.osz",
            "780932 VINXIS - A Centralized View.osz",
            "785572 S3RL - I'll See You Again (feat. Chi Chi).osz",
            "785650 yuki. feat. setsunan - Hello! World.osz",
            "785677 Dictate - Militant.osz",
            "785731 S3RL - Catchit (Radio Edit).osz",
            "785774 LukHash - GLITCH.osz",
            "786498 Trial & Error - Tokoyami no keiyaku KEGARETA-SHOUJO feat. GUMI.osz",
            "789374 Pulse - LP.osz",
            "789528 James Portland - Sky.osz",
            "789529 Lexurus - Gravity.osz",
            "789544 Andromedik - Invasion.osz",
            "789905 Gourski x Himmes - Silence.osz",
            "791667 cYsmix - Babaroque (Short Ver.).osz",
            "791798 cYsmix - Behind the Walls.osz",
            "791845 cYsmix - Little Knight.osz",
            "792241 cYsmix - Eden.osz",
            "792396 cYsmix - The Ballad of a Mindless Girl.osz",
            "795432 Phonetic - Journey.osz",
            "831322 DJ'TEKINA//SOMETHING - Hidamari no Uta.osz",
            "847764 Cranky - Crocus.osz",
            "847776 Culprate & Joe Ford - Gaucho.osz",
            "847812 J. Pachelbel - Canon (Cranky Remix).osz",
            "847900 Cranky - Time Alter.osz",
            "847930 LukHash - 8BIT FAIRY TALE.osz",
            "848003 Culprate - Aurora.osz",
            "848068 nanobii - popsicle beach.osz",
            "848090 Trial & Error - DAI*TAN SENSATION feat. Nanahira, Mii, Aitsuki Nakuru (Short Ver.).osz",
            "848259 Culprate & Skorpion - Jester.osz",
            "848976 Dictate - Treason.osz",
            "851543 Culprate - Florn.osz",
            "864748 Thaehan - Angry Birds Epic (Remix).osz",
            "873667 OISHII - ONIGIRI FREEWAY.osz",
            "876227 Culprate, Keota & Sophie Meiers - Mechanic Heartbeat.osz",
            "880487 cYsmix - Peer Gynt.osz",
            "883088 Wisp X - Somewhere I'd Rather Be.osz",
            "891333 HyuN - White Aura.osz",
            "891334 HyuN - Wild Card.osz",
            "891337 HyuN feat. LyuU - Cross Over.osz",
            "891338 HyuN & Ritoru - Apocalypse in Love.osz",
            "891339 HyuN feat. Ato - Asu wa Ame ga Yamukara.osz",
            "891345 HyuN - Infinity Heaven.osz",
            "891348 HyuN - Guitian.osz",
            "891356 HyuN - Legend of Genesis.osz",
            "891366 HyuN - Illusion of Inflict.osz",
            "891417 HyuN feat. Yu-A - My life is for you.osz",
            "891441 HyuN - You'Re aRleAdY dEAd.osz",
            "891632 HyuN feat. YURI - Disorder.osz",
            "891712 HyuN - Tokyo's Starlight.osz",
            "901091 *namirin - Ciel etoile.osz",
            "916990 *namirin - Koishiteiku Planet.osz",
            "929284 tieff - Sense of Nostalgia.osz",
            "933940 Ben Briggs - Yes (Maybe).osz",
            "934415 Ben Briggs - Fearless Living.osz",
            "934627 Ben Briggs - New Game Plus.osz",
            "934666 Ben Briggs - Wave Island.osz",
            "936126 siromaru + cranky - conflict.osz",
            "940377 onumi - ARROGANCE.osz",
            "940597 tieff - Take Your Swimsuit.osz",
            "941085 tieff - Our Story.osz",
            "949297 tieff - Sunflower.osz",
            "952380 Ben Briggs - Why Are We Yelling.osz",
            "954272 *namirin - Kanzen Shouri*Esper Girl.osz",
            "955866 KIRA & Heartbreaker - B.B.F (feat. Hatsune Miku & Kagamine Rin).osz",
            "961320 Kuba Oms - All In All.osz",
            "964553 The Flashbulb - You Take the World's Weight Away.osz",
            "965651 Fractal Dreamers - Ad Astra.osz",
            "966225 The Flashbulb - Passage D.osz",
            "966324 DJ'TEKINA//SOMETHING - Hidamari no Uta.osz",
            "972810 James Landino & Kabuki - Birdsong.osz",
            "972932 James Landino - Hide And Seek.osz",
            "977276 The Flashbulb - Mellann.osz",
            "981616 *namirin - Mizutamari Tobikoete (with Nanahira).osz",
            "985788 Loki - Wizard's Tower.osz",
            "996628 OISHII - ONIGIRI FREEWAY.osz",
            "996898 HyuN - White Aura.osz",
            "1003554 yuki. - Nadeshiko Sensation.osz",
            "1014936 Thaehan - Bwa !.osz",
            "1019827 UNDEAD CORPORATION - Sad Dream.osz",
            "1020213 Creo - Idolize.osz",
            "1021450 Thaehan - Chiptune & Baroque.osz",
        };

        private static readonly string[] bundled_taiko =
        {
            "707824 Fractal Dreamers - Fortuna Redux.osz",
            "789553 Cranky - Ran.osz",
            "827822 Function Phantom - Neuronecia.osz",
            "847323 Nakanojojo - Bittersweet (feat. Kuishinboakachan a.k.a Kiato).osz",
            "847433 Trial & Error - Tokoyami no keiyaku KEGARETA-SHOUJO feat. GUMI.osz",
            "847576 dark cat - hot chocolate.osz",
            "847957 Wisp X - Final Moments.osz",
            "876282 VINXIS - Greetings.osz",
            "876648 Thaehan - Angry Birds Epic (Remix).osz",
            "877069 IAHN - Transform (Original Mix).osz",
            "877496 Thaehan - Leprechaun.osz",
            "877935 Thaehan - Overpowered.osz",
            "878344 yuki. - Be Your Light.osz",
            "918446 VINXIS - Facade.osz",
            "918903 LukHash - Ghosts.osz",
            "919251 *namirin - Hitokoto no Kyori.osz",
            "919704 S3RL - I Will Pick You Up (feat. Tamika).osz",
            "921535 SOOOO - Raven Haven.osz",
            "927206 *namirin - Kanzen Shouri*Esper Girl.osz",
            "927544 Camellia feat. Nanahira - Kansoku Eisei.osz",
            "930806 Nakanojojo - Pararara (feat. Amekoya).osz",
            "931741 Camellia - Quaoar.osz",
            "935699 Rin - Mythic set ~ Heart-Stirring Urban Legends.osz",
            "935732 Thaehan - Yuujou.osz",
            "941145 Function Phantom - Euclid.osz",
            "942334 Dictate - Cauldron.osz",
            "946540 nanobii - astral blast.osz",
            "948844 Rin - Kishinjou set 01 ~ Mist Lake.osz",
            "949122 Wisp X - Petal.osz",
            "951618 Rin - Kishinjou set 02 ~ Mermaid from the Uncharted Land.osz",
            "957412 Rin - Lunatic set 16 ~ The Space Shrine Maiden Returns Home.osz",
            "961335 Thaehan - Insert Coin.osz",
            "965178 The Flashbulb - DIDJ PVC.osz",
            "966087 The Flashbulb - Creep.osz",
            "966277 The Flashbulb - Amen Iraq.osz",
            "966407 LukHash - ROOM 12.osz",
            "966451 The Flashbulb - Six Acid Strings.osz",
            "972301 BilliumMoto - four veiled stars.osz",
            "973173 nanobii - popsicle beach.osz",
            "973954 BilliumMoto - Rocky Buinne (Short Ver.).osz",
            "975435 BilliumMoto - life flashes before weeb eyes.osz",
            "978759 L. V. Beethoven - Moonlight Sonata (Cranky Remix).osz",
            "982559 BilliumMoto - HDHR.osz",
            "984361 The Flashbulb - Ninedump.osz",
            "1023681 Inferi - The Ruin of Mankind.osz",
            "1034358 ALEPH - The Evil Spirit.osz",
            "1037567 ALEPH - Scintillations.osz",
        };

        private static readonly string[] bundled_catch =
        {
            "554256 Helblinde - When Time Sleeps.osz",
            "693123 yuki. - Nadeshiko Sensation.osz",
            "767009 OISHII - PIZZA PLAZA.osz",
            "767346 Thaehan - Bwa !.osz",
            "815162 VINXIS - Greetings.osz",
            "840964 cYsmix - Breeze.osz",
            "932657 Wisp X - Eventide.osz",
            "933700 onumi - CONFUSION PART ONE.osz",
            "933984 onumi - PERSONALITY.osz",
            "934785 onumi - FAKE.osz",
            "936545 onumi - REGRET PART ONE.osz",
            "943803 Fractal Dreamers - Everything for a Dream.osz",
            "943876 S3RL - I Will Pick You Up (feat. Tamika).osz",
            "946773 Trial & Error - DREAMING COLOR (Short Ver.).osz",
            "955808 Trial & Error - Tokoyami no keiyaku KEGARETA-SHOUJO feat. GUMI (Short Ver.).osz",
            "957808 Fractal Dreamers - Module_410.osz",
            "957842 antiPLUR - One Life Left to Live.osz",
            "965730 The Flashbulb - Lawn Wake IV (Black).osz",
            "966240 Creo - Challenger.osz",
            "968232 Rin - Lunatic set 15 ~ The Moon as Seen from the Shrine.osz",
            "972302 VINXIS - A Centralized View.osz",
            "972887 HyuN - Illusion of Inflict.osz",
            "1008600 LukHash - WHEN AN ANGEL DIES.osz",
            "1032103 LukHash - H8 U.osz",
        };

        private static readonly string[] bundled_mania =
        {
            "943516 antiPLUR - Clockwork Spooks.osz",
            "946394 VINXIS - Three Times The Original Charm.osz",
            "966408 antiPLUR - One Life Left to Live.osz",
            "971561 antiPLUR - Runengon.osz",
            "983864 James Landino - Shiba Island.osz",
            "989512 BilliumMoto - 1xMISS.osz",
            "994104 James Landino - Reaction feat. Slyleaf.osz",
            "1003217 nekodex - circles!.osz",
            "1009907 James Landino & Kabuki - Birdsong.osz",
            "1015169 Thaehan - Insert Coin.osz",
        };
    }
}
