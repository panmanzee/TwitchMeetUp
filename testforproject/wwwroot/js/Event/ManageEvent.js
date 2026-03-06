// ── CONFIG ──
// eventId is injected by Razor: <body data-event-id="@Model.Eid">
const EVENT_ID = parseInt(document.body.dataset.eventId, 10);

const COLORS = [
    '#7c5cfc', '#fc5c7d', '#2ecc8a', '#f9a825', '#29b6f6',
    '#ab47bc', '#ff7043', '#26c6da', '#66bb6a', '#ec407a',
    '#5c6bc0', '#26a69a',
];

// ── STATE ──
let event = null;
let participated = [];
let selected = [];
let capacity = 1;
let isClosed = false;

// ── INIT ──
document.addEventListener('DOMContentLoaded', () => {
    loadEvent();
    loadParticipants();
});

// ══════════════════════════════════════════════════════
//  API CALLS
// ══════════════════════════════════════════════════════

/**
 * GET /api/events/{id}
 * Maps to Event model: Eid, Name, EventStart, EventStop,
 *   ExpiredDate, MaxParticitpant, status, Description, Location
 */
async function loadEvent() {
    try {
        const res = await fetch(`/api/events/${EVENT_ID}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        event = await res.json();
        capacity = event.maxParticitpant;

        document.getElementById('event-name').textContent = event.name;
        document.getElementById('event-status').textContent = `⬡ ${event.status.toUpperCase()}`;
        document.getElementById('start-time').value = toDateTimeLocal(event.eventStart);
        document.getElementById('end-time').value = toDateTimeLocal(event.eventStop);
        document.getElementById('expire-time').value = toDateTimeLocal(event.expiredDate);
        document.getElementById('cap-display').textContent = capacity;

        updateStats();
    } catch (err) {
        showToast('❌', `Failed to load event: ${err.message}`);
    }
}

/**
 * GET /api/events/{id}/participants
 * Returns User[] — all users in Event.Participants collection.
 * Expects joinedAt from the join-table (add this to your DTO/controller).
 */
async function loadParticipants() {
    try {
        const res = await fetch(`/api/events/${EVENT_ID}/participants`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();

        // data = { isClosed: bool, users: User[] }
        if (data.isClosed) {
            isClosed = true;
            selected = data.users.map(normaliseUser);
            participated = [];
            render();
            lockClosedUI();
        } else {
            isClosed = false;
            participated = data.users.map(normaliseUser);
            selected = [];
            render();
        }
    } catch (err) {
        showToast('❌', `Failed to load participants: ${err.message}`);
    }
}

// Disable all controls when event is already closed on page load
function lockClosedUI() {
    document.getElementById('event-status').textContent = '⬡ CLOSED';
    document.querySelectorAll('.cap-btn, .btn-fcfs, .btn-close, .btn-save, .btn-confirm').forEach(b => {
        b.disabled = true;
        b.style.opacity = '0.4';
        b.style.cursor = 'not-allowed';
    });
}

/**
 * PUT /api/events/{id}
 * Saves schedule & capacity. Body fields map to Event model properties.
 * Validation on server: EventStop > EventStart, ExpiredDate > EventStop.
 */
async function saveSettings() {
    const payload = {
        eid: EVENT_ID,
        eventStart: document.getElementById('start-time').value,
        eventStop: document.getElementById('end-time').value,
        expiredDate: document.getElementById('expire-time').value,
        maxParticitpant: capacity,
    };

    try {
        const res = await fetch(`/api/events/${EVENT_ID}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload),
        });
        if (!res.ok) {
            const body = await res.json().catch(() => ({}));
            // ASP.NET ModelState errors come back as { errors: { Field: ["msg"] } }
            const detail = flattenErrors(body);
            throw new Error(detail || `HTTP ${res.status}`);
        }
        showToast('💾', 'Settings saved successfully');
    } catch (err) {
        showToast('❌', `Save failed: ${err.message}`);
    }
}

/**
 * POST /api/events/{id}/participants/confirm
 * Body: { userIds: number[] }
 * Server replaces Event.Participants with this list.
 */
async function confirmChanges() {
    if (selected.length === 0) {
        showToast('⚠', 'No users selected yet');
        return;
    }

    const payload = { userIds: selected.map(u => u.uid) };

    try {
        const res = await fetch(`/api/events/${EVENT_ID}/participants/confirm`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload),
        });
        if (!res.ok) {
            const body = await res.json().catch(() => ({}));
            throw new Error(body.message || `HTTP ${res.status}`);
        }
        showToast('✓', `Confirmed ${selected.length} participants`);
    } catch (err) {
        showToast('❌', `Confirm failed: ${err.message}`);
    }
}

/**
 * PATCH /api/events/{id}/status
 * Body: { status: "closed" }
 * Updates Event.status field (open → closed).
 */
