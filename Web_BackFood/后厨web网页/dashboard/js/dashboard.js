// API基础URL
const API_BASE_URL = '/api';

// 订单数据
let orders = [];
let currentTab = 'pending';

// 页面加载时检查登录状态
window.onload = function() {
    if (!localStorage.getItem('loggedIn')) {
        window.location.href = '../login/index.html';
    }
    
    // 显示当前登录用户名
    const username = localStorage.getItem('username');
    if (username) {
        document.getElementById('current-username').textContent = username;
    }
    
    // 从API获取订单数据
    fetchOrders();
};

// 从API获取订单数据
async function fetchOrders() {
    const orderList = document.getElementById('order-list');
    orderList.innerHTML = '<div class="no-orders">加载中...</div>';
    
    try {
        console.log('页面URL:', window.location.href);
        console.log('API基础URL:', API_BASE_URL);
        
        const timestamp = new Date().getTime();
        const url = `${API_BASE_URL}/orders?timestamp=${timestamp}`;
        console.log('请求URL:', url);
        
        const response = await fetch(url);
        console.log('响应状态:', response.status);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        
        const data = await response.json();
        console.log('获取到数据:', data);
        
        orders = data;
        renderOrders();
        updateRevenue();
    } catch (error) {
        console.error('获取订单失败:', error);
        orderList.innerHTML = `<div class="no-orders">获取订单失败: ${error.message}</div>`;
    }
};

// 切换标签
function switchTab(tab) {
    currentTab = tab;
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    event.currentTarget.classList.add('active');
    renderOrders();
}

// 渲染订单列表
function renderOrders() {
    const orderList = document.getElementById('order-list');
    const filteredOrders = currentTab === 'pending' 
        ? orders.filter(order => !order.items.every(item => item.status))
        : orders.filter(order => order.items.every(item => item.status));
    
    if (filteredOrders.length === 0) {
        orderList.innerHTML = '<div class="no-orders">暂无' + (currentTab === 'pending' ? '待出餐' : '已出餐') + '订单</div>';
        return;
    }
    
    orderList.innerHTML = filteredOrders.map(order => {
        const completedItems = order.items.filter(item => item.status).length;
        return `
            <div class="order-card" onclick="openOrderDetail('${order.id}')">
                <div class="order-header">
                    <span class="order-id">${order.id}</span>
                    <span class="order-time">${order.time}</span>
                </div>
                <div class="order-info">
                    <span class="table-number">${order.table}</span>
                    <span class="status">${completedItems}/${order.items.length} 已出餐</span>
                </div>
            </div>
        `;
    }).join('');
}

// 打开订单详情
        function openOrderDetail(orderId) {
            localStorage.setItem('currentOrderId', orderId);
            window.location.href = '../order-detail/index.html';
        }

// 更新营业额
function updateRevenue() {
    // 只计算已经完成出餐的订单
    const completedOrders = orders.filter(order => order.items.every(item => item.status));
    const total = completedOrders.reduce((sum, order) => sum + order.total, 0);
    document.getElementById('total-revenue').textContent = total;
}

// 退出登录
function logout() {
    // 清除所有本地缓存数据
    localStorage.removeItem('loggedIn');
    localStorage.removeItem('username');
    localStorage.removeItem('orders');
    localStorage.removeItem('currentOrderId');
    // 跳转到登录页面
    window.location.href = '../login/index.html';
}

// 暴露订单数据给其他页面
window.orders = orders;
window.updateOrders = function(updatedOrders) {
    orders = updatedOrders;
    renderOrders();
    updateRevenue();
};

// 刷新订单数据
function refreshOrders() {
    fetchOrders();
};

// 清理缓存
async function clearCache() {
    if (confirm('确定要清理缓存并重置数据吗？这将恢复到初始状态。')) {
        try {
            // 调用API重置数据
            const resetResponse = await fetch(`${API_BASE_URL}/reset`);
            if (!resetResponse.ok) {
                throw new Error('Failed to reset server data');
            }
            
            // 清除本地缓存
            localStorage.removeItem('orders');
            localStorage.removeItem('currentOrderId');
            
            // 重新获取订单数据
            await fetchOrders();
            alert('缓存已清理，数据已重置为初始状态');
        } catch (error) {
            console.error('Error clearing cache:', error);
            alert('清理缓存失败，请刷新页面重试');
        }
    }
};