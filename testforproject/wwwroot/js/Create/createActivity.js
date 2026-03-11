function toggle_category() {
    var drop = document.getElementById("drop");
    if (drop.style.display == "none" || drop.style.display === "") {
        drop.style.display = "block";
    } else {
        drop.style.display = "none";
    }
}

async function searchLocation(query) {
    const res = await fetch(
        `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=5`
    );
    const data = await res.json();
    return data;
}

function show_Select_categories() {
    var checkboxs = document.querySelectorAll('#drop input[type="checkbox"]:checked');
    var text = document.getElementById("select");

    if (checkboxs.length == 0) {
        text.innerText = "Select categories"
    } else if (checkboxs.length == 1) {
        text.innerText = checkboxs[0].nextSibling.nodeValue.trim();
    } else {
        var names = Array.from(checkboxs).map(cb => cb.closest('label').textContent.trim());
        text.innerText = names.join(", ");

    }
}
let map;
let marker;

const searchInput = document.getElementById('locationSearch');
const suggestions = document.getElementById('locationSuggestions');

let searchTimeout;

searchInput.addEventListener('input', function () {
    clearTimeout(searchTimeout);
    const query = this.value.trim();

    if (query.length < 3) {
        suggestions.innerHTML = '';
        return;
    }

    searchTimeout = setTimeout(async () => {
        const res = await fetch(
            `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=5`
        );
        const data = await res.json();

        suggestions.innerHTML = '';

        data.forEach(place => {
            const item = document.createElement('div');
            item.style.cssText = `
                padding: 10px 14px; 
                cursor: pointer; 
                font-size: 13px;
                border-bottom: 1px solid #2a2a30;
                color: #f0eff5;
            `;
            item.textContent = place.display_name;

            item.addEventListener('mouseenter', () => item.style.background = '#2a2a30');
            item.addEventListener('mouseleave', () => item.style.background = 'transparent');

            item.addEventListener('mousedown', (e) => {
                e.preventDefault(); // ← สำคัญมากครับ! ป้องกัน blur

                const lat = parseFloat(place.lat);
                const lon = parseFloat(place.lon);

                if (window.map) {
                    window.map.setView([lat, lon], 15);
                    if (window.currentMarker) window.map.removeLayer(window.currentMarker);
                    window.currentMarker = L.marker([lat, lon]).addTo(window.map);
                }

                document.getElementById('locationSearch').value = place.display_name;
                document.getElementById('locationSuggestions').innerHTML = '';
            });

            suggestions.appendChild(item);
        });
    }, 500);
});

document.addEventListener('click', (e) => {
    if (!searchInput.contains(e.target)) {
        suggestions.innerHTML = '';
    }
});

document.getElementById('location').addEventListener('click', function () {
    var mapContainer = document.getElementById('map-container');

    // Toggle map visibility
    if (mapContainer.style.display === 'none') {
        mapContainer.style.display = 'block';

        // If map hasn't been created yet, initialize it
        if (!map) {
            // Centers the map in Thailand by default
            map = L.map('map-container').setView([13.644, 100.680], 12);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap contributors'
            }).addTo(map);

            // Listen for clicks on the map
            map.on('click', function (e) {
                let lat = e.latlng.lat;
                let lng = e.latlng.lng;

                // Move the marker to the clicked spot
                if (marker) {
                    map.removeLayer(marker);
                }
                marker = L.marker([lat, lng]).addTo(map);

                // Add a temporary "Loading..." text
                document.getElementById('locationSearch').value = "Loading address...";

                // Pull the address using the free OpenStreetMap API
                fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`)
                    .then(response => response.json())
                    .then(data => {
                        // Put the real address into the text box
                        let placeName = data.display_name;
                        document.getElementById('locationSearch').value = placeName;

                        // Automatically hide the map after selecting (optional)
                        setTimeout(() => { mapContainer.style.display = 'none'; }, 500);
                    })
                    .catch(error => {
                        document.getElementById('locationSearch').value = `${lat}, ${lng}`;
                    });
            });
        }

        // This fixes a bug where the map looks broken when unhidden
        setTimeout(function () { map.invalidateSize(); }, 100);
    } else {
        mapContainer.style.display = 'none';
    }
});

// --- Form Validation Logic ---
document.querySelector("form").addEventListener("submit", function (e) {
    const start = new Date(document.getElementById("StartTime").value);
    const stop = new Date(document.getElementById("closetime").value);
    const expired = new Date(document.getElementById("Expired-Date").value);
    const max = parseInt(document.getElementById("max").value);
    const name = document.getElementById("name").value.trim();
    const location = document.getElementById("locationSearch").value.trim();
    const description = document.getElementById("description").value.trim(); 
    const now = new Date();
    let errors = [];

    // ADD: Required field checks
    if (!name) errors.push("Title is required.");
    if (!location) errors.push("Location is required.");
    if (!description) errors.push("Description is required.");

    // Check for missing dates so it doesn't crash
    if (isNaN(start.getTime())) errors.push("Please select an Event Start time.");
    if (isNaN(stop.getTime())) errors.push("Please select an Event Stop time.");

    // Validation rules
    if (stop <= start) errors.push("Event Stop must be later than Event Start.");
    if (expired > start) errors.push("Expired Date must be before Event Start.");
    if (max < 1 || isNaN(max)) errors.push("Max Participants must be at least 1.");
    if (start < now) errors.push("Event Start cannot be in the past.");

    if (errors.length > 0) {
        e.preventDefault();
        const box = document.getElementById("clientErrors");
        box.innerHTML = errors.map(err => `<p style="margin:4px 0;">⚠️ ${err}</p>`).join('');
        box.style.display = "block";
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
});