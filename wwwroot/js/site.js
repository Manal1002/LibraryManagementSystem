// Sarasavi Library Management Portal - Frontend Enhancements
document.addEventListener("DOMContentLoaded", () => {
    console.log("Sarasavi Library Management Portal UX Initialized.");

    // Auto-focus search input if present
    const searchInput = document.querySelector(".search-control");
    if (searchInput) {
        // Move cursor to the end of the text if prefilled
        const len = searchInput.value.length;
        searchInput.focus();
        searchInput.setSelectionRange(len, len);
    }

    // Dynamic warning confirmation on Reset buttons
    const resetButtons = document.querySelectorAll("button[type='reset']");
    resetButtons.forEach(btn => {
        btn.addEventListener("click", (e) => {
            const form = btn.closest("form");
            if (form) {
                // Let the native reset execute, but add a soft transient glow effect to inputs
                const inputs = form.querySelectorAll(".form-control");
                inputs.forEach(input => {
                    input.style.transition = "box-shadow 0.2s ease";
                    input.style.boxShadow = "0 0 10px rgba(168, 85, 247, 0.2)";
                    setTimeout(() => {
                        input.style.boxShadow = "";
                    }, 500);
                });
            }
        });
    });

    // Custom visual effects for cards on hover
    const cards = document.querySelectorAll(".book-card, .stat-card");
    cards.forEach(card => {
        card.addEventListener("mouseenter", () => {
            card.style.transition = "transform 0.3s cubic-bezier(0.4, 0, 0.2, 1), border-color 0.3s ease, box-shadow 0.3s ease";
        });
    });
});
