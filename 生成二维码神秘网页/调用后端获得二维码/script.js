const SECRET = '^mFIT!xzJ@j55QN%R^4yZ0vx';
const APPID = 'wx986e22f241e13ba2';
const PATH = 'subpkg/order/order';
const TABLE_IDS = [1, 2, 3, 4, 5, 6, 7, 8];

let toastTimer = null;

function $(id) { return document.getElementById(id); }

function showToast(msg) {
  const t = $('toast');
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => t.classList.remove('show'), 2500);
}

function setStatus(online, text) {
  $('statusDot').className = 'status-dot ' + (online ? 'online' : 'offline');
  $('statusText').textContent = text;
}

function buildTableUrl(tableId) {
  const query = 'tableId=' + tableId + '&secret=' + encodeURIComponent(SECRET);
  return 'weixin://dl/business/?appid=' + APPID + '&path=' + PATH + '&query=' + query;
}

function getApiBase() {
  var v = $('apiBase').value.replace(/\/+$/, '');
  return v.replace(/\/table-qrcodes(\/.*)?$/, '');
}

// ---- Fallback QR via external API (when local QR lib fails) ----
function drawFallbackQr(container, text, size, tableId) {
  container.innerHTML = '';
  var img = document.createElement('img');
  img.src = 'https://api.qrserver.com/v1/create-qr-code/?size=' + size + 'x' + size + '&margin=12&data=' + encodeURIComponent(text);
  img.style.width = size + 'px';
  img.style.height = size + 'px';
  img.style.objectFit = 'contain';
  img.alt = tableId + '号桌二维码';
  img.title = tableId + '号桌二维码';
  img.onerror = function() {
    container.innerHTML = '<div class="loading-placeholder" style="font-size:10px;word-break:break-all;padding:8px;">QR 服务不可用，请复制下方链接使用</div>';
  };
  container.appendChild(img);
}

// ---- QR Code Generation ----
function generateQr(containerId, text, tableId) {
  const container = $(containerId);
  if (!container) return;

  container.innerHTML = '';
  const size = Math.min(container.offsetWidth || 180, 180);

  if (typeof QRCode === 'undefined' || window._qrLibFailed) {
    if (window._qrLibFailed) {
      drawFallbackQr(container, text, size, tableId);
      return;
    }
    container.innerHTML = '<div class="loading-placeholder">二维码加载中...</div>';
    var s = document.createElement('script');
    s.src = 'https://cdn.jsdelivr.net/npm/qrcode@1.5.3/build/qrcode.min.js';
    s.onload = function() { window._qrLibFailed = false; generateQr(containerId, text, tableId); };
    s.onerror = function() { window._qrLibFailed = true; drawFallbackQr(container, text, size, tableId); };
    document.head.appendChild(s);
    return;
  }

  try {
    QRCode.toDataURL(text, {
      width: size,
      margin: 2,
      color: { dark: '#3d2e1a', light: '#ffffff' }
    }).then(function(url) {
      var img = document.createElement('img');
      img.src = url;
      img.style.width = size + 'px';
      img.style.height = size + 'px';
      img.style.objectFit = 'contain';
      img.alt = tableId + '号桌二维码';
      container.appendChild(img);
    }).catch(function() {
      drawFallbackQr(container, text, size, tableId);
    });
  } catch(e) {
    drawFallbackQr(container, text, size, tableId);
  }
}

// ---- Render Cards ----
function renderCards(data) {
  const grid = $('grid');
  grid.innerHTML = '';

  if (!data || data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><span class="icon">&#128247;</span><p>暂无桌台数据</p></div>';
    $('tableCount').textContent = '共 0 张桌台';
    return;
  }

  $('tableCount').textContent = '共 ' + data.length + ' 张桌台';

  data.forEach((item, idx) => {
    const tableId = item.tableId || item.id || (idx + 1);
    const tableName = item.tableName || item.tableNo || tableId + '号桌';
    // 始终使用本地生成的 URL，确保二维码编码正确的微信跳转链接
    const url = buildTableUrl(tableId);

    const card = document.createElement('div');
    card.className = 'card';
    card.id = 'card-' + tableId;

    card.innerHTML = `
      <div class="card-badge">${tableName}</div>
      <div class="card-title" style="margin-top:8px;">${tableName}</div>
      <div class="card-subtitle">桌台 ID: ${tableId}</div>
      <div class="qr-wrapper" id="qr-${tableId}">
        <div class="loading-placeholder">生成中...</div>
      </div>
      <div class="card-url" onclick="copyUrl(this, '${tableId}')" title="点击复制链接">
        ${url}
        <span class="copy-hint">复制</span>
      </div>
      <div class="card-actions">
        <button class="btn btn-outline btn-sm" onclick="regenerateCard(${tableId})">重新生成</button>
        <button class="btn btn-outline btn-sm" onclick="downloadCard(${tableId})">下载 PNG</button>
      </div>
    `;

    grid.appendChild(card);

    // 无论 API 是否返回二维码图片，都基于本地 URL 重新生成
    setTimeout(() => {
      generateQr('qr-' + tableId, url, tableId);
    }, 100 * idx);
  });
}