async function closeEvent() {
    if (selected.length === 0) {
        showToast('⚠', 'Please confirm participants before closing');
        return;
    }
    if (!confirm('Close this event? Only selected participants will remain.')) return;

    try {
        const res = await fetch(`/api/events/${EVENT_ID}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: 'closed' }),
        });
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        isClosed = true;
        participated = [];
        render();

        document.getElementById('event-status').textContent = '⬡ CLOSED';

        // Disable +/- buttons and FCFS since event is now closed
        document.querySelectorAll('.cap-btn, .btn-fcfs, .btn-close, .btn-confirm, .btn-save').forEach(b => {
            b.disabled = true;
            b.style.opacity = '0.4';
            b.style.cursor = 'not-allowed';
        });

        showToast('🔒', 'Event closed — only selected participants remain');
    } catch (err) {
        showToast('❌', `Close failed: ${err.message}`);
    }
}

// ══════════════════════════════════════════════════════
//  LOCAL ACTIONS  (optimistic UI — no server call until Confirm)
// ══════════════════════════════════════════════════════

function addUser(uid) {
    if (isClosed) return;
    if (selected.length >= capacity) {
        showToast('⚠', `Capacity full (${capacity} slots)`);
        return;
    }
    const idx = participated.findIndex(u => u.uid === uid);
    if (idx === -1) return;
    selected.push(participated.splice(idx, 1)[0]);
    render();
}

function removeUser(uid) {
    if (isClosed) return;
    const idx = selected.findIndex(u => u.uid === uid);
    if (idx === -1) return;
    participated.push(selected.splice(idx, 1)[0]);
    participated.sort((a, b) => new Date(a.joinedAt) - new Date(b.joinedAt));
    render();
}

/**
 * First-Come-First-Serve:
 * Merge all registered users, sort by joinedAt ASC, pick first `capacity` users.
 * This mirrors the logic: out of 50 registrants, take the earliest 30 (capacity).
 */
function firstComeFirstServe() {
    const all = [...participated, ...selected]
        .sort((a, b) => new Date(a.joinedAt) - new Date(b.joinedAt));
    selected = all.slice(0, capacity);
    participated = all.slice(capacity);
    render();
    showToast('⚡', `Selected top ${selected.length} by join time`);
}

function changeCapacity(delta) {
    capacity = Math.max(1, Math.min(500, capacity + delta));
    document.getElementById('cap-display').textContent = capacity;
    updateStats();
}

// ══════════════════════════════════════════════════════
//  RENDER
// ══════════════════════════════════════════════════════

function render() {
    renderList('participated-list', participated, 'add');
    renderList('selected-list', selected, 'remove');
    updateStats();
}

function renderList(containerId, users, action) {
    const el = document.getElementById(containerId);
    el.innerHTML = '';

    if (users.length === 0) {
        if (action === 'remove') {
            el.innerHTML = `
        <div class="empty-hint">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
            <circle cx="9" cy="7" r="4"/>
            <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          No users selected yet
        </div>`;
        }
        return;
    }

    // Global rank across all registered users, sorted by join time
    const allSorted = [...participated, ...selected]
        .sort((a, b) => new Date(a.joinedAt) - new Date(b.joinedAt));

    users.forEach(user => {
        const rank = allSorted.findIndex(u => u.uid === user.uid) + 1;
        const label = user.displayName || user.username || `User #${user.uid}`;
        const initials = getInitials(label);
        const color = COLORS[(user.uid - 1) % COLORS.length];
        const timeStr = new Date(user.joinedAt)
            .toLocaleTimeString('en', { hour: '2-digit', minute: '2-digit' });

        // Use profilePictureSrc (User model) if available, otherwise coloured initials
        const avatarHtml = user.profilePictureSrc
            ? `<img src="${user.profilePictureSrc}" class="avatar-img" alt="${label}">`
            : `<div class="avatar" style="background:${color}22;color:${color};border:1px solid ${color}44">${initials}</div>`;

        const item = document.createElement('div');
        item.className = 'user-item';
        item.dataset.uid = user.uid;
        item.innerHTML = `
      <div class="user-info">
        ${avatarHtml}
        <div>
          <div class="user-name">${label}</div>
          <div class="join-time">#${rank} · joined ${timeStr}</div>
        </div>
      </div>
      <button class="action-btn ${action}"
              onclick="${action === 'add' ? 'addUser' : 'removeUser'}(${user.uid})"
              title="${action === 'add' ? 'Add to selected' : 'Remove'}">
        ${action === 'add' ? '+' : '−'}
      </button>`;
        el.appendChild(item);
    });
}

function updateStats() {
    const pct = capacity > 0
        ? Math.min(100, (selected.length / capacity) * 100)
        : 0;
    document.getElementById('part-count').textContent = participated.length;
    document.getElementById('sel-count').textContent = selected.length;
    document.getElementById('cap-fill').style.width = pct + '%';
    document.getElementById('cap-warn')
        .classList.toggle('show', selected.length > capacity);
}

// ══════════════════════════════════════════════════════
//  HELPERS
// ══════════════════════════════════════════════════════

/**
 * Normalise server User → local shape.
 * User model: Uid, Username, DisplayName, ProfilePictureSrc
 * joinedAt comes from the Event–User join table; add it to your participant DTO.
 */
function normaliseUser(u) {
    return {
        uid: u.uid,
        username: u.username ?? '',
        displayName: u.displayName ?? '',
        profilePictureSrc: u.profilePictureSrc ?? null,
        joinedAt: u.joinedAt ?? new Date().toISOString(),
    };
}

function getInitials(name) {
    return name.trim().split(/\s+/).map(w => w[0]).join('').slice(0, 2).toUpperCase();
}

/**
 * Convert a .NET ISO DateTime string to the value format
 * expected by <input type="datetime-local">  →  "YYYY-MM-DDTHH:MM"
 */
function toDateTimeLocal(isoString) {
    if (!isoString) return '';
    const d = new Date(isoString);
    const pad = n => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
        `T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/**
 * Flatten ASP.NET Core ModelState / ProblemDetails errors into a single string.
 * Typical shape: { errors: { EventStop: ["EventStop must be later..."] } }
 */
function flattenErrors(body) {
    if (!body) return '';
    if (body.errors) {
        return Object.values(body.errors).flat().join(' | ');
    }
    return body.message || body.title || '';
}

// ── TOAST ──
let toastTimer;
function showToast(icon, msg) {
    const toast = document.getElementById('toast');
    document.getElementById('toast-icon').textContent = icon;
    document.getElementById('toast-msg').textContent = msg;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
}