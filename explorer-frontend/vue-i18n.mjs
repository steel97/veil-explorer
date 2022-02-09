import translation_en from "@/localization/en";
import translation_ru from "@/localization/ru";

export default async () => ({
    locale: "en",
    fallbackLocale: "en",
    messages: {
        en: translation_en,
        ru: translation_ru
    }
});