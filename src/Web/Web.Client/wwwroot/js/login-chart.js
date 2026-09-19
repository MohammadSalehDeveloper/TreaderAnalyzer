/**
 * Lightweight animated forex candlestick chart for the auth layout.
 * ES module — start/stop from Blazor via IJSRuntime.
 */

let animationId = 0;
let resizeObserver = null;
let canvasEl = null;
let ctx = null;
let candles = [];
let tick = 0;
let running = false;

const PALETTE = {
    bgGrid: 'rgba(158, 179, 194, 0.08)',
    up: '#2ec4b6',
    down: '#f07167',
    wick: 'rgba(232, 238, 243, 0.55)',
    line: 'rgba(46, 196, 182, 0.55)',
    scan: 'rgba(46, 196, 182, 0.12)',
};

function seededRand(seed) {
    let s = seed % 2147483647;
    if (s <= 0) s += 2147483646;
    return () => {
        s = (s * 16807) % 2147483647;
        return (s - 1) / 2147483646;
    };
}

function buildCandles(count) {
    const rand = seededRand(42);
    const list = [];
    let price = 1.085;

    for (let i = 0; i < count; i++) {
        const drift = (rand() - 0.48) * 0.0045;
        const open = price;
        const close = open + drift;
        const high = Math.max(open, close) + rand() * 0.0018;
        const low = Math.min(open, close) - rand() * 0.0018;
        list.push({ open, high, low, close });
        price = close;
    }

    return list;
}

function resize() {
    if (!canvasEl || !ctx) return;
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    const { clientWidth: w, clientHeight: h } = canvasEl;
    canvasEl.width = Math.max(1, Math.floor(w * dpr));
    canvasEl.height = Math.max(1, Math.floor(h * dpr));
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
}

function priceRange(list) {
    let min = Infinity;
    let max = -Infinity;
    for (const c of list) {
        min = Math.min(min, c.low);
        max = Math.max(max, c.high);
    }
    const pad = (max - min) * 0.12 || 0.002;
    return { min: min - pad, max: max + pad };
}

function draw() {
    if (!canvasEl || !ctx || !running) return;

    const w = canvasEl.clientWidth;
    const h = canvasEl.clientHeight;
    ctx.clearRect(0, 0, w, h);

    // Horizontal grid
    ctx.strokeStyle = PALETTE.bgGrid;
    ctx.lineWidth = 1;
    for (let i = 1; i < 6; i++) {
        const y = (h / 6) * i;
        ctx.beginPath();
        ctx.moveTo(0, y);
        ctx.lineTo(w, y);
        ctx.stroke();
    }

    const visible = candles.slice(-48);
    const { min, max } = priceRange(visible);
    const span = max - min || 1;
    const slot = w / (visible.length + 1);
    const bodyW = Math.max(3, slot * 0.55);

    const yOf = (p) => h - ((p - min) / span) * (h * 0.78) - h * 0.11;

    // Soft scan sweep
    const sweepX = ((tick * 1.6) % (w + 120)) - 60;
    const grad = ctx.createLinearGradient(sweepX - 40, 0, sweepX + 40, 0);
    grad.addColorStop(0, 'transparent');
    grad.addColorStop(0.5, PALETTE.scan);
    grad.addColorStop(1, 'transparent');
    ctx.fillStyle = grad;
    ctx.fillRect(sweepX - 40, 0, 80, h);

    // Closing path
    ctx.beginPath();
    visible.forEach((c, i) => {
        const x = slot * (i + 1);
        const y = yOf(c.close);
        if (i === 0) ctx.moveTo(x, y);
        else ctx.lineTo(x, y);
    });
    ctx.strokeStyle = PALETTE.line;
    ctx.lineWidth = 1.5;
    ctx.stroke();

    visible.forEach((c, i) => {
        const x = slot * (i + 1);
        const yOpen = yOf(c.open);
        const yClose = yOf(c.close);
        const yHigh = yOf(c.high);
        const yLow = yOf(c.low);
        const up = c.close >= c.open;
        const color = up ? PALETTE.up : PALETTE.down;

        ctx.strokeStyle = PALETTE.wick;
        ctx.beginPath();
        ctx.moveTo(x, yHigh);
        ctx.lineTo(x, yLow);
        ctx.stroke();

        const top = Math.min(yOpen, yClose);
        const bodyH = Math.max(2, Math.abs(yClose - yOpen));
        ctx.fillStyle = color;
        ctx.fillRect(x - bodyW / 2, top, bodyW, bodyH);
    });

    // Advance last candle slightly for life
    const last = candles[candles.length - 1];
    const wobble = Math.sin(tick / 18) * 0.00035;
    last.close = last.open + (last.close - last.open) * 0.98 + wobble;
    last.high = Math.max(last.high, last.close, last.open);
    last.low = Math.min(last.low, last.close, last.open);

    if (tick % 90 === 0) {
        const prev = candles[candles.length - 1];
        const drift = (Math.sin(tick / 40) + (Math.random() - 0.5)) * 0.0018;
        const open = prev.close;
        const close = open + drift;
        candles.push({
            open,
            close,
            high: Math.max(open, close) + Math.random() * 0.0009,
            low: Math.min(open, close) - Math.random() * 0.0009,
        });
        if (candles.length > 80) candles.shift();
    }

    tick += 1;
    animationId = requestAnimationFrame(draw);
}

export function startLoginChart(canvasId) {
    stopLoginChart();
    canvasEl = document.getElementById(canvasId);
    if (!canvasEl) return;

    ctx = canvasEl.getContext('2d');
    candles = buildCandles(56);
    tick = 0;
    running = true;
    resize();

    resizeObserver = new ResizeObserver(() => resize());
    resizeObserver.observe(canvasEl);
    animationId = requestAnimationFrame(draw);
}

export function stopLoginChart() {
    running = false;
    if (animationId) {
        cancelAnimationFrame(animationId);
        animationId = 0;
    }
    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }
    canvasEl = null;
    ctx = null;
}
