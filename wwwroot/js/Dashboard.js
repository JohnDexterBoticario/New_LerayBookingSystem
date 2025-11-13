// wwwroot/js/dashboard.js

document.addEventListener("DOMContentLoaded", () => {
    const chartData = window.dashboardData || [];
    const ctx = document.getElementById("appointmentsChart")?.getContext("2d");

    if (!ctx) return; // stop if the canvas isn't on this page

    const labels = chartData.map(x => x.MonthLabel || x.Month);
    const counts = chartData.map(x => x.Count || 0);

    new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "Appointments",
                data: counts,
                backgroundColor: "rgba(54, 162, 235, 0.6)"
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
});
