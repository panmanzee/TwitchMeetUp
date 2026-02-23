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
