
function filterMovies() {
    var filterType = document.getElementById("movieFilter").value;
    var filterValueInput = document.getElementById("filterValue");

    if (filterType == "rating") {
        filterValueInput.placeholder = "Enter minimum rating";
    } else if (filterType == "genre") {
        filterValueInput.placeholder = "Enter genre";
    }
}

function applyFilter() {
    var filterType = document.getElementById("movieFilter").value;
    var filterValue = document.getElementById("filterValue").value;

    if (filterType == "rating") {
        window.location.href = `/Movie/MovieList?minRating=${filterValue}`;
    } else if (filterType == "genre") {
        window.location.href = `/Movie/MovieList?genre=${filterValue}`;
    }
}