// ---- Load from API ----
async function loadFromApi() {
  const base = getApiBase();
  const url = base + '/table-qrcodes';

  setStatus(false, '正在从 API 加载...');
  $('modeText').textContent = 'API 模式';

  try {
    const resp = await fetch(url, { method: 'GET', signal: AbortSignal.timeout(8000) });
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const result = await resp.json();
    if (result.code !== 0) throw new Error(result.message || '接口返回异常');

    const data = result.data || [];
    if (data.length === 0) throw new Error('暂无桌台数据');

    renderCards(data);
    setStatus(true, 'API 已连接，数据加载成功');
    showToast('已加载 ' + data.length + ' 张桌台二维码');
  } catch (e) {
    setStatus(false, 'API 请求失败: ' + e.message);
    showToast('API 请求失败，当前仍为离线模式');
  }
}

// ---- Local Generation ----
function generateAllLocal() {
  setStatus(false, '正在本地生成二维码...');
  $('modeText').textContent = '离线模式';

  const data = TABLE_IDS.map(id => ({
    tableId: id,
    tableName: id + '号桌',
    tableNo: id + '号桌',
    url: buildTableUrl(id),
    qrCode: ''
  }));

  renderCards(data);
  setStatus(true, '离线模式 — 本地生成二维码（共 ' + data.length + ' 张）');
  showToast('已本地生成 ' + data.length + ' 张桌台二维码');
}

function regenerateCard(tableId) {
  const wrap = document.getElementById('qr-' + tableId);
  if (wrap) {
    wrap.innerHTML = '';
    generateQr('qr-' + tableId, buildTableUrl(tableId), tableId);
    showToast('已重新生成 ' + tableId + '号桌二维码');
  }
}

// ---- Connect to API (test) ----
async function connectApi() {
  const base = getApiBase();
  setStatus(false, '正在连接...');
  try {
    const resp = await fetch(base + '/table-qrcodes', { method: 'GET', signal: AbortSignal.timeout(5000) });
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const result = await resp.json();
    if (result.code === 0) {
      setStatus(true, 'API 已连接');
      showToast('API 连接成功');
    } else {
      throw new Error(result.message);
    }
  } catch (e) {
    setStatus(false, '连接失败: ' + e.message);
    showToast('API 连接失败');
  }
}

function useLocalMode() {
  generateAllLocal();
  showToast('已切换为离线生成模式');
}

// ---- Utilities ----
function copyUrl(el, tableId) {
  const text = el.textContent.replace('复制', '').trim();
  navigator.clipboard.writeText(text).then(() => {
    showToast('已复制 ' + tableId + '号桌二维码链接');
  }).catch(() => {
    const ta = document.createElement('textarea');
    ta.value = text;
    document.body.appendChild(ta);
    ta.select();
    document.execCommand('copy');
    document.body.removeChild(ta);
    showToast('已复制 ' + tableId + '号桌二维码链接');
  });
}

function downloadCard(tableId) {
  const wrap = document.getElementById('qr-' + tableId);
  if (!wrap) {
    showToast('请先生成二维码');
    return;
  }
  const img = wrap.querySelector('img');
  if (!img || !img.src) { showToast('二维码尚未生成'); return; }
  const src = img.src;
  if (src.startsWith('data:')) {
    const a = document.createElement('a');
    a.href = src;
    a.download = tableId + '号桌二维码.png';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    showToast('已下载 ' + tableId + '号桌二维码');
  } else {
    window.open(src, '_blank');
  }
}

function printPage() {
  window.print();
}

// ---- Event Binding ----
$('btnConnect').addEventListener('click', connectApi);
$('btnLocalMode').addEventListener('click', useLocalMode);
$('btnLoadApi').addEventListener('click', loadFromApi);
$('btnGenerateAll').addEventListener('click', generateAllLocal);
$('btnPrint').addEventListener('click', printPage);

// ---- Init ----
document.addEventListener('DOMContentLoaded', generateAllLocal);
