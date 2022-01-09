export interface Action<T, K> {
    (item: T, item2: K): void;
}

export const useUI = () => {

    const update = (lastDate: number, callback: Action<number, number>) => {
        const now = Date.now();
        const elapsed = now - lastDate;
        callback(elapsed, now);
    }

    // probably can be done in simplier way, just dont have time for this
    const scrollToAnimated = (element: HTMLElement, from: number, to: number, durationInitial: number, durationLeft: number, ltime = Date.now()) => {
        if (durationLeft <= 0) return;

        const x = 1.0 - durationLeft / durationInitial;
        if (x < 0) {
            element.scrollTop = to;
            return;
        }

        // https://easings.net/#easeOutCubic
        // return 1 - pow(1 - x, 3);
        const target = 1 - Math.pow(1 - x, 3);

        requestAnimationFrame(() => update(ltime, (elapsed: number, ltime: number) => {
            element.scrollTop = from - Math.abs(from - to) * target;
            if (element.scrollTop === to) return;
            scrollToAnimated(element, from, to, durationInitial, durationLeft - elapsed, ltime);
        }));
    };

    return { scrollToAnimated };
}