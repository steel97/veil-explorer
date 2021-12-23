//import { DateTime } from "luxon";

export const useFormatting = () => {
    const formatDateLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        return date.toLocaleDateString();
    };

    const formatDateTimeLocal = (utcTime: number) => {
        //const date = DateTime.fromMillis(utcTime)
        //return date.toLocaleString(DateTime.DATE_SHORT);
        const date = new Date(utcTime * 1000);
        return date.toLocaleDateString() + " " + date.toLocaleTimeString();
    };

    return { formatDateLocal, formatDateTimeLocal };
}