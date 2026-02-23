let currentSkip = 4;

function loadMore() {
    const btn = document.querySelector('.btn-view-more');
    btn.innerText = "Loading...";
    btn.disabled = true;

    fetch(`/api/DashboardApi/LoadMoreEvents?skip=${currentSkip}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Server error");
            }
            return response.json();
        })
        .then(data => {
            if (data && data.length > 0) {
                // ตอนนี้มันจะหา id="eventGrid" เจอแล้ว
                const grid = document.getElementById('eventGrid');

                data.forEach(item => {
                    // ใช้โครงสร้าง HTML เดียวกับ Dashboard.cshtml เป๊ะๆ
                    const cardHtml = `
                        <div class="event-card" id="event-card-${item.eid}">
                            <div class="card-img">Image Placeholder</div>
                            <div class="card-content">
                                <h4>${item.name}</h4>
                                <p>👤 Owner: ${item.owner}</p>
                                <p>📍 ${item.location}</p>

                                <div class="card-footer">
                                    <span style="font-size: 13px;">🎯 Target: ${item.maxParticitpant}</span>
                                    
                                </div>
                            </div>
                        </div>
                    `;
                    grid.insertAdjacentHTML('beforeend', cardHtml);
                });

                currentSkip += data.length;
                btn.innerText = "View More";
                btn.disabled = false;

                // ถ้าโหลดมาแล้วได้น้อยกว่า 4 แสดงว่าหมดแล้ว ให้ซ่อนปุ่ม
                if (data.length < 4) {
                    document.getElementById('viewMoreContainer').style.display = 'none';
                }
            } else {
                document.getElementById('viewMoreContainer').style.display = 'none';
            }
        })
        .catch(error => {
            console.error('Error:', error);
            btn.innerText = "Try Again";
            btn.disabled = false;
        });
}