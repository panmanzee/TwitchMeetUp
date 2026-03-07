
const API_BASE = '/api/NotiApi';

async function fetchNotifications() {
    const res = await fetch(`${API_BASE}/GetNoti`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    });

    if (!res.ok) throw new Error(`Failed to fetch notifications: ${res.status}`);
    return await res.json();
}

async function markAsRead(id) {
    const res = await fetch(`${API_BASE}/MarkRead/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        }
    });

    if (!res.ok) throw new Error(`Failed to mark notification as read: ${res.status}`);
    return await res.json();
}

function formatRelativeTime(dateStr) {
    const now = new Date();
    const date = new Date(dateStr);
    const diffMs = now - date;
    const diffMin = Math.floor(diffMs / 60000);
    const diffHr = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHr / 24);

    if (isNaN(date.getTime())) return dateStr;
    if (diffMin < 1) return 'เมื่อกี้';
    if (diffMin < 60) return `${diffMin} นาทีที่ผ่านมา`;
    if (diffHr < 24) return `${diffHr} ชั่วโมงที่ผ่านมา`;
    if (diffDay < 30) return `${diffDay} วันที่ผ่านมา`;
    return date.toLocaleDateString('th-TH');
}

function buildItem(n) {
    // Show the user who triggered the event (invoke the event)
    const displayUser = n.triggerUser || n.user;

    const initials = displayUser?.username
        ? displayUser.username.slice(0, 2).toUpperCase()
        : '🔔';

    const avatarContent = displayUser?.profileImage
        ? `<img src="${displayUser.profileImage}" alt="${displayUser.username ?? ''}">`
        : initials;

    const href = n.href || '#';

    return `
    <div class="noti-item ${!n.isReaded ? 'unread' : ''}" data-id="${n.id}" data-href="${href}">
      <div class="avatar">${avatarContent}</div>
      <div class="noti-content">
        <div class="noti-title">${n.title}</div>
        <div class="noti-description">${n.description}</div>
        <div class="noti-time">${formatRelativeTime(n.date)}</div>
      </div>
      <button class="more-btn" title="More options">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
          <path d="M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z"/>
        </svg>
      </button>
    </div>
  `;
}

function renderLoading() {
    document.getElementById('notiList').innerHTML = `
    <div style="padding:32px;text-align:center;color:#666;font-size:13px;">
      <div style="margin-bottom:8px;">⏳</div>
      กำลังโหลด...
    </div>`;
}

function renderError(message) {
    document.getElementById('notiList').innerHTML = `
    <div style="padding:32px;text-align:center;color:#f87171;font-size:13px;">
      <div style="margin-bottom:8px;">⚠️</div>
      ${message}
    </div>`;
}

function renderEmpty() {
    document.getElementById('notiList').innerHTML = `
    <div style="padding:32px;text-align:center;color:#666;font-size:13px;">
      <div style="margin-bottom:8px;">🔔</div>
      ไม่มีการแจ้งเตือน
    </div>`;
}

function renderNotifications(allNotis) {
    const list = document.getElementById('notiList');

    if (!allNotis || allNotis.length === 0) {
        renderEmpty();
        return;
    }

    const unread = allNotis.filter(n => !n.isReaded);
    const read = allNotis.filter(n => n.isReaded);

    let html = '';

    if (unread.length > 0) {
        html += `<div class="section-label">สำคัญ</div>`;
        unread.forEach(n => (html += buildItem(n)));
    }

    if (read.length > 0) {
        if (unread.length > 0) html += `<div class="divider"></div>`;
        html += `<div class="section-label">การแจ้งเตือนเพิ่มเติม</div>`;
        read.forEach(n => (html += buildItem(n)));
    }

    list.innerHTML = html;

    list.querySelectorAll('.noti-item').forEach(el => {
        el.addEventListener('click', async e => {
            if (e.target.closest('.more-btn')) return;

            const id = parseInt(el.dataset.id);
            const href = el.dataset.href;

            if (el.classList.contains('unread')) {
                try {
                    await markAsRead(id);
                    el.classList.remove('unread');
                    updateBadge();
                } catch (err) {
                    console.error('Mark as read failed:', err);
                }
            }

            if (href && href !== '#') {
                window.location.href = href;
            }
        });
    });
}

function updateBadge() {
    const unreadCount = document.querySelectorAll('.noti-item.unread').length;
    const badge = document.getElementById('badgeCount');
    if (unreadCount === 0) {
        badge.style.display = 'none';
    } else {
        badge.style.display = '';
        badge.textContent = unreadCount > 9 ? '9+' : unreadCount;
    }
}

async function initBadge() {
    try {
        const notis = await fetchNotifications();
        const unreadCount = notis.filter(n => !n.isReaded).length;
        const badge = document.getElementById('badgeCount');
        if (unreadCount === 0) {
            badge.style.display = 'none';
        } else {
            badge.style.display = '';
            badge.textContent = unreadCount > 9 ? '9+' : unreadCount;
        }
    } catch (err) {
        console.warn('Could not load notification count:', err);
        const badge = document.getElementById('badgeCount');
        if (badge) badge.style.display = 'none';
    }
}

const bellBtn = document.getElementById('bellBtn');
const notiPanel = document.getElementById('notiPanel');

if (bellBtn) {
    bellBtn.addEventListener('click', async e => {
        e.stopPropagation();
        const isOpen = notiPanel.classList.toggle('open');

        if (isOpen) {
            renderLoading();
            try {
                const notis = await fetchNotifications();
                renderNotifications(notis);
                updateBadge();
            } catch (err) {
                console.error('Failed to load notifications:', err);
                renderError('โหลดการแจ้งเตือนไม่สำเร็จ');
            }
        }
    });
}

document.addEventListener('click', e => {
    if (notiPanel && !notiPanel.contains(e.target)) {
        notiPanel.classList.remove('open');
    }
});

initBadge();
