document.querySelectorAll('.password-toggle').forEach(button => {
    button.addEventListener('click', function (e) {
        e.preventDefault();
        const input = this.previousElementSibling;
        if (input.type === 'password') {
            input.type = 'text';
            this.innerHTML = '<i class="fa fa-eye-slash" aria-hidden="true"></i>';
        } else {
            input.type = 'password';
            this.innerHTML = '<i class="fa fa-eye" aria-hidden="true"></i>';
        }
    });
});

async function login() {
    const errorMessage = document.getElementById("errorMessage");
    const username = document.getElementById("username").value
    const password = document.getElementById("password").value

    if (!username || !password) {
        errorMessage.innerText = "All fields required"
        return;
    }

    const data = {username, password};

    try {
        const res = await fetch("/api/AccountApi/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify(data)
        });

        const result = await res.json();

        if (!res.ok) {
            errorMessage.innerText = result.message;
            return;
        }

        window.location.href = "/Dashboard/show";
    } catch (error) {
        errorMessage.innerText = "An unexpected error occured";
        console.error("Login error:", error);
    }
}