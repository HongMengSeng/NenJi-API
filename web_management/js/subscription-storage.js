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

	function escapeSvgText(value) {
		return String(value || '')
			.replace(/&/g, '&amp;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;')
			.replace(/"/g, '&quot;')
			.replace(/'/g, '&#39;');
	}

	function createPlaceholderImage(title, subtitle, colorStart, colorEnd) {
		var safeTitle = escapeSvgText(title || '一亩田认购');
		var safeSubtitle = escapeSvgText(subtitle || '田间封面待补充');
		var fieldColor = colorStart || '#7faa52';
		var soilColor = colorEnd || '#c88e5a';
		var svg = [
			'<svg xmlns="http://www.w3.org/2000/svg" width="720" height="480" viewBox="0 0 720 480">',
			'<rect width="720" height="480" rx="32" fill="#f7f1e6"/>',
			'<rect x="40" y="40" width="640" height="400" rx="28" fill="#fffaf1" stroke="#e6d7bc" stroke-width="2"/>',
			'<circle cx="592" cy="128" r="42" fill="#f2d89b"/>',
			'<path d="M80 326 C180 274 258 288 352 328 C438 364 536 366 640 308 L640 400 L80 400 Z" fill="#dce6bf"/>',
			'<path d="M80 354 C190 308 282 320 376 356 C470 392 560 392 640 340 L640 400 L80 400 Z" fill="' + fieldColor + '"/>',
			'<path d="M80 384 C198 350 304 356 414 384 C514 408 580 402 640 374 L640 400 L80 400 Z" fill="' + soilColor + '"/>',
			'<path d="M548 188 C548 160 566 136 592 124 C594 150 588 175 574 198 Z" fill="#7faa52"/>',
			'<path d="M594 198 C578 172 578 142 592 124 C614 142 626 170 624 198 Z" fill="#93bb60"/>',
			'<path d="M574 198 L600 198" stroke="#6b8f43" stroke-width="4" stroke-linecap="round"/>',
			'<text x="78" y="158" fill="#547333" font-size="18" font-family="Microsoft YaHei, sans-serif" font-weight="700" letter-spacing="3">FARM NOTE</text>',
			'<text x="78" y="222" fill="#31402a" font-size="40" font-family="Microsoft YaHei, sans-serif" font-weight="700">' + safeTitle + '</text>',
			'<text x="78" y="274" fill="#7a5b3c" font-size="24" font-family="Microsoft YaHei, sans-serif">' + safeSubtitle + '</text>',
			'<text x="78" y="336" fill="#9d7d58" font-size="20" font-family="Microsoft YaHei, sans-serif">农场风认购档案占位图</text>',
			'</svg>'
		].join('');

		return 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(svg);
	}

	function decodeSvgContent(value) {
		if (typeof value !== 'string' || value.indexOf('data:image/svg+xml') !== 0) {
			return '';
		}

		try {
			return decodeURIComponent(value.split(',')[1] || '');
		} catch (error) {
			return '';
		}
	}

	function isLegacyPlaceholderImage(value) {
		var svg = decodeSvgContent(value);

		return Boolean(svg) && (
			svg.indexOf('农场认购套餐展示图') !== -1 ||
			(
				svg.indexOf('<linearGradient id="bg"') !== -1 &&
				svg.indexOf('fill="url(#bg)"') !== -1 &&
				svg.indexOf('Microsoft YaHei, sans-serif') !== -1
			)
		);
	}

	function usePlaceholderIfNeeded(value, title, subtitle, colorStart, colorEnd) {
		var text = value === null || value === undefined ? '' : String(value).trim();

		if (!text || isLegacyPlaceholderImage(text)) {
			return createPlaceholderImage(title, subtitle, colorStart, colorEnd);
		}

		return text;
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
		var cover = usePlaceholderIfNeeded(
			raw.cover || raw.coverImage || carouselImages[0] || detailImages[0],
			name,
			'田间封面待补充',
			'#7faa52',
			'#c88e5a'
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
			createPlaceholderImage(name, '农事周期记录', '#93bb60', '#b57b4a')
		]).map(function (image, imageIndex) {
			return usePlaceholderIfNeeded(
				image,
				name,
				imageIndex === 0 ? '田间封面待补充' : '农事周期记录',
				imageIndex === 0 ? '#7faa52' : '#93bb60',
				imageIndex === 0 ? '#c88e5a' : '#b57b4a'
			);
		}).slice(0, 5);
		var normalizedDetailImages = (detailImages.length ? detailImages : [
			createPlaceholderImage(name, '收获记录待补充', '#cfb17a', '#b57b4a'),
			createPlaceholderImage(name, '田间细节待补充', '#8ead63', '#d2a46c')
		]).map(function (image, imageIndex) {
			return usePlaceholderIfNeeded(
				image,
				name,
				imageIndex === 0 ? '收获记录待补充' : '田间细节待补充',
				imageIndex === 0 ? '#cfb17a' : '#8ead63',
				imageIndex === 0 ? '#b57b4a' : '#d2a46c'
			);
		}).slice(0, 5);

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
				cover: createPlaceholderImage('春季时令一亩田认购', '番茄 黄瓜 玉米 青菜', '#7faa52', '#c88e5a'),
				carouselImages: [
					createPlaceholderImage('春季时令一亩田认购', '地块全景记录', '#7faa52', '#c88e5a'),
					createPlaceholderImage('春季时令一亩田认购', '作物长势记录', '#93bb60', '#b57b4a')
				],
				detailImages: [
					createPlaceholderImage('春季时令一亩田认购', '田块作物清单', '#cfb17a', '#b57b4a'),
					createPlaceholderImage('春季时令一亩田认购', '采收节奏示意', '#8ead63', '#d2a46c')
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
				cover: createPlaceholderImage('亲子体验一亩田认购', '草莓 生菜 胡萝卜 南瓜', '#8ead63', '#d2a46c'),
				carouselImages: [
					createPlaceholderImage('亲子体验一亩田认购', '亲子巡田记录', '#8ead63', '#d2a46c'),
					createPlaceholderImage('亲子体验一亩田认购', '采摘时光记录', '#cfb17a', '#b57b4a')
				],
				detailImages: [
					createPlaceholderImage('亲子体验一亩田认购', '认购体验清单', '#cfb17a', '#b57b4a'),
					createPlaceholderImage('亲子体验一亩田认购', '作物成熟记录', '#7faa52', '#c88e5a')
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
				cover: createPlaceholderImage('企业团建共享田认购', '西红柿 茄子 玉米 秋葵', '#7faa52', '#b57b4a'),
				carouselImages: [
					createPlaceholderImage('企业团建共享田认购', '共享田全景记录', '#7faa52', '#b57b4a'),
					createPlaceholderImage('企业团建共享田认购', '团建采收安排', '#93bb60', '#d2a46c')
				],
				detailImages: [
					createPlaceholderImage('企业团建共享田认购', '认购权益清单', '#cfb17a', '#b57b4a'),
					createPlaceholderImage('企业团建共享田认购', '作物结构记录', '#8ead63', '#d2a46c')
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
