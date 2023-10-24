// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Users
{
    [JsonConverter(typeof(StringEnumConverter))]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum CountryCode
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("Bangladesh")]
        BD,

        [Description("Belgium")]
        BE,

        [Description("Burkina Faso")]
        BF,

        [Description("Bulgaria")]
        BG,

        [Description("Bosnia and Herzegovina")]
        BA,

        [Description("Barbados")]
        BB,

        [Description("Wallis and Futuna")]
        WF,

        [Description("Saint Barthelemy")]
        BL,

        [Description("Bermuda")]
        BM,

        [Description("Brunei")]
        BN,

        [Description("Bolivia")]
        BO,

        [Description("Bahrain")]
        BH,

        [Description("Burundi")]
        BI,

        [Description("Benin")]
        BJ,

        [Description("Bhutan")]
        BT,

        [Description("Jamaica")]
        JM,

        [Description("Bouvet Island")]
        BV,

        [Description("Botswana")]
        BW,

        [Description("Samoa")]
        WS,

        [Description("Bonaire, Saint Eustatius and Saba")]
        BQ,

        [Description("Brazil")]
        BR,

        [Description("Bahamas")]
        BS,

        [Description("Jersey")]
        JE,

        [Description("Belarus")]
        BY,

        [Description("Belize")]
        BZ,

        [Description("Russian Federation")]
        RU,

        [Description("Rwanda")]
        RW,

        [Description("Serbia")]
        RS,

        [Description("East Timor")]
        TL,

        [Description("Reunion")]
        RE,

        [Description("Turkmenistan")]
        TM,

        [Description("Tajikistan")]
        TJ,

        [Description("Romania")]
        RO,

        [Description("Tokelau")]
        TK,

        [Description("Guinea-Bissau")]
        GW,

        [Description("Guam")]
        GU,

        [Description("Guatemala")]
        GT,

        [Description("South Georgia and the South Sandwich Islands")]
        GS,

        [Description("Greece")]
        GR,

        [Description("Equatorial Guinea")]
        GQ,

        [Description("Guadeloupe")]
        GP,

        [Description("Japan")]
        JP,

        [Description("Guyana")]
        GY,

        [Description("Guernsey")]
        GG,

        [Description("French Guiana")]
        GF,

        [Description("Georgia")]
        GE,

        [Description("Grenada")]
        GD,

        [Description("United Kingdom")]
        GB,

        [Description("Gabon")]
        GA,

        [Description("El Salvador")]
        SV,

        [Description("Guinea")]
        GN,

        [Description("Gambia")]
        GM,

        [Description("Greenland")]
        GL,

        [Description("Gibraltar")]
        GI,

        [Description("Ghana")]
        GH,

        [Description("Oman")]
        OM,

        [Description("Tunisia")]
        TN,

        [Description("Jordan")]
        JO,

        [Description("Croatia")]
        HR,

        [Description("Haiti")]
        HT,

        [Description("Hungary")]
        HU,

        [Description("Hong Kong")]
        HK,

        [Description("Honduras")]
        HN,

        [Description("Heard Island and McDonald Islands")]
        HM,

        [Description("Venezuela")]
        VE,

        [Description("Puerto Rico")]
        PR,

        [Description("Palestinian Territory")]
        PS,

        [Description("Palau")]
        PW,

        [Description("Portugal")]
        PT,

        [Description("Svalbard and Jan Mayen")]
        SJ,

        [Description("Paraguay")]
        PY,

        [Description("Iraq")]
        IQ,

        [Description("Panama")]
        PA,

        [Description("French Polynesia")]
        PF,

        [Description("Papua New Guinea")]
        PG,

        [Description("Peru")]
        PE,

        [Description("Pakistan")]
        PK,

        [Description("Philippines")]
        PH,

        [Description("Pitcairn")]
        PN,

        [Description("Poland")]
        PL,

        [Description("Saint Pierre and Miquelon")]
        PM,

        [Description("Zambia")]
        ZM,

        [Description("Western Sahara")]
        EH,

        [Description("Estonia")]
        EE,

        [Description("Egypt")]
        EG,

        [Description("South Africa")]
        ZA,

        [Description("Ecuador")]
        EC,

        [Description("Italy")]
        IT,

        [Description("Vietnam")]
        VN,

        [Description("Solomon Islands")]
        SB,

        [Description("Ethiopia")]
        ET,

        [Description("Somalia")]
        SO,

        [Description("Zimbabwe")]
        ZW,

        [Description("Saudi Arabia")]
        SA,

        [Description("Spain")]
        ES,

        [Description("Eritrea")]
        ER,

        [Description("Montenegro")]
        ME,

        [Description("Moldova")]
        MD,

        [Description("Madagascar")]
        MG,

        [Description("Saint Martin")]
        MF,

        [Description("Morocco")]
        MA,

        [Description("Monaco")]
        MC,

        [Description("Uzbekistan")]
        UZ,

        [Description("Myanmar")]
        MM,

        [Description("Mali")]
        ML,

        [Description("Macao")]
        MO,

        [Description("Mongolia")]
        MN,

        [Description("Marshall Islands")]
        MH,

        [Description("North Macedonia")]
        MK,

        [Description("Mauritius")]
        MU,

        [Description("Malta")]
        MT,

        [Description("Malawi")]
        MW,

        [Description("Maldives")]
        MV,

        [Description("Martinique")]
        MQ,

        [Description("Northern Mariana Islands")]
        MP,

        [Description("Montserrat")]
        MS,

        [Description("Mauritania")]
        MR,

        [Description("Isle of Man")]
        IM,

        [Description("Uganda")]
        UG,

        [Description("Tanzania")]
        TZ,

        [Description("Malaysia")]
        MY,

        [Description("Mexico")]
        MX,

        [Description("Israel")]
        IL,

        [Description("France")]
        FR,

        [Description("British Indian Ocean Territory")]
        IO,

        [Description("Saint Helena")]
        SH,

        [Description("Finland")]
        FI,

        [Description("Fiji")]
        FJ,

        [Description("Falkland Islands")]
        FK,

        [Description("Micronesia")]
        FM,

        [Description("Faroe Islands")]
        FO,

        [Description("Nicaragua")]
        NI,

        [Description("Netherlands")]
        NL,

        [Description("Norway")]
        NO,

        [Description("Namibia")]
        NA,

        [Description("Vanuatu")]
        VU,

        [Description("New Caledonia")]
        NC,

        [Description("Niger")]
        NE,

        [Description("Norfolk Island")]
        NF,

        [Description("Nigeria")]
        NG,

        [Description("New Zealand")]
        NZ,

        [Description("Nepal")]
        NP,

        [Description("Nauru")]
        NR,

        [Description("Niue")]
        NU,

        [Description("Cook Islands")]
        CK,

        [Description("Kosovo")]
        XK,

        [Description("Ivory Coast")]
        CI,

        [Description("Switzerland")]
        CH,

        [Description("Colombia")]
        CO,

        [Description("China")]
        CN,

        [Description("Cameroon")]
        CM,

        [Description("Chile")]
        CL,

        [Description("Cocos Islands")]
        CC,

        [Description("Canada")]
        CA,

        [Description("Republic of the Congo")]
        CG,

        [Description("Central African Republic")]
        CF,

        [Description("Democratic Republic of the Congo")]
        CD,

        [Description("Czech Republic")]
        CZ,

        [Description("Cyprus")]
        CY,

        [Description("Christmas Island")]
        CX,

        [Description("Costa Rica")]
        CR,

        [Description("Curacao")]
        CW,

        [Description("Cabo Verde")]
        CV,

        [Description("Cuba")]
        CU,

        [Description("Eswatini")]
        SZ,

        [Description("Syria")]
        SY,

        [Description("Sint Maarten")]
        SX,

        [Description("Kyrgyzstan")]
        KG,

        [Description("Kenya")]
        KE,

        [Description("South Sudan")]
        SS,

        [Description("Suriname")]
        SR,

        [Description("Kiribati")]
        KI,

        [Description("Cambodia")]
        KH,

        [Description("Saint Kitts and Nevis")]
        KN,

        [Description("Comoros")]
        KM,

        [Description("Sao Tome and Principe")]
        ST,

        [Description("Slovakia")]
        SK,

        [Description("South Korea")]
        KR,

        [Description("Slovenia")]
        SI,

        [Description("North Korea")]
        KP,

        [Description("Kuwait")]
        KW,

        [Description("Senegal")]
        SN,

        [Description("San Marino")]
        SM,

        [Description("Sierra Leone")]
        SL,

        [Description("Seychelles")]
        SC,

        [Description("Kazakhstan")]
        KZ,

        [Description("Cayman Islands")]
        KY,

        [Description("Singapore")]
        SG,

        [Description("Sweden")]
        SE,

        [Description("Sudan")]
        SD,

        [Description("Dominican Republic")]
        DO,

        [Description("Dominica")]
        DM,

        [Description("Djibouti")]
        DJ,

        [Description("Denmark")]
        DK,

        [Description("British Virgin Islands")]
        VG,

        [Description("Germany")]
        DE,

        [Description("Yemen")]
        YE,

        [Description("Algeria")]
        DZ,

        [Description("United States")]
        US,

        [Description("Uruguay")]
        UY,

        [Description("Mayotte")]
        YT,

        [Description("United States Minor Outlying Islands")]
        UM,

        [Description("Lebanon")]
        LB,

        [Description("Saint Lucia")]
        LC,

        [Description("Laos")]
        LA,

        [Description("Tuvalu")]
        TV,

        [Description("Taiwan")]
        TW,

        [Description("Trinidad and Tobago")]
        TT,

        [Description("Türkiye")]
        TR,

        [Description("Sri Lanka")]
        LK,

        [Description("Liechtenstein")]
        LI,

        [Description("Latvia")]
        LV,

        [Description("Tonga")]
        TO,

        [Description("Lithuania")]
        LT,

        [Description("Luxembourg")]
        LU,

        [Description("Liberia")]
        LR,

        [Description("Lesotho")]
        LS,

        [Description("Thailand")]
        TH,

        [Description("French Southern Territories")]
        TF,

        [Description("Togo")]
        TG,

        [Description("Chad")]
        TD,

        [Description("Turks and Caicos Islands")]
        TC,

        [Description("Libya")]
        LY,

        [Description("Vatican")]
        VA,

        [Description("Saint Vincent and the Grenadines")]
        VC,

        [Description("United Arab Emirates")]
        AE,

        [Description("Andorra")]
        AD,

        [Description("Antigua and Barbuda")]
        AG,

        [Description("Afghanistan")]
        AF,

        [Description("Anguilla")]
        AI,

        [Description("U.S. Virgin Islands")]
        VI,

        [Description("Iceland")]
        IS,

        [Description("Iran")]
        IR,

        [Description("Armenia")]
        AM,

        [Description("Albania")]
        AL,

        [Description("Angola")]
        AO,

        [Description("Antarctica")]
        AQ,

        [Description("American Samoa")]
        AS,

        [Description("Argentina")]
        AR,

        [Description("Australia")]
        AU,

        [Description("Austria")]
        AT,

        [Description("Aruba")]
        AW,

        [Description("India")]
        IN,

        [Description("Aland Islands")]
        AX,

        [Description("Azerbaijan")]
        AZ,

        [Description("Ireland")]
        IE,

        [Description("Indonesia")]
        ID,

        [Description("Ukraine")]
        UA,

        [Description("Qatar")]
        QA,

        [Description("Mozambique")]
        MZ,
    }
}
