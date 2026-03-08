let current = 0;

async function logout() {
    await fetch('/api/AccountApi/logout', {
        method: 'POST',
        credentials: 'include'
    });

    location.reload();
}

// for carousel slider
function slide(dir) {
    const track = document.getElementById('carouselTrack');
    const imgWidth = 220 + 12;
    const totalImages = track.children.length;

    const wrapperWidth = track.parentElement.offsetWidth;
    const totalWidth = totalImages * imgWidth - 12;
    const maxScroll = totalWidth - wrapperWidth;
    const maxSlide = Math.ceil(maxScroll / imgWidth);

    current = Math.max(0, Math.min(current + dir, maxSlide));

    const translateX = Math.min(current * imgWidth, maxScroll);
    track.style.transform = `translateX(-${translateX}px)`;

    document.querySelector('.left-arrow').style.opacity = current === 0 ? '0' : '1';
    document.querySelector('.right-arrow').style.opacity = translateX >= maxScroll ? '0' : '1';
}

function scroll(dir) {
    const el = document.getElementById('attendScroll');
    el.scrollBy({ left: dir * 120, behavior: 'smooth' });
}

// For photo section
//const mockPhotos = [
//    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
//    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
//    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
//    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
//];
function renderPhotos() {
    const emptyState = document.querySelector('.empty-state');
    const carousel = document.querySelector('.carousel-wrapper');
    const track = document.querySelector('.carousel-track');
    const photoCount = document.querySelector('.photo-count');

    console.log(mockPhotos.length)
    if (mockPhotos.length === 0) {
        carousel.style.display = 'none';
        emptyState.style.display = 'flex';
    }
    else {
        carousel.style.display = 'flex';
        emptyState.style.display = 'none';
        photoCount.textContent = mockPhotos.length;

        track.innerHTML = mockPhotos
            .map(url => `<img src="${url}">`)
            .join('');
    }
    console.log(track.innerHTML)
    const leftArrow = document.querySelector('.left-arrow');
    const rightArrow = document.querySelector('.right-arrow');

    leftArrow.style.opacity = '0';
    rightArrow.style.opacity = mockPhotos.length <= 1 ? '0' : '1';
}

// For remain time to Close Register
function updateRemainTime() {
    const closeTime = new Date('2027-02-23T22:00:00');
    const now = new Date();
    const diff = closeTime - now;

    if (diff <= 0) {
        document.getElementById('remainTime').textContent = 'Registration closed';
        document.getElementById('remainTime').style.color = 'red';
        return;
    }

    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const mins = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

    document.getElementById('remainTime').textContent =
        `${days}d ${hours}h ${mins}m remaining`;
}

async function joinEvent(eventId) {
    const response = await fetch('/api/EventApi/join', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({ eventId: eventId })
    });

    const data = await response.json();

    if (response.ok) {
        showToast('✅ ' + data.message, 'success');
        setTimeout(() => location.reload(), 1000);
    } else {
        showToast('❌ ' + data.message, 'error');;
    }
}

function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = `toast ${type} show`;
    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

function decodeHtml(html) {
    const txt = document.createElement('textarea');
    txt.innerHTML = html;
    return txt.value;
}

async function initMap() {
    const decoded = decodeHtml(eventLocation); // ← decode ก่อน!
    console.log('decoded:', decoded);

    const res = await fetch(
        `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(decoded)}&format=json&limit=1`
    );
    const data = await res.json();

    if (data.length > 0) {
        const lat = parseFloat(data[0].lat);
        const lng = parseFloat(data[0].lon);
        const map = L.map('map').setView([lat, lng], 15);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
        L.marker([lat, lng]).addTo(map).bindPopup(decoded).openPopup();
    } else {
        // ถ้ายัง geocode ไม่ได้ แสดง placeholder
        document.getElementById('map').innerHTML =
            '<div style="display:flex;align-items:center;justify-content:center;height:100%;color:var(--text-muted);">📍 Location not found</div>';
    }
}


window.onload = () => {
    updateRemainTime();
    setInterval(updateRemainTime, 60000);
    renderPhotos();
    document.querySelector('.left-arrow').style.opacity = '0';

    document.querySelector('.left-arrow').style.opacity = '0';

    initMap();

    //---------------------------------------------
    const readmoreBtn = document.getElementById('readMoreBtn');
    const detailText = document.querySelector('.detail');

    function toggleText() {
        detailText.classList.toggle("expanded");
        readmoreBtn.textContent = detailText.classList.contains("expanded")
            ? "Read Less"
            : "Read More";
    }

    function checkOverflow() {
        if (detailText.scrollHeight <= detailText.clientHeight) {
            readmoreBtn.style.display = "none";
        } else {
            readmoreBtn.style.display = "inline";
        }
    }

    readmoreBtn.addEventListener('click', toggleText);
    checkOverflow();
    //---------------------------------------------

    
}