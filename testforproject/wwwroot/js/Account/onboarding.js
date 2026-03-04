document.addEventListener("DOMContentLoaded", () => {
    const checkboxes = document.querySelectorAll(".cat-checkbox");
    const countDisplay = document.getElementById("selectionCount");
    const submitBtn = document.getElementById("submitBtn");
    const form = document.getElementById("onboardingForm");
    const errorMessage = document.getElementById("errorMessage");

    let selectedCategories = [];

    checkboxes.forEach(cb => {
        cb.addEventListener("change", function () {
            if (this.checked) {
                if (selectedCategories.length < 5) {
                    selectedCategories.push(parseInt(this.value));
                } else {
                    this.checked = false; // block selection if already 5
                }
            } else {
                selectedCategories = selectedCategories.filter(id => id !== parseInt(this.value));
            }

            countDisplay.innerText = `${selectedCategories.length} / 5 selected`;

            if (selectedCategories.length === 5) {
                submitBtn.disabled = false;
            } else {
                submitBtn.disabled = true;
            }
        });
    });

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        if (selectedCategories.length !== 5) {
            errorMessage.innerText = "Please select exactly 5 categories.";
            return;
        }

        submitBtn.disabled = true;
        submitBtn.innerText = "Saving...";

        try {
            const response = await fetch("/api/AccountApi/onboarding", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(selectedCategories)
            });

            if (!response.ok) {
                const result = await response.json();
                errorMessage.innerText = result.message || "Failed to save.";
                submitBtn.disabled = false;
                submitBtn.innerText = "Complete Setup";
                return;
            }

            // Go to home after onboarding
            window.location.href = "/";

        } catch (error) {
            errorMessage.innerText = "An unexpected error occurred.";
            submitBtn.disabled = false;
            submitBtn.innerText = "Complete Setup";
        }
    });
});
