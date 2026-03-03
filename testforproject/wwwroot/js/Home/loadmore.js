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
                // ⚠️ ข้อควรระวัง: เช็ค ID ของ Grid ให้ตรงกับใน Show.cshtml ของคุณด้วยนะครับ 
                // (ถ้าใน HTML เป็น id="eventGridContainer" ตรงนี้ก็ต้องแก้เป็น eventGridContainer ครับ)
                const grid = document.getElementById('eventGridContainer'); // หรือ 'eventGridContainer'

                data.forEach(item => {

                    // 🌟 1. เช็ครูปภาพ: ถ้ามีลิงก์รูปให้แสดงรูป ถ้าไม่มีให้แสดงข้อความ No Image
                    let imageHtml = item.imageUrl
                        ? `<img src="${item.imageUrl}" alt="${item.name}" style="width: 100%; height: 100%; object-fit: cover;" />`
                        : `<span style="color: #adadb8;">No Image</span>`;

                    // 🌟 2. เอา imageHtml ไปหยอดลงในการ์ด (ปรับ style กล่องรูปให้แสดงภาพได้พอดี)
                    const cardHtml = `
                  <div class="event-card" id="event-card-${item.eid}">
                      
                      <div class="card-img" style="padding: 0; overflow: hidden; height: 220px; display: flex; align-items: center; justify-content: center; background-color: #303032;">
                          ${imageHtml}
                      </div>

                      <div class="card-content" style="padding: 20px;">
                          <h4>${item.name}</h4>
                          <p>👤 Owner: ${item.owner}</p>
                          <p>📍 ${item.location}</p>

                          <div class="card-footer" style="margin-top: 15px; border-top: 1px solid #303032; padding-top: 10px;">
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