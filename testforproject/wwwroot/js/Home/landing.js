// Generate floating particles
const container = document.getElementById('particles');
for (let i = 0; i < 35; i++) {
    const p = document.createElement('div');
    p.className = 'particle';
    const size = Math.random() * 3 + 1;
    p.style.cssText = [
        'width:' + size + 'px',
        'height:' + size + 'px',
        'left:' + (Math.random() * 100) + '%',
        'animation-duration:' + (Math.random() * 18 + 12) + 's',
        'animation-delay:' + (Math.random() * 18) + 's',
        'opacity:' + (Math.random() * 0.3 + 0.05)
    ].join(';');
    container.appendChild(p);
}

// Navbar scroll effect
window.addEventListener('scroll', function () {
    document.getElementById('lp-nav').classList.toggle('lp-nav-scrolled', window.scrollY > 30);
});

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(function (a) {
    a.addEventListener('click', function (e) {
        e.preventDefault();
        var target = document.querySelector(a.getAttribute('href'));
        if (target) target.scrollIntoView({ behavior: 'smooth' });
    });
});

// Hamburger menu
var hamburger = document.getElementById('lp-hamburger');
var mobileMenu = document.getElementById('lp-mobile-menu');
hamburger.addEventListener('click', function () {
    hamburger.classList.toggle('is-open');
    mobileMenu.classList.toggle('is-open');
    document.body.style.overflow = mobileMenu.classList.contains('is-open') ? 'hidden' : '';
});
mobileMenu.querySelectorAll('a').forEach(function (link) {
    link.addEventListener('click', function () {
        hamburger.classList.remove('is-open');
        mobileMenu.classList.remove('is-open');
        document.body.style.overflow = '';
    });
});
