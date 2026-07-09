(() => {
    const ready = () => {
        const table = document.getElementById("bench-table");
        if (!table) return;

        const rows = Array.from(table.querySelectorAll("tbody tr"));
        rows.forEach((row, index) => {
            if (index % 2 === 0) {
                row.classList.add("row-even");
            }
        });

        console.log("[sample] benchmark table initialized");
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", ready);
    } else {
        ready();
    }
})();
