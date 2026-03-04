console.log("JS LOADED");
document.getElementById("registerForm")
    .addEventListener("submit", async function (e) {
        e.preventDefault();
        console.log("new regis")

        const username = document.getElementById("username").value
        const email = document.getElementById("email").value
        const password = document.getElementById("password").value
        const confirmPassword = document.getElementById("confirmPassword").value

        const errorMessage = document.getElementById("errorMessage");

        if (!username || !email || !password || !confirmPassword) {
            errorMessage.innerText = "All fields required"
            return;
        }
        else if (password !== confirmPassword) {
            errorMessage.innerText = "Password do not match";
            return;
        }
        const data = { username, email, password, confirmPassword };

        try {
            const response = await fetch("/api/AccountApi/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                console.log(response)
                errorMessage.innerText = result.message;
                return;
            }

            window.location.href = "/Account/Onboarding";
        } catch (error) {
            errorMessage.innerText = "An unexpected error occured";
            console.error("Login error:", error);
        }
    });
document.querySelectorAll('.password-toggle').forEach(button => {
    button.addEventListener('click', function () {
        console.log("Hello")
        const input = this.previousElementSibling;
        console.log(input)
        if (input.type === 'password') {
            console.log("Hello1")
            input.type = 'text';
            this.innerHTML = '<i class="fa fa-eye-slash" aria-hidden="true"></i>';
        } else {
            console.log("Hello2")
            input.type = 'password';
            this.innerHTML = '<i class="fa fa-eye" aria-hidden="true"></i>';
        }
    });
});
