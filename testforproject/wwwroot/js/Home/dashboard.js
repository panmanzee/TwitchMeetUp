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

// 1. ฟังก์ชันคำนวณตำแหน่ง
function parallax() {
    var s = document.getElementById("floater");
    if (s) { // เช็คก่อนว่ามี ID นี้อยู่บนหน้าเว็บจริงๆ จะได้ไม่ Error
        var yPos = 0 - (window.pageYOffset / 5);
        s.style.top = 50 + yPos + "%";
    }
}

// 2. 🌟 ตัวจุดชนวน: สั่งให้เบราว์เซอร์เรียกใช้ฟังก์ชันนี้ทุกครั้งที่มีการเลื่อนหน้าจอ 🌟
window.addEventListener("scroll", parallax);
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
           
            const activityArea = document.getElementById("eventGridContainer");

            if (activityArea) {
                
                activityArea.innerHTML = html;

                
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

                
                activityArea.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });
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

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("search-group");
    const autocompleteList = document.getElementById("complete");

    // Listen for every letter typed into the search bar
    searchInput.addEventListener("input", function () {
        let query = this.value;

        // If the box is empty, hide the dropdown
        if (!query) {
            autocompleteList.style.display = "none";
            return;
        }

        // Ask the C# Controller for matching names
        fetch(`/Dashboard/Suggestions?query=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                autocompleteList.innerHTML = ""; 

                if (data.length > 0) {
                    autocompleteList.style.display = "block"; // Show dropdown

                    // Create a row for each suggestion
                    data.forEach(suggestion => {
                        let item = document.createElement("div");
                        item.innerHTML = suggestion;

                        // What happens when you click a suggestion?
                        item.addEventListener("click", function () {
                            searchInput.value = suggestion; // Put text in search bar
                            autocompleteList.style.display = "none"; // Hide dropdown

                            // Optional: Automatically trigger the search right after clicking!
                            if (typeof fetchAjaxData === "function") {
                                fetchAjaxData();
                            } else {
                                document.getElementById("searchForm").submit();
                            }
                        });

                        autocompleteList.appendChild(item);
                    });
                } else {
                    autocompleteList.style.display = "none"; // Hide if no matches
                }
            })
            .catch(error => console.error("Error fetching suggestions:", error));
    });

    // Hide dropdown if the user clicks anywhere else on the page
    document.addEventListener("click", function (e) {
        if (e.target !== searchInput) {
            autocompleteList.style.display = "none";
        }
    });


});