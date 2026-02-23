const input = document.getElementById("image");
const form = document.getElementById("upload");

form.addEventListener("submit", async function (event) {
    event.preventDefault(); 

    const file = input.files[0];
    if (!file) {
        alert("Please select an image");
        return;
    }

    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch("/api/UserApi/upload", {
        method: "POST",
        body: formData
    });

    const data = await response.json();
    console.log(data);
});
