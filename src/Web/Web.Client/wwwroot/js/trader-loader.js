/**
 * Forex chart loader — scrolling candles + pattern recognition overlay.
 * Mouse parallax; 10s minimum duration before dismiss.
 */
(function () {
    const STATUS_LINES = [
        'Streaming EUR/USD candles…',
        'Plotting support & resistance…',
        'Detecting ascending triangle…',
        'Marking double-top structure…',
        'Confirming trendline breakout…',
    ];

    const PATTERNS = [
        { name: 'ASC. TRIANGLE', at: 0.18 },
        { name: 'SUPPORT / RESISTANCE', at: 0.38 },
        { name: 'DOUBLE TOP', at: 0.58 },
        { name: 'TREND CHANNEL', at: 0.78 },
    ];

    const loaderEl = document.getElementById('forensics-loader');
    const canvas = document.getElementById('forensics-canvas');
    const statusEl = document.getElementById('forensics-status');
    const meterEl = document.getElementById('forensics-meter-fill');
    const percentEl = document.getElementById('forensics-percent');

    if (!loaderEl || !canvas) return;

    let THREE;
    let renderer;
    let scene;
    let camera;
    let animationId = 0;
    let disposed = false;
    let progress = 0;
    let statusIndex = 0;
    let statusTimer = 0;
    let bootStartedAt = performance.now();
    let blazorReady = false;
    let finishScheduled = false;

    const mouse = { x: 0, y: 0, tx: 0, ty: 0 };
    const ANIMATION_DURATION_MS = 10000;
    const CANDLE_COUNT = 48;
    const CANDLE_GAP = 0.22;

    let chartRoot;
    let candleMeshes = [];
    let priceHistory = [];
    let scrollOffset = 0;
    let patternGroup;
    let patternLayers = [];
    let gridLines;
    let scanLine;
    let pairLabels = [];
    let rngState = 42;

    function rand() {
        rngState = (rngState * 16807) % 2147483647;
        return (rngState - 1) / 2147483646;
    }

    function onPointerMove(e) {
        const point = e.touches ? e.touches[0] : e;
        mouse.tx = (point.clientX / window.innerWidth) * 2 - 1;
        mouse.ty = -(point.clientY / window.innerHeight) * 2 + 1;
    }

    function onResize() {
        if (!renderer || !camera) return;
        const w = window.innerWidth;
        const h = window.innerHeight;
        camera.aspect = w / h;
        camera.updateProjectionMatrix();
        renderer.setSize(w, h, false);
    }

    function readBlazorProgress() {
        const root = document.getElementById('app') || document.documentElement;
        const styles = getComputedStyle(root);
        const raw = styles.getPropertyValue('--blazor-load-percentage').trim();
        if (!raw) return null;
        const n = parseFloat(raw);
        return Number.isFinite(n) ? Math.min(100, Math.max(0, n)) : null;
    }

    function setUiProgress(value) {
        progress = Math.max(progress, Math.min(100, value));
        if (meterEl) meterEl.style.width = `${progress}%`;
        if (percentEl) percentEl.textContent = `${Math.round(progress)}%`;
    }

    function cycleStatus(dt) {
        statusTimer += dt;
        if (statusTimer < 2.0) return;
        statusTimer = 0;
        statusIndex = (statusIndex + 1) % STATUS_LINES.length;
        if (statusEl) statusEl.textContent = STATUS_LINES[statusIndex];
    }

    function makeCandle(open, close, high, low) {
        const bull = close >= open;
        const color = bull ? 0x26a69a : 0xef5350;
        const bodyH = Math.max(0.04, Math.abs(close - open));
        const mid = (open + close) / 2;

        const group = new THREE.Group();

        const body = new THREE.Mesh(
            new THREE.BoxGeometry(0.14, bodyH, 0.08),
            new THREE.MeshBasicMaterial({ color, transparent: true, opacity: 0.92 })
        );
        body.position.y = mid;
        group.add(body);

        const wickH = Math.max(0.02, high - low);
        const wick = new THREE.Mesh(
            new THREE.BoxGeometry(0.025, wickH, 0.025),
            new THREE.MeshBasicMaterial({ color, transparent: true, opacity: 0.75 })
        );
        wick.position.y = (high + low) / 2;
        group.add(wick);

        group.userData = { body, wick, open, close, high, low, bull };
        return group;
    }

    function generateSeries(count, seedPrice) {
        const series = [];
        let price = seedPrice;
        for (let i = 0; i < count; i++) {
            const drift = Math.sin(i * 0.18) * 0.08 + (rand() - 0.48) * 0.22;
            const open = price;
            const close = open + drift;
            const high = Math.max(open, close) + rand() * 0.12;
            const low = Math.min(open, close) - rand() * 0.12;
            series.push({ open, close, high, low });
            price = close;
        }
        return series;
    }

    let candleDirty = true;

    function rebuildCandleVisual(mesh, ohlc) {
        const { open, close, high, low } = ohlc;
        const bull = close >= open;
        const color = bull ? 0x26a69a : 0xef5350;
        const bodyH = Math.max(0.04, Math.abs(close - open));
        const mid = (open + close) / 2;

        mesh.userData.body.geometry.dispose();
        mesh.userData.body.geometry = new THREE.BoxGeometry(0.14, bodyH, 0.08);
        mesh.userData.body.position.y = mid;
        mesh.userData.body.material.color.setHex(color);

        const wickH = Math.max(0.02, high - low);
        mesh.userData.wick.geometry.dispose();
        mesh.userData.wick.geometry = new THREE.BoxGeometry(0.025, wickH, 0.025);
        mesh.userData.wick.position.y = (high + low) / 2;
        mesh.userData.wick.material.color.setHex(color);
    }

    function syncCandleMeshes() {
        const startIdx = Math.max(0, priceHistory.length - CANDLE_COUNT);
        for (let i = 0; i < CANDLE_COUNT; i++) {
            const ohlc = priceHistory[startIdx + i];
            if (!ohlc) continue;
            rebuildCandleVisual(candleMeshes[i], ohlc);
        }
        candleDirty = false;
    }

    function createGrid(width, height) {
        const group = new THREE.Group();
        const mat = new THREE.LineBasicMaterial({
            color: 0x1e3a4c,
            transparent: true,
            opacity: 0.55,
        });

        for (let i = 0; i <= 8; i++) {
            const y = -height / 2 + (i / 8) * height;
            const geo = new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(-width / 2, y, -0.02),
                new THREE.Vector3(width / 2, y, -0.02),
            ]);
            group.add(new THREE.Line(geo, mat));
        }
        for (let i = 0; i <= 12; i++) {
            const x = -width / 2 + (i / 12) * width;
            const geo = new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(x, -height / 2, -0.02),
                new THREE.Vector3(x, height / 2, -0.02),
            ]);
            group.add(new THREE.Line(geo, mat));
        }
        return group;
    }

    function createLabelTexture(text, color) {
        const c = document.createElement('canvas');
        c.width = 512;
        c.height = 128;
        const ctx = c.getContext('2d');
        ctx.clearRect(0, 0, c.width, c.height);
        ctx.font = '600 42px "IBM Plex Mono", monospace';
        ctx.fillStyle = color;
        ctx.textAlign = 'left';
        ctx.textBaseline = 'middle';
        ctx.fillText(text, 16, 64);
        const tex = new THREE.CanvasTexture(c);
        tex.needsUpdate = true;
        return tex;
    }

    function createSpriteLabel(text, color, position) {
        const mat = new THREE.SpriteMaterial({
            map: createLabelTexture(text, color),
            transparent: true,
            opacity: 0,
            depthWrite: false,
        });
        const sprite = new THREE.Sprite(mat);
        sprite.scale.set(2.4, 0.6, 1);
        sprite.position.copy(position);
        sprite.userData.targetOpacity = 0.9;
        return sprite;
    }

    function lineFromPoints(points, color, opacity) {
        const geo = new THREE.BufferGeometry().setFromPoints(points);
        const mat = new THREE.LineBasicMaterial({
            color,
            transparent: true,
            opacity: opacity ?? 0,
            linewidth: 1,
        });
        const line = new THREE.Line(geo, mat);
        line.userData.targetOpacity = opacity ?? 0.85;
        return line;
    }

    function createPatternOverlays(chartWidth) {
        patternGroup = new THREE.Group();
        patternLayers = [];

        // Ascending triangle
        const triangle = new THREE.Group();
        triangle.add(lineFromPoints([
            new THREE.Vector3(-3.2, -0.6, 0.05),
            new THREE.Vector3(0.4, 0.9, 0.05),
            new THREE.Vector3(0.4, 1.15, 0.05),
            new THREE.Vector3(-3.2, 1.15, 0.05),
            new THREE.Vector3(-3.2, -0.6, 0.05),
        ], 0xf4a261, 0));
        const triLabel = createSpriteLabel('ASC. TRIANGLE', '#f4a261', new THREE.Vector3(-1.4, 1.55, 0.1));
        triangle.add(triLabel);
        triangle.userData.revealAt = PATTERNS[0].at;
        patternGroup.add(triangle);
        patternLayers.push(triangle);

        // Support / Resistance
        const sr = new THREE.Group();
        sr.add(lineFromPoints([
            new THREE.Vector3(-chartWidth / 2 + 0.3, 1.15, 0.06),
            new THREE.Vector3(chartWidth / 2 - 0.3, 1.15, 0.06),
        ], 0x2ec4b6, 0));
        sr.add(lineFromPoints([
            new THREE.Vector3(-chartWidth / 2 + 0.3, -0.85, 0.06),
            new THREE.Vector3(chartWidth / 2 - 0.3, -0.85, 0.06),
        ], 0xef5350, 0));
        sr.add(createSpriteLabel('RESISTANCE', '#2ec4b6', new THREE.Vector3(3.4, 1.35, 0.1)));
        sr.add(createSpriteLabel('SUPPORT', '#ef5350', new THREE.Vector3(3.4, -1.05, 0.1)));
        sr.userData.revealAt = PATTERNS[1].at;
        patternGroup.add(sr);
        patternLayers.push(sr);

        // Double top
        const doubleTop = new THREE.Group();
        doubleTop.add(lineFromPoints([
            new THREE.Vector3(0.8, 0.2, 0.07),
            new THREE.Vector3(1.5, 1.35, 0.07),
            new THREE.Vector3(2.2, 0.35, 0.07),
            new THREE.Vector3(2.9, 1.32, 0.07),
            new THREE.Vector3(3.6, 0.15, 0.07),
        ], 0xe9c46a, 0));
        doubleTop.add(createSpriteLabel('DOUBLE TOP', '#e9c46a', new THREE.Vector3(2.2, 1.7, 0.12)));
        doubleTop.userData.revealAt = PATTERNS[2].at;
        patternGroup.add(doubleTop);
        patternLayers.push(doubleTop);

        // Trend channel
        const channel = new THREE.Group();
        channel.add(lineFromPoints([
            new THREE.Vector3(-4.2, -1.3, 0.08),
            new THREE.Vector3(4.2, 0.55, 0.08),
        ], 0x9eb3c2, 0));
        channel.add(lineFromPoints([
            new THREE.Vector3(-4.2, -0.55, 0.08),
            new THREE.Vector3(4.2, 1.3, 0.08),
        ], 0x9eb3c2, 0));
        channel.add(createSpriteLabel('TREND CHANNEL', '#9eb3c2', new THREE.Vector3(-2.6, -1.65, 0.12)));
        channel.userData.revealAt = PATTERNS[3].at;
        patternGroup.add(channel);
        patternLayers.push(channel);

        chartRoot.add(patternGroup);
    }

    function setLayerOpacity(layer, opacity) {
        layer.traverse((obj) => {
            if (obj.material && obj.material.opacity !== undefined) {
                const target = obj.userData.targetOpacity ?? 0.85;
                obj.material.opacity = opacity * target;
                obj.material.transparent = true;
            }
        });
    }

    function createMiniChart(z, y, scale, seed) {
        const group = new THREE.Group();
        group.position.set(0, y, z);
        group.scale.setScalar(scale);

        const series = generateSeries(28, seed);
        const startX = -28 * CANDLE_GAP * 0.5;
        series.forEach((c, i) => {
            const mesh = makeCandle(c.open, c.close, c.high, c.low);
            mesh.position.x = startX + i * CANDLE_GAP;
            group.add(mesh);
        });

        const frame = createGrid(7, 3.2);
        frame.scale.set(1, 1, 1);
        group.add(frame);
        return group;
    }

    function createMainChart() {
        chartRoot = new THREE.Group();
        const chartWidth = CANDLE_COUNT * CANDLE_GAP;

        gridLines = createGrid(chartWidth + 0.6, 4.2);
        chartRoot.add(gridLines);

        priceHistory = generateSeries(CANDLE_COUNT + 20, 0);
        const startX = -chartWidth / 2;

        for (let i = 0; i < CANDLE_COUNT; i++) {
            const c = priceHistory[i];
            const mesh = makeCandle(c.open, c.close, c.high, c.low);
            mesh.position.x = startX + i * CANDLE_GAP;
            chartRoot.add(mesh);
            candleMeshes.push(mesh);
        }

        // Horizontal scan cursor
        scanLine = new THREE.Mesh(
            new THREE.PlaneGeometry(0.03, 4.2),
            new THREE.MeshBasicMaterial({
                color: 0xf4a261,
                transparent: true,
                opacity: 0.55,
                depthWrite: false,
            })
        );
        scanLine.position.z = 0.15;
        chartRoot.add(scanLine);

        createPatternOverlays(chartWidth);

        const pair = createSpriteLabel('EUR / USD  ·  M15', '#e8eef3', new THREE.Vector3(-3.8, 2.35, 0.2));
        pair.userData.targetOpacity = 0.95;
        setLayerOpacity(pair, 1);
        pair.material.opacity = 0.95;
        chartRoot.add(pair);
        pairLabels.push(pair);

        scene.add(chartRoot);

        // Depth layer charts
        scene.add(createMiniChart(-3.2, 1.4, 0.45, 0.4));
        scene.add(createMiniChart(-2.4, -1.8, 0.38, -0.2));
    }

    function pushNewCandle() {
        const last = priceHistory[priceHistory.length - 1];
        const drift = Math.sin(priceHistory.length * 0.21) * 0.07 + (rand() - 0.48) * 0.2;
        const open = last.close;
        const close = open + drift;
        const high = Math.max(open, close) + rand() * 0.1;
        const low = Math.min(open, close) - rand() * 0.1;
        priceHistory.push({ open, close, high, low });
        if (priceHistory.length > 200) priceHistory.shift();
        candleDirty = true;
    }

    function updateCandles(time) {
        scrollOffset += 0.009;
        if (scrollOffset >= CANDLE_GAP) {
            scrollOffset -= CANDLE_GAP;
            pushNewCandle();
        }

        if (candleDirty) syncCandleMeshes();

        const chartWidth = CANDLE_COUNT * CANDLE_GAP;
        const startX = -chartWidth / 2;

        for (let i = 0; i < CANDLE_COUNT; i++) {
            const mesh = candleMeshes[i];
            mesh.position.x = startX + i * CANDLE_GAP - scrollOffset;
            const influence = 1 - Math.min(1, Math.abs(mesh.position.x - mouse.x * 4) / 4);
            mesh.position.z = influence * mouse.y * 0.25;
            mesh.scale.y = 1 + influence * 0.08;
        }

        if (scanLine) {
            const span = chartWidth * 0.9;
            scanLine.position.x = -span / 2 + ((time * 0.35) % 1) * span;
            scanLine.material.opacity = 0.35 + (progress / 100) * 0.35;
        }
    }

    function updatePatterns() {
        const t = progress / 100;
        patternLayers.forEach((layer) => {
            const reveal = layer.userData.revealAt;
            const local = Math.max(0, Math.min(1, (t - reveal) / 0.12));
            setLayerOpacity(layer, local);
            layer.visible = local > 0.01;
        });
    }

    function animate(now) {
        if (disposed) return;
        animationId = requestAnimationFrame(animate);
        const time = now * 0.001;

        mouse.x += (mouse.tx - mouse.x) * 0.08;
        mouse.y += (mouse.ty - mouse.y) * 0.08;

        const elapsedMs = performance.now() - bootStartedAt;
        const timedPct = Math.min(99, (elapsedMs / ANIMATION_DURATION_MS) * 100);
        const blazorPct = readBlazorProgress();
        setUiProgress(blazorPct != null ? Math.max(timedPct, Math.min(99, blazorPct * 0.85)) : timedPct);

        if (elapsedMs >= ANIMATION_DURATION_MS && blazorReady) {
            finish();
        }

        cycleStatus(0.016);
        updateCandles(time);
        updatePatterns();

        if (chartRoot) {
            chartRoot.rotation.y = mouse.x * 0.18;
            chartRoot.rotation.x = -0.08 + mouse.y * 0.1;
            chartRoot.position.y = mouse.y * 0.15;
        }

        camera.position.x = mouse.x * 0.45;
        camera.position.y = 0.35 + mouse.y * 0.3;
        camera.position.z = 8.2;
        camera.lookAt(0, 0.1, 0);

        renderer.render(scene, camera);
    }

    function dispose() {
        if (disposed) return;
        disposed = true;
        cancelAnimationFrame(animationId);
        window.removeEventListener('pointermove', onPointerMove);
        window.removeEventListener('touchmove', onPointerMove);
        window.removeEventListener('resize', onResize);

        if (renderer) {
            renderer.dispose();
            if (typeof renderer.forceContextLoss === 'function') {
                renderer.forceContextLoss();
            }
        }
    }

    function finish() {
        if (loaderEl.classList.contains('is-done') || finishScheduled) return;

        const elapsed = performance.now() - bootStartedAt;
        const remaining = Math.max(0, ANIMATION_DURATION_MS - elapsed);

        if (remaining > 0) {
            blazorReady = true;
            finishScheduled = true;
            setTimeout(() => {
                finishScheduled = false;
                finish();
            }, remaining);
            return;
        }

        setUiProgress(100);
        if (statusEl) statusEl.textContent = 'Patterns locked. Entering desk…';
        loaderEl.setAttribute('aria-busy', 'false');
        loaderEl.classList.add('is-done');
        setTimeout(dispose, 800);
    }

    function markBlazorReady() {
        blazorReady = true;
        finish();
    }

    async function boot() {
        const mod = await import('https://unpkg.com/three@0.170.0/build/three.module.js');
        THREE = mod;

        scene = new THREE.Scene();
        camera = new THREE.PerspectiveCamera(50, window.innerWidth / window.innerHeight, 0.1, 100);
        camera.position.set(0, 0.4, 8.2);

        renderer = new THREE.WebGLRenderer({
            canvas,
            antialias: true,
            alpha: true,
            powerPreference: 'high-performance',
        });
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        renderer.setSize(window.innerWidth, window.innerHeight, false);
        renderer.setClearColor(0x000000, 0);

        createMainChart();

        window.addEventListener('pointermove', onPointerMove, { passive: true });
        window.addEventListener('touchmove', onPointerMove, { passive: true });
        window.addEventListener('resize', onResize);

        if (statusEl) statusEl.textContent = STATUS_LINES[0];
        animate(0);
    }

    window.__forensicsLoaderComplete = markBlazorReady;

    boot().catch((err) => {
        console.error('Forex chart loader failed', err);
        finish();
    });
})();
