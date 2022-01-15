import { useI18n } from "vue-i18n";

export const useLocalization = () => {

    const headers = useRequestHeaders(["accept-language"]);
    const { availableLocales, fallbackLocale } = useI18n();

    const getClientLocaleIgnoreCheck = () => {
        const acceptLanguages = headers["accept-language"];
        let targetLocale: string | null = null;

        if (acceptLanguages != null) {
            const langs = acceptLanguages.split(";")[0].split(",");
            for (const entr of langs) {
                if (entr.startsWith("q=")) continue;
                targetLocale = entr;
                break;
            }
        }

        const retLocale = targetLocale ?? (process.client ? navigator.language ?? "en-US" : "en-US");
        return retLocale == "*" ? "en-US" : retLocale;
    };

    const getClientLocale = () => {
        const acceptLanguages = headers["accept-language"];
        const acceptLanguagesPr: Array<string> = [];
        if (acceptLanguages != null) {
            acceptLanguages.split(";").forEach(langEntry => {
                try {
                    const data = langEntry.split(",");
                    for (const entr of data) {
                        if (entr.startsWith("q=")) continue;
                        const lang = entr.split("-")[0];
                        if (acceptLanguagesPr.indexOf(entr) == -1)
                            acceptLanguagesPr.push(entr); // add language and region
                        if (acceptLanguagesPr.indexOf(lang) == -1)
                            acceptLanguagesPr.push(lang); // add language
                    }
                } catch {

                }
            });
        }
        const navlocales = process.client ? (navigator.languages ?? ["en-US", "en"]) : ["en-US", "en"];
        const locales = acceptLanguagesPr.length > 0 ? acceptLanguagesPr : navlocales;

        for (const locale of locales) {
            if (availableLocales.indexOf(locale) > -1) {
                return locale;
            }
        }

        return fallbackLocale.value;
    };

    return { getClientLocale, getClientLocaleIgnoreCheck };
}