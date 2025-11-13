// Sample data for demonstration
const clientHistory = [
    { name: "John Doe", email: "john@example.com", service: "Facial", date: "2025-10-20", status: "Completed" },
    { name: "Jane Smith", email: "jane@example.com", service: "Massage", date: "2025-10-21", status: "Cancelled" },
    { name: "Alex Johnson", email: "alex@example.com", service: "Hair Treatment", date: "2025-10-22", status: "Completed" },
    { name: "Maria Garcia", email: "maria@example.com", service: "Manicure", date: "2025-10-23", status: "Completed" },
    // Add more sample data
];

const tableBody = document.querySelector("#historyTable tbody");
const searchInput = document.getElementById("searchInput");
const prevBtn = document.getElementById("prevBtn");
const nextBtn = document.getElementById("nextBtn");
const pageInfo = document.getElementById("pageInfo");

let currentPage = 1;
const rowsPerPage = 5;

// Render table function
function renderTable(page, data) {
    tableBody.innerHTML = "";
    const start = (page - 1) * rowsPerPage;
    const end = start + rowsPerPage;
    const pageData = data.slice(start, end);

    pageData.forEach((client, index) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${start + index + 1}</td>
            <td>${client.name}</td>
            <td>${client.email}</td>
            <td>${client.service}</td>
            <td>${client.date}</td>
            <td>${client.status}</td>
        `;
        tableBody.appendChild(row);
    });

    pageInfo.textContent = `Page ${currentPage} of ${Math.ceil(data.length / rowsPerPage)}`;
    prevBtn.disabled = currentPage === 1;
    nextBtn.disabled = currentPage === Math.ceil(data.length / rowsPerPage);
}

// Search
searchInput.addEventListener("input", () => {
    currentPage = 1;
    renderTable(currentPage, getFilteredData());
});

// Pagination
prevBtn.addEventListener("click", () => {
    currentPage--;
    renderTable(currentPage, getFilteredData());
});

nextBtn.addEventListener("click", () => {
    currentPage++;
    renderTable(currentPage, getFilteredData());
});

function getFilteredData() {
    const searchTerm = searchInput.value.toLowerCase();
    return clientHistory.filter(client =>
        client.name.toLowerCase().includes(searchTerm) ||
        client.email.toLowerCase().includes(searchTerm) ||
        client.service.toLowerCase().includes(searchTerm)
    );
}

// Initial render
renderTable(currentPage, clientHistory);
