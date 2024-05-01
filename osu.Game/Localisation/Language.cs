// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace osu.Game.Localisation
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum Language
    {
        [Description(@"English")]
        en,

        // TODO: Requires Arabic glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"اَلْعَرَبِيَّةُ")]
        // ar,

        [Description(@"Беларуская мова")]
        be,

        [Description(@"Български")]
        bg,

        [Description(@"Català")]
        ca,

        [Description(@"Česky")]
        cs,

        [Description(@"Dansk")]
        da,

        [Description(@"Deutsch")]
        de,

        [Description(@"Ελληνικά")]
        el,

        [Description(@"español")]
        es,

        // TODO: Requires Arabic glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"فارسی")]
        // fa_ir,

        [Description(@"Suomi")]
        fi,

        // TODO: Doesn't work as appropriate satellite assemblies aren't copied from resources (see: https://github.com/ppy/osu/discussions/18851#discussioncomment-3042170)
        // [Description(@"Filipino")]
        // fil,

        [Description(@"français")]
        fr,

        // TODO: Requires Hebrew glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"עברית")]
        // he,

        [Description(@"Hrvatski")]
        hr_hr,

        [Description(@"Magyar")]
        hu,

        [Description(@"Bahasa Indonesia")]
        id,

        [Description(@"Italiano")]
        it,

        [Description(@"日本語")]
        ja,

        [Description(@"한국어")]
        ko,

        [Description(@"Lietuvių")]
        lt,

        [Description(@"Latviešu")]
        lv_lv,

        [Description(@"Melayu")]
        ms_my,

        [Description(@"Nederlands")]
        nl,

        [Description(@"Norsk")]
        no,

        [Description(@"polski")]
        pl,

        [Description(@"Português")]
        pt,

        [Description(@"Português (Brasil)")]
        pt_br,

        [Description(@"Română")]
        ro,

        [Description(@"Русский")]
        ru,

        // TODO: Requires Sinhala glyphs to be added to resources.
        // Additionally, no translations available yet.
        // [Description(@"සිංහල")]
        // si_lk,

        [Description(@"Slovenčina")]
        sk,

        [Description(@"Slovenščina")]
        sl,

        [Description(@"Српски")]
        sr,

        [Description(@"Svenska")]
        sv,

        // Tajik has no associated localisations yet, and is not supported on Windows versions <10.
        // TODO: update language mapping in osu-resources to redirect tg-TJ to tg-Cyrl-TJ (which is supported on earlier Windows versions)
        // [Description(@"Тоҷикӣ")]
        // tg_tj,

        [Description(@"ไทย")]
        th,

        // Tagalog has no associated localisations yet, and is not supported on Xamarin platforms or Windows versions <10.
        // Can be revisited if localisations ever arrive.
        //[Description(@"Tagalog")]
        //tl,

        [Description(@"Türkçe")]
        tr,

        [Description(@"Українська мова")]
        uk,

        [Description(@"Tiếng Việt")]
        vi,

        [Description(@"简体中文")]
        zh,

        // Traditional Chinese (Hong Kong) is listed in web sources but has no associated localisations,
        // and was wrongly falling back to Simplified Chinese.
        // Can be revisited if localisations ever arrive.
        // [Description(@"繁體中文（香港）")]
        // zh_hk,

        [Description(@"繁體中文（台灣）")]
        zh_hant,

#if DEBUG
        [Description(@"Debug (show raw keys)")]
        debug
#endif
    }
}
