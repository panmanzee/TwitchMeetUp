let current = 0;

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

// For photo section
const mockPhotos = [
    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
    "https://m.media-amazon.com/images/S/pv-target-images/fe3fd7d72c5b6a289353e46dff534db8da37702d99366bb4fb571ede0abf5387._SX1080_FMjpg_.jpg",
];

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


// For read more button
const readmoreBtn = document.getElementById('readMoreBtn');
const detailText = document.querySelector('.detail');

function checkOverflow() {
    // scrollHeight = full height of content
    // clientHeight = visible height
    if (text.scrollHeight <= text.clientHeight) {
        readmoreBtn.style.display = "none";
    } else {
        readmoreBtn.style.display = "inline";
    }
}

readmoreBtn.addEventListener("click", function () {
    detailText.classList.toggle("expanded");

    if (detailText.classList.contains("expanded")) {
        readmoreBtn.textContent = "Read Less";
    }
    else {
        readmoreBtn.textContent = "Read More";
    }
});



window.onload = () => {
    updateRemainTime();
    setInterval(updateRemainTime, 60000); // อัพเดตทุก 1 นาที

    renderPhotos();
    document.querySelector('.left-arrow').style.opacity = '0';

    document.querySelector('.left-arrow').style.opacity = '0';
}