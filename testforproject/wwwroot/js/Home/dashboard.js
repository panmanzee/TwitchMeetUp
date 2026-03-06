async function slide(direction) {
    const container = document.getElementById("categorylist");


    const widthToScroll = container.clientWidth;

    if (direction === 'right') {
        container.scrollBy({
            left: widthToScroll,
            behavior: 'smooth'
        });
    } else {
        container.scrollBy({
            left: -widthToScroll,
            behavior: 'smooth'
        });
    }
}

function toggleSortMenu() {
    var menu = document.getElementById("sortMenu");
    if (menu.style.display === "none" || menu.style.display === "") {
        menu.style.display = "block";
    } else {
        menu.style.display = "none";
    }
}




function selectCategory(categoryName, ele) {
    document.getElementById("categoryInput").value = categoryName;
    var options = document.querySelectorAll('.category-slide .option');
    options.forEach(opt => opt.classList.remove('active'));

    ele.classList.add('active');

    fetchAjaxData();
}

document.getElementById("searchForm").addEventListener("submit", function (event) {
    event.preventDefault();
    fetchAjaxData();
});


function applySort(orderValue) {
    document.getElementById("sortOrderInput").value = orderValue;


    document.getElementById("sortMenu").style.display = "none";


    fetchAjaxData();
}


function fetchAjaxData() {

    var query = document.getElementById("search-group").value;
    var sort = document.getElementById("sortOrderInput").value;
    var category = document.getElementById("categoryInput").value;

    // Reset skip counter for Load More functionality
    if (typeof currentSkip !== 'undefined') {
        currentSkip = 4;
    }

    fetch(`/Dashboard/SearchEventsAJAX?searchQuery=${encodeURIComponent(query)}&sortorder=${encodeURIComponent(sort)}&categoryFilter=${encodeURIComponent(category)}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById("eventGridContainer").innerHTML = html;

            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const loadedCount = doc.querySelectorAll('.event-card').length;

            const viewMoreContainer = document.getElementById('viewMore');
            if (viewMoreContainer) {
                if (loadedCount < 4) {
                    viewMoreContainer.style.display = 'none';
                } else {
                    viewMoreContainer.style.display = 'block';
                }
            }
        })
        .catch(error => console.error('Error fetching data:', error));
}

window.onclick = function (event) {
    if (!event.target.matches('.hamburger') && !event.target.closest('#sortMenu')) {
        var menu = document.getElementById("sortMenu");
        if (menu && menu.style.display === "block") {
            menu.style.display = "none";
        }
    }

    if (!event.target.closest('#profileDropdownBtn') && !event.target.closest('#profileDropdown')) {
        var profileMenu = document.getElementById("profileDropdown");
        if (profileMenu && profileMenu.style.display === "block") {
            profileMenu.style.display = "none";
        }
    }
}