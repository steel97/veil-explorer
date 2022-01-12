//import { DateTime } from "luxon";
import { useI18n } from "vue-i18n";

import { useLocalization } from "@/composables/Localization";

export const useFormatting = () => {
    const { getClientLocaleIgnoreCheck } = useLocalization();
    const { fallbackLocale } = useI18n();

    const formatDateLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        try {
            return date.toLocaleDateString(getClientLocaleIgnoreCheck());
        } catch {
            return date.toLocaleDateString(fallbackLocale.value.toString());
        }
    };

    const formatDateTimeLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        try {
            return date.toLocaleDateString(getClientLocaleIgnoreCheck()) + " " + date.toLocaleTimeString(getClientLocaleIgnoreCheck());
        } catch {
            return date.toLocaleDateString(fallbackLocale.value.toString());
        }
    };

    const formatTimeLocal = (utcTime: number) => {
        const date = new Date(utcTime * 1000);
        try {
            return date.toLocaleTimeString(getClientLocaleIgnoreCheck());
        } catch {
            return date.toLocaleDateString(fallbackLocale.value.toString());
        }
    };

    return { formatDateLocal, formatDateTimeLocal, formatTimeLocal };
}