(function (window) {
	'use strict';

	var STORAGE_KEY = 'subscriptions';

	function safeParse(value) {
		if (!value) {
			return null;
		}

		try {
			return JSON.parse(value);
		} catch (error) {
			console.error('认购套餐数据解析失败:', error);
			return null;
		}
	}

	function pad(value) {
		return String(value).padStart(2, '0');
	}

	function formatDateTime(date) {
		return [
			date.getFullYear(),
			pad(date.getMonth() + 1),
			pad(date.getDate())
		].join('-') + ' ' + [
			pad(date.getHours()),
			pad(date.getMinutes()),
			pad(date.getSeconds())
		].join(':');
	}

	function createPlaceholderImage(title, subtitle, colorStart, colorEnd) {
		var svg = [
			'<svg xmlns="http://www.w3.org/2000/svg" width="720" height="480" viewBox="0 0 720 480">',
			'<defs>',
			'<linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%">',
			'<stop offset="0%" stop-color="' + colorStart + '"/>',
			'<stop offset="100%" stop-color="' + colorEnd + '"/>',
			'</linearGradient>',
			'</defs>',
			'<rect width="720" height="480" rx="32" fill="url(#bg)"/>',
			'<circle cx="612" cy="108" r="68" fill="rgba(255,255,255,0.14)"/>',
			'<circle cx="126" cy="390" r="82" fill="rgba(255,255,255,0.1)"/>',
			'<path d="M0 360 C120 290 180 420 300 360 C420 300 520 420 720 310 L720 480 L0 480 Z" fill="rgba(255,255,255,0.18)"/>',
			'<text x="64" y="190" fill="#ffffff" font-size="42" font-family="Microsoft YaHei, sans-serif" font-weight="700">' + title + '</text>',
			'<text x="64" y="246" fill="rgba(255,255,255,0.92)" font-size="24" font-family="Microsoft YaHei, sans-serif">' + subtitle + '</text>',
			'<text x="64" y="402" fill="rgba(255,255,255,0.88)" font-size="20" font-family="Microsoft YaHei, sans-serif">农场认购套餐展示图</text>',
			'</svg>'
		].join('');

		return 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(svg);
	}

	function ensureArray(value) {
		if (Array.isArray(value)) {
			return value.filter(Boolean);
		}

		if (typeof value === 'string' && value.trim()) {
			return [value.trim()];
		}

		return [];
	}

	function normalizeText(value, fallback) {
		var text = value === null || value === undefined ? '' : String(value).trim();
		return text || (fallback || '');
	}

	function normalizeNumberString(value, fallback) {
		var text = normalizeText(value, '');

		if (!text) {
			return fallback || '';
		}

		var numeric = Number(text);

		if (!Number.isFinite(numeric)) {
			return text;
		}

		if (Number.isInteger(numeric)) {
			return String(numeric);
		}

		return String(Number(numeric.toFixed(2)));
	}

	function getNumericValue(value) {
		if (value === null || value === undefined) {
			return 0;
		}

		var text = String(value).replace(/[^\d.]/g, '');
		var numeric = Number(text);
		return Number.isFinite(numeric) ? numeric : 0;
	}

	function buildHarvestTemplate(name, cropSummary) {
		return [
			name + ' 在一个种植周期内可采收以下农产品：',
			cropSummary.split('/').map(function (item) {
				return item.trim();
			}).filter(Boolean).map(function (item, index) {
				return (index + 1) + '. ' + item + '，预计产量约 120-220 斤';
			}).join('\n'),
			'实际产出将根据天气、种植情况和采收安排动态调整。'
		].filter(Boolean).join('\n');
	}

	function normalizeStatus(value) {
		return normalizeText(value, '已上架') === '已下架' ? '已下架' : '已上架';
	}

	function normalizeSubscription(subscription, index) {
		var raw = subscription || {};
		var name = normalizeText(raw.name || raw.packageName || raw.title, '认购套餐' + (index + 1));
		var carouselImages = ensureArray(raw.carouselImages || raw.banners || raw.carousel || raw.slides);
		var detailImages = ensureArray(raw.detailImages || raw.specImages || raw.detailPics || raw.images);
		var cover = normalizeText(
			raw.cover || raw.coverImage || carouselImages[0] || detailImages[0],
			createPlaceholderImage(name, '认购套餐封面图', '#16a34a', '#4ade80')
		);
		var cropSummary = normalizeText(
			raw.cropSummary || raw.products || raw.harvestList || raw.minYield,
			'番茄 / 黄瓜 / 玉米 / 青菜'
		);
		var expectedYield = normalizeText(
			raw.expectedYield || raw.output || raw.minYield,
			'约 800 斤'
		);
		var square = normalizeNumberString(
			raw.square || raw.areaSize || raw.squareMeters || raw.squareMeter || raw.area,
			'666.7'
		);
		var cycle = normalizeText(
			raw.cycle || raw.plantingCycle || raw.growingCycle,
			'2026-04 至 2026-09'
		);
		var intro = normalizeText(
			raw.intro || raw.description || raw.farmIntro,
			name + ' 面向家庭与企业开放认购，种植周期内可按排期收获当季农产品，并支持订单中心查看认购详情。'
		);
		var harvestDetail = normalizeText(
			raw.harvestDetail || raw.specInfo || raw.harvestNotes || raw.detail,
			buildHarvestTemplate(name, cropSummary)
		);
		var notes = normalizeText(
			raw.notes || raw.notice || raw.bookingNotes,
			'支付成功后生成认购订单，订单中心可查看认购详情、联系人信息、联系方式以及当季产出安排。'
		);
		var normalizedCarouselImages = (carouselImages.length ? carouselImages : [
			cover,
			createPlaceholderImage(name, '种植周期展示图', '#0ea5e9', '#2563eb')
		]).slice(0, 5);
		var normalizedDetailImages = (detailImages.length ? detailImages : [
			createPlaceholderImage(name, '收获明细示意图', '#f59e0b', '#f97316'),
			createPlaceholderImage(name, '搭配图片', '#14b8a6', '#22c55e')
		]).slice(0, 5);

		return {
			id: normalizeText(raw.id, 'SUB' + Date.now() + String(index || 0).padStart(3, '0')),
			cover: cover,
			carouselImages: normalizedCarouselImages,
			detailImages: normalizedDetailImages,
			name: name,
			createTime: normalizeText(raw.createTime || raw.createdAt, formatDateTime(new Date())),
			price: normalizeNumberString(raw.price || raw.unitPrice || raw.amount, '2999'),
			stock: normalizeNumberString(raw.stock || raw.inventory || raw.stockCount, '20'),
			square: square,
			cycle: cycle,
			cropSummary: cropSummary,
			expectedYield: expectedYield,
			status: normalizeStatus(raw.status),
			intro: intro,
			harvestDetail: harvestDetail,
			notes: notes,
			selected: Boolean(raw.selected)
		};
	}

	function withUnselected(subscription) {
		return Object.assign({}, subscription, {
			selected: false
		});
	}

	function getDefaultSubscriptions() {
		return [
			normalizeSubscription({
				id: 'SUB20260401001',
				name: '春季时令一亩田认购',
				createTime: '2026-03-20 09:30:00',
				price: '2999',
				stock: '18',
				square: '666.7',
				cycle: '2026-04 至 2026-09',
				cropSummary: '番茄 / 黄瓜 / 玉米 / 青菜',
				expectedYield: '约 800 斤',
				status: '已上架',
				intro: '适合家庭长期认购，覆盖春夏一整个种植周期，搭配田间巡园和阶段采收提醒。',
				harvestDetail: '1. 番茄预计收获约 220 斤\n2. 黄瓜预计收获约 180 斤\n3. 玉米预计收获约 200 斤\n4. 青菜预计收获约 200 斤',
				notes: '在线认购需填写联系人信息和联系方式；支付成功后自动生成认购订单，可在订单中心查看认购详情。',
				cover: createPlaceholderImage('春季时令一亩田认购', '番茄 黄瓜 玉米 青菜', '#16a34a', '#4ade80'),
				carouselImages: [
					createPlaceholderImage('春季时令一亩田认购', '地块航拍图', '#16a34a', '#4ade80'),
					createPlaceholderImage('春季时令一亩田认购', '田间实拍图', '#0ea5e9', '#2563eb')
				],
				detailImages: [
					createPlaceholderImage('春季时令一亩田认购', '套餐明细图', '#f59e0b', '#f97316'),
					createPlaceholderImage('春季时令一亩田认购', '收获示意图', '#14b8a6', '#22c55e')
				]
			}, 0),
			normalizeSubscription({
				id: 'SUB20260401002',
				name: '亲子体验一亩田认购',
				createTime: '2026-03-22 14:15:00',
				price: '3680',
				stock: '12',
				square: '720',
				cycle: '2026-05 至 2026-10',
				cropSummary: '草莓 / 生菜 / 胡萝卜 / 南瓜',
				expectedYield: '约 680 斤',
				status: '已上架',
				intro: '适合亲子家庭参与认购，包含亲子采摘体验、种植科普讲解和阶段性收获安排。',
				harvestDetail: '1. 草莓预计收获约 120 斤\n2. 生菜预计收获约 140 斤\n3. 胡萝卜预计收获约 180 斤\n4. 南瓜预计收获约 240 斤',
				notes: '支持线上认购与订单查询；如需到场体验，系统可在订单中心查看预约提醒。',
				cover: createPlaceholderImage('亲子体验一亩田认购', '草莓 生菜 胡萝卜 南瓜', '#ec4899', '#f97316'),
				carouselImages: [
					createPlaceholderImage('亲子体验一亩田认购', '亲子巡田图', '#ec4899', '#f97316'),
					createPlaceholderImage('亲子体验一亩田认购', '采摘体验图', '#f59e0b', '#facc15')
				],
				detailImages: [
					createPlaceholderImage('亲子体验一亩田认购', '套餐明细图', '#8b5cf6', '#6366f1'),
					createPlaceholderImage('亲子体验一亩田认购', '作物产出图', '#06b6d4', '#0ea5e9')
				]
			}, 1),
			normalizeSubscription({
				id: 'SUB20260401003',
				name: '企业团建共享田认购',
				createTime: '2026-03-25 11:40:00',
				price: '4999',
				stock: '8',
				square: '860',
				cycle: '2026-04 至 2026-11',
				cropSummary: '西红柿 / 茄子 / 玉米 / 秋葵 / 豆角',
				expectedYield: '约 1200 斤',
				status: '已下架',
				intro: '适合企业团建和员工福利场景，套餐支持统一认购、采收分配和地块展示。',
				harvestDetail: '1. 西红柿预计收获约 260 斤\n2. 茄子预计收获约 220 斤\n3. 玉米预计收获约 260 斤\n4. 秋葵预计收获约 180 斤\n5. 豆角预计收获约 280 斤',
				notes: '后台支持上下架、编辑套餐信息；支付成功后可在订单中心查看联系人、联系方式和认购详情。',
				cover: createPlaceholderImage('企业团建共享田认购', '西红柿 茄子 玉米 秋葵', '#0f766e', '#22c55e'),
				carouselImages: [
					createPlaceholderImage('企业团建共享田认购', '共享田展示图', '#0f766e', '#22c55e'),
					createPlaceholderImage('企业团建共享田认购', '采收安排图', '#2563eb', '#38bdf8')
				],
				detailImages: [
					createPlaceholderImage('企业团建共享田认购', '认购内容图', '#7c3aed', '#a855f7'),
					createPlaceholderImage('企业团建共享田认购', '作物结构图', '#f97316', '#fb923c')
				]
			}, 2)
		];
	}

	function loadSubscriptions() {
		var stored = safeParse(window.localStorage.getItem(STORAGE_KEY));
		var list = Array.isArray(stored) && stored.length
			? stored.map(function (item, index) {
				return withUnselected(normalizeSubscription(item, index));
			})
			: getDefaultSubscriptions();

		if (!(Array.isArray(stored) && stored.length)) {
			saveSubscriptions(list);
		}

		return list;
	}

	function saveSubscriptions(list) {
		var normalized = (Array.isArray(list) ? list : []).map(function (item, index) {
			return normalizeSubscription(item, index);
		});
		var storageList = normalized.map(withUnselected);

		window.localStorage.setItem(STORAGE_KEY, JSON.stringify(storageList));
		return normalized;
	}

	function generateId() {
		var now = new Date();
		var prefix = [
			now.getFullYear(),
			pad(now.getMonth() + 1),
			pad(now.getDate()),
			pad(now.getHours()),
			pad(now.getMinutes()),
			pad(now.getSeconds())
		].join('');

		return 'SUB' + prefix + String(Math.floor(Math.random() * 900) + 100);
	}

	window.SubscriptionStore = {
		STORAGE_KEY: STORAGE_KEY,
		createPlaceholderImage: createPlaceholderImage,
		formatDateTime: formatDateTime,
		getDefaultSubscriptions: getDefaultSubscriptions,
		getNumericValue: getNumericValue,
		generateId: generateId,
		load: loadSubscriptions,
		normalize: normalizeSubscription,
		safeParse: safeParse,
		save: saveSubscriptions
	};
})(window);
