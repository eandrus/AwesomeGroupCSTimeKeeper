// Close the current status message
function closeAlert() {
    $(".alert").slideUp();
}

window.setTimeout(closeAlert, 3000);

//Load them dang charts
var chartAreas = document.getElementsByClassName("chart-area");

for (var i = 0; i < chartAreas.length; i++) {
    var ctx = chartAreas[i].getContext("2d");
    new Chart(ctx, JSON.parse(chartAreas[i].id));
}

