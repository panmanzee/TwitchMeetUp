let currentSkip = 8;

async function logout() {
    await fetch('/api/AccountApi/logout', {
        method: 'POST',
        credentials: 'include'
    });

    location.reload();
}

function loadMore() {
    const btn = document.querySelector('.btn-view-more');
    btn.innerText = "Loading...";
    btn.disabled = true;

    // ดึงค่า filter จาก form ที่มีอยู่ (ถ้ามี)
    const searchQuery = document.getElementById('search-group') ? document.getElementById('search-group').value : '';
    const categoryFilter = document.getElementById('categoryInput') ? document.getElementById('categoryInput').value : '';

    // ส่ง request ไปหา Controller ใหม่ที่คืนค่าเป็น HTML (PartialView)
    // แล้วแนบค่า filter ต่างๆ ไปด้วยเพื่อให้ LoadMore ได้ข้อมูลที่ตรงหมวดหมู่
    const url = `/Dashboard/LoadMoreEvents?skip=${currentSkip}&searchQuery=${encodeURIComponent(searchQuery)}&categoryFilter=${encodeURIComponent(categoryFilter)}`;

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error("Server error");
            }
            // รับค่ากลับมาเป็นข้อความ HTML แทน JSON
            return response.text();
        })
        .then(html => {
            // ถ้ามี html ส่งกลับมา (ไม่ใช่ string ว่าง)
            if (html && html.trim().length > 0) {
                const grid = document.getElementById('eventGridContainer');

                // นำ html ไปต่อท้าย grid เดิม
                grid.insertAdjacentHTML('beforeend', html);

                // สมมติว่าคืนค่ามา 4 รายการเสมอ (ถ้าอยากเช็คเป๊ะๆ อาจจะต้องนับจำนวน card ใน html ที่ส่งมา)
                // การหาจำนวน element ใน string HTML ที่เพิ่งเอามาแปะ อาจทำโคลนมานับคร่าวๆ ได้
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const loadedCount = doc.querySelectorAll('.event-card').length;

                currentSkip += loadedCount;
                btn.innerText = "View More";
                btn.disabled = false;

                // ถ้าโหลดมาแล้วได้น้อยกว่า 4 แสดงว่าหมดแล้ว ให้ซ่อนปุ่ม
                if (loadedCount < 8) {
                    document.getElementById('viewMore').style.display = 'none';
                }
            } else {
                // ถ้าข้อมูลหมดแล้ว
                document.getElementById('viewMore').style.display = 'none';
                btn.innerText = "View More";
                btn.disabled = false;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            btn.innerText = "Try Again";
            btn.disabled = false;
        });
}