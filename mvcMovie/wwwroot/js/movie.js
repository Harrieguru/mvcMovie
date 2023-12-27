// Sample data (you can fetch this from your server instead)
const moviesData = [
    {
        title: "Avatar",
        genre: "Sci-Fi",
        rating: 7.6,
        description: "A sequel to the visually...",
        imageLink: "https://s3.amazonaws.com/m0viefiles/avatar_img.jpg",
        // ... other attributes
    },
    // ... other movies
];

// Dynamically generate movie cards
moviesData.forEach(movie => {
    // Create movie card elements and append to movie-list-container
    // You can use createElement and appendChild methods or any frontend framework/library you're using
});

// Handle filtering
document.getElementById("movieFilter").addEventListener("change", function () {
    const selectedFilter = this.value;
    // Hide or show movie cards based on the selected filter
    // For example, if filtering by genre, you can check the data-genre attribute
});
