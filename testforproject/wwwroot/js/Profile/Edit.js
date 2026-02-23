const button = document.querySelector(".save");
const modal = document.getElementById("uploadModal");
const openBtn = document.querySelector("img");
const closeBtn = document.querySelector(".close");
const input = document.getElementById("image")
const form = document.getElementById("upload")

openBtn.addEventListener("click", () => {
    modal.style.display = "block";
});

closeBtn.addEventListener("click", () => {
    modal.style.display = "none";
});

window.addEventListener("click", (e) => {
    if (e.target === modal) {
        modal.style.display = "none";
    }
});

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
    modal.style.display = "none";
    openBtn.src = `/imageProfile/${uid}.jpg?t=`+ new Date().getTime()
});

button.addEventListener("click", () => {

    const Bio = document.querySelector("#Bio");
    const DisplayName = document.querySelector("#DisplayName");
    const Age = document.querySelector("#Age"); 
    const Gender = document.querySelector("#Gender");
    const ageError = document.querySelector("#ageError");
    

    const Data = {
        Bio: Bio.value,
        DisplayName: DisplayName.value,
        Age: Age.value,
        Gender: Gender.value
    };
    const ageValue = parseInt(Age.value);

    if (isNaN(ageValue) || ageValue < 1 || ageValue > 80) {
        ageError.textContent = "Please enter age between 1 and 80";
        ageError.style.display = "block";
        return; 
    }
    if (!Gender.value) {
        ageError.textContent = "Please select your gender";
        ageError.style.display = "block";
        return; 
    }


    fetch("/api/ProfileApi/Edit", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(Data)
    })
        .then(res => res.json())
        .then(result => {
            if (result.ok) {

            }
            else {
                if (result.errors.age !== null) {
                    console.log("plz in 1-80")
                }
                console.log(result.title)
            }
        })
        .catch(err => console.error(err));
});
