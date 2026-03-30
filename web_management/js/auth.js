(function (window, document) {
	'use strict';

	var LOGIN_PAGE = 'login.html';
	var TOKEN_CHECK_INTERVAL = 3000;
	var activeToken = '';
	var sessionMonitorStarted = false;
	var loggingOut = false;

	function safeParse(value) {
		if (!value) {
			return null;
		}

		try {
			return JSON.parse(value);
		} catch (error) {
			console.error('解析登录用户信息失败:', error);
			return null;
		}
	}

	function getToken() {
		return (window.localStorage.getItem('token') || '').trim();
	}

	function getUserInfo() {
		return safeParse(window.localStorage.getItem('userInfo'));
	}

	function clearAuth() {
		window.localStorage.removeItem('token');
		window.localStorage.removeItem('userInfo');
	}

	function normalizeUrlPath(url) {
		if (!url) {
			return '';
		}

		try {
			return new URL(url, window.location.origin).pathname;
		} catch (error) {
			return '';
		}
	}

	function isProtectedApiRequest(url) {
		var path = normalizeUrlPath(url);
		return path.indexOf('/api/') === 0;
	}

	function redirectToLogin() {
		if (window.location.pathname.toLowerCase().endsWith('/' + LOGIN_PAGE) ||
			window.location.pathname.toLowerCase().endsWith(LOGIN_PAGE)) {
			return;
		}

		window.location.href = LOGIN_PAGE;
	}

	function forceLogout(reason) {
		if (loggingOut) {
			return;
		}

		loggingOut = true;
		console.warn(reason || '登录状态已失效，正在返回登录页');
		clearAuth();
		redirectToLogin();
	}

	function ensureAuthenticated() {
		var token = getToken();

		if (!token) {
			clearAuth();
			redirectToLogin();
			return null;
		}

		activeToken = token;

		return {
			token: token,
			userInfo: getUserInfo()
		};
	}

	function getDisplayName(userInfo) {
		if (!userInfo) {
			return '已登录用户';
		}

		return userInfo.nickname || userInfo.phone || userInfo.id || '已登录用户';
	}

	function getAvatarText(userInfo) {
		var displayName = getDisplayName(userInfo);
		return (displayName || '用').toString().trim().charAt(0) || '用';
	}

	function updateHeaderUserInfo() {
		var session = ensureAuthenticated();

		if (!session) {
			return;
		}

		var displayName = getDisplayName(session.userInfo);
		var avatarText = getAvatarText(session.userInfo);
		var userInfoBlocks = document.querySelectorAll('.user-info');

		userInfoBlocks.forEach(function (block) {
			var avatar = block.querySelector('.user-avatar');
			var textBlocks = Array.prototype.filter.call(block.children, function (child) {
				return !child.classList.contains('user-avatar');
			});

			if (avatar) {
				avatar.textContent = avatarText;
			}

			if (textBlocks.length > 0) {
				textBlocks[0].textContent = displayName;
			}
		});
	}

	function getAuthHeaders(extraHeaders) {
		var headers = Object.assign({}, extraHeaders || {});
		var token = getToken();

		if (token) {
			headers.token = token;
			headers.Authorization = 'Bearer ' + token;
		}

		return headers;
	}

	function hasTokenChanged() {
		var currentToken = getToken();

		if (!activeToken) {
			activeToken = currentToken;
			return false;
		}

		return !currentToken || currentToken !== activeToken;
	}

	function checkTokenChange() {
		if (hasTokenChanged()) {
			forceLogout('检测到登录 token 已变化，正在返回登录页');
			return false;
		}

		return true;
	}

	function startSessionMonitor() {
		if (sessionMonitorStarted) {
			return;
		}

		sessionMonitorStarted = true;
		activeToken = getToken();

		window.addEventListener('storage', function (event) {
			if (event.key === 'token') {
				checkTokenChange();
			}
		});

		window.addEventListener('focus', checkTokenChange);
		document.addEventListener('visibilitychange', function () {
			if (!document.hidden) {
				checkTokenChange();
			}
		});

		window.setInterval(checkTokenChange, TOKEN_CHECK_INTERVAL);
	}

	function wrapFetch() {
		if (typeof window.fetch !== 'function' || window.fetch.__authWrapped) {
			return;
		}

		var nativeFetch = window.fetch.bind(window);

		function getRequestUrl(input) {
			if (typeof input === 'string') {
				return input;
			}

			if (input && typeof input.url === 'string') {
				return input.url;
			}

			return '';
		}

		var wrappedFetch = function (input, init) {
			return nativeFetch(input, init).then(function (response) {
				if (isProtectedApiRequest(getRequestUrl(input)) &&
					(response.status === 401 || response.status === 403)) {
					forceLogout('登录已过期或 token 已失效，正在返回登录页');
				}

				return response;
			});
		};

		wrappedFetch.__authWrapped = true;
		window.fetch = wrappedFetch;
	}

	window.Auth = {
		requireAuth: ensureAuthenticated,
		getToken: getToken,
		getUserInfo: getUserInfo,
		getAuthHeaders: getAuthHeaders,
		clearAuth: clearAuth,
		redirectToLogin: redirectToLogin,
		updateHeaderUserInfo: updateHeaderUserInfo
	};

	if (ensureAuthenticated()) {
		wrapFetch();
		startSessionMonitor();

		if (document.readyState === 'loading') {
			document.addEventListener('DOMContentLoaded', updateHeaderUserInfo);
		} else {
			updateHeaderUserInfo();
		}
	}
})(window, document);
