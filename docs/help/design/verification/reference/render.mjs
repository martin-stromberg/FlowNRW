import { spawn } from 'node:child_process';
import { mkdir, writeFile } from 'node:fs/promises';
import { resolve, join } from 'node:path';
import { pathToFileURL } from 'node:url';
const out = resolve('docs/help/design/verification/reference');
await mkdir(out, { recursive: true });
const chrome = spawn('C:/Program Files/Google/Chrome/Application/chrome.exe', ['--headless', '--disable-gpu', '--no-first-run', '--no-default-browser-check', '--remote-debugging-port=0', `--user-data-dir=${join(out, 'cdp-profile')}`, 'about:blank'], { windowsHide: true });
let endpoint;
const ready = new Promise((resolveReady, reject) => {
  chrome.stderr.on('data', b => { const match = b.toString().match(/DevTools listening on (ws:\/\/[^\s]+)/); if (match) { endpoint = match[1]; resolveReady(); } });
  chrome.on('error', reject);
});
await ready;
const socket = new WebSocket(endpoint);
await new Promise(r => socket.addEventListener('open', r, { once: true }));
let serial = 0;
const pending = new Map();
socket.addEventListener('message', e => { const m = JSON.parse(e.data); if (m.id && pending.has(m.id)) { const p = pending.get(m.id); pending.delete(m.id); m.error ? p.reject(m.error) : p.resolve(m.result); } });
const send = (method, params = {}, sessionId) => new Promise((resolveCall, reject) => { const id = ++serial; pending.set(id, { resolve: resolveCall, reject }); socket.send(JSON.stringify({ id, method, params, sessionId })); });
const { targetId } = await send('Target.createTarget', { url: 'about:blank' });
const { sessionId } = await send('Target.attachToTarget', { targetId, flatten: true });
const call = (method, params) => send(method, params, sessionId);
await call('Page.enable');
const report = [];
for (const name of ['abfahrtsmonitor_live', 'verbindungssuche', 'fahrtbegleiter_detail', 'umgebungskarte_stationen']) {
  for (const [width, height] of [[430, 900], [1024, 768]]) {
    await call('Emulation.setDeviceMetricsOverride', { width, height, deviceScaleFactor: 1, mobile: false });
    await call('Page.navigate', { url: pathToFileURL(resolve(`docs/design/reference/${name}/code.html`)).href });
    await new Promise(r => setTimeout(r, 5000));
    await call('Runtime.evaluate', { expression: 'document.fonts.ready', awaitPromise: true });
    const { result } = await call('Runtime.evaluate', { expression: 'JSON.stringify({width:innerWidth,height:innerHeight,dpr:devicePixelRatio,background:getComputedStyle(document.body).backgroundColor,font:getComputedStyle(document.body).fontFamily,fonts:document.fonts.status,images:Array.from(document.images).map(i=>({src:i.src,loaded:i.complete&&i.naturalWidth>0}))})', returnByValue: true });
    const { data } = await call('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
    await writeFile(join(out, `${name}-${width}x${height}.png`), Buffer.from(data, 'base64'));
    report.push({ name, ...JSON.parse(result.value) });
    console.log(name, width, height);
  }
}
await writeFile(join(out, 'render-metadata.json'), JSON.stringify({ date: new Date().toISOString(), browser: await send('Browser.getVersion'), captures: report }, null, 2));
await send('Browser.close');
socket.close();
