window.radioPlayer = {
    setAndPlay: (audioEl, src, positionSeconds) => {
        if (!audioEl) return;

        const absolute = new URL(src, window.location.origin).href;
        if (audioEl.src !== absolute) {
            audioEl.src = src;
        }

        const clamped = Math.max(0, positionSeconds);
        if (!isNaN(audioEl.duration)) {
            audioEl.currentTime = Math.min(clamped, audioEl.duration || clamped);
        } else {
            audioEl.currentTime = clamped;
        }

        audioEl.play().catch(() => { });
    }
};
