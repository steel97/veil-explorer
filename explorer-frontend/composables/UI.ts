export const useUI = () => {
    const scrollToAnimated = (element: HTMLElement, to: number, duration: number) => {
        if (duration <= 0) return;
        var difference = to - element.scrollTop;
        var perTick = difference / duration * 10;

        setTimeout(function () {
            element.scrollTop = element.scrollTop + perTick;
            if (element.scrollTop === to) return;
            scrollToAnimated(element, to, duration - 10);
        }, 10);
    };

    return { scrollToAnimated };
}