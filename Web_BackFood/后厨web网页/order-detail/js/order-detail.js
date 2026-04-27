// API基础URL
const API_BASE_URL = '/api';

let currentOrder = null;

// 页面加载时检查登录状态和获取订单数据
window.onload = function() {
    if (!localStorage.getItem('loggedIn')) {
        window.location.href = '../login/index.html';
    }
    
    // 显示当前登录用户名
    const username = localStorage.getItem('username');
    if (username) {
        document.getElementById('current-username').textContent = username;
    }
    
    const orderId = localStorage.getItem('currentOrderId');
    if (!orderId) {
        window.location.href = '../dashboard/index.html';
    }
    
    // 从API获取订单数据
    fetchOrderDetail(orderId);
};

// 从API获取订单详情
async function fetchOrderDetail(orderId) {
    // 显示加载状态
    document.getElementById('items-list').innerHTML = '<div style="text-align: center; padding: 60px 20px; color: #999;">加载中...</div>';
    
    try {
        // 设置超时时间
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 5000); // 5秒超时
        
        // 添加时间戳参数，避免浏览器缓存
        const timestamp = new Date().getTime();
        const response = await fetch(`${API_BASE_URL}/orders/${orderId}?timestamp=${timestamp}`, {
            signal: controller.signal,
            headers: {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0'
            }
        });
        
        clearTimeout(timeoutId);
        
        if (!response.ok) {
            throw new Error('Failed to fetch order details');
        }
        
        currentOrder = await response.json();
        renderOrderDetails();
    } catch (error) {
        console.error('Error fetching order details:', error);
        // 显示错误提示
        document.getElementById('items-list').innerHTML = '<div style="text-align: center; padding: 60px 20px; color: #999;">获取订单详情失败，请检查网络连接后返回首页重试</div>';
    }
};

// 渲染订单详情
function renderOrderDetails() {
    document.getElementById('order-id').textContent = currentOrder.id;
    document.getElementById('order-time').textContent = currentOrder.time;
    document.getElementById('table-number').textContent = currentOrder.table;
    
    const itemsList = document.getElementById('items-list');
    itemsList.innerHTML = currentOrder.items.map((item, index) => `
        <div class="item">
            <div class="item-info">
                <span class="item-name">${item.name}</span>
                <span class="item-quantity">x${item.quantity}</span>
            </div>
            <div class="item-status">
                <span class="status-text ${item.status ? 'completed' : 'pending'}">
                    ${item.status ? '已出餐' : '未出餐'}
                </span>
                <button class="complete-button" 
                        ${item.status ? 'disabled' : ''}
                        onclick="markAsCompleted(${index})">
                    ${item.status ? '已出餐' : '出餐'}
                </button>
            </div>
        </div>
    `).join('');
}

// 标记菜品为已出餐
async function markAsCompleted(index) {
    const button = event.target;
    button.classList.add('loading');
    button.disabled = true;
    
    try {
        // 设置超时时间
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 8000); // 8秒超时
        
        // 通过API标记菜品为已出餐
        const response = await fetch(`${API_BASE_URL}/orders/${currentOrder.id}/items/${index}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ status: true }),
            signal: controller.signal
        });
        
        clearTimeout(timeoutId);
        
        if (!response.ok) {
            throw new Error('Failed to mark item as completed');
        }
        
        // 更新本地订单数据
        currentOrder = await response.json();
        
        // 重新渲染当前页面
        renderOrderDetails();
        
        // 检查是否所有菜品都已出餐
        const allCompleted = currentOrder.items.every(item => item.status);
        if (allCompleted) {
            // 显示完成提示，不再自动返回首页
            button.textContent = '已出餐';
            button.disabled = true;
        }
        
        // 更新父页面的订单数据
        if (window.opener && window.opener.refreshOrders) {
            window.opener.refreshOrders();
        }
    } catch (error) {
        console.error('Error marking item as completed:', error);
        // 显示错误提示
        document.getElementById('error-message').style.display = 'block';
        // 恢复按钮状态
        button.disabled = false;
        button.classList.remove('loading');
    } finally {
        button.classList.remove('loading');
    }
}

// 返回首页
function goBack() {
    window.location.href = '../dashboard/index.html';
}