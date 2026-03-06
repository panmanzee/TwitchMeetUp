const followBtn = document.getElementById('followBtn');

if (followBtn) {
    followBtn.addEventListener('click', async function () {
        const userId = this.dataset.userId;
        const isFollowing = this.dataset.following === 'true';
        const endpoint = isFollowing ? '/api/ProfileApi/Unfollow' : '/api/ProfileApi/Follow';

        try {
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ targetUserId: parseInt(userId) })
            });

            const data = await response.json();

            if (data.success) {

                this.textContent = data.isFollowing ? 'Unfollow' : 'Follow';
                this.dataset.following = data.isFollowing;
                this.classList.toggle('unfollow', data.isFollowing);
                document.getElementById('followerCount').textContent = data.followerCount;
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Something went wrong. Please try again.');
        }
    });
}

async function unjoinEvent(eventId, btnElement) {
    if (!confirm("Are you sure you want to unjoin this event?")) return;

    btnElement.disabled = true;
    btnElement.innerText = "Processing...";

    try {
        const response = await fetch('/api/EventApi/unjoin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ eventId: eventId })
        });

        if (response.ok) {
            // Success, remove the card from the UI
            const card = btnElement.closest('.event-card');
            if (card) {
                card.remove();
            }

            // Check if there are no more cards, show empty text
            const cardsContainer = document.querySelector('.section:nth-child(2) .cards');
            if (cardsContainer && cardsContainer.children.length === 0) {
                cardsContainer.innerHTML = '<p class="empty-text">No participated events yet.</p>';
            }
        } else {
            const data = await response.json();
            alert(data.message || 'Failed to unjoin event');
            btnElement.disabled = false;
            btnElement.innerText = "Unjoin";
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Something went wrong. Please try again.');
        btnElement.disabled = false;
        btnElement.innerText = "Unjoin";
    }
}
