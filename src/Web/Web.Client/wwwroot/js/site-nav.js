export function scrollToId(id) {
    const tryScroll = (attemptsLeft) => {
        const el = document.getElementById(id);
        if (el) {
            el.scrollIntoView({ behavior: "smooth", block: "start" });
            return;
        }

        if (attemptsLeft > 0)
            requestAnimationFrame(() => tryScroll(attemptsLeft - 1));
    };

    tryScroll(120);
}

export function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}
