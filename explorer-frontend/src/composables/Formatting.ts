//import { DateTime } from "luxon";

import { useLocalization } from "@/composables/Localization";

export const useFormatting = () => {
    const { getClientLocaleIgnoreCheck } = useLocalization();

    const formatDateLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        return date.toLocaleDateString(getClientLocaleIgnoreCheck());
    };

    const formatDateTimeLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        return date.toLocaleDateString(getClientLocaleIgnoreCheck()) + " " + date.toLocaleTimeString(getClientLocaleIgnoreCheck());
    };

    const formatTimeLocal = (utcTime: number) => {
        const date = new Date(utcTime * 1000);
        return date.toLocaleTimeString(getClientLocaleIgnoreCheck());
    };

    return { formatDateLocal, formatDateTimeLocal, formatTimeLocal };
}