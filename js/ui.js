document.addEventListener("DOMContentLoaded", function () {
  const grid = document.getElementById("talents-grid");
  const pointsDisplay = document.getElementById("points");
  let points = 5;

  grid.querySelectorAll(".talent-slot").forEach(slot => {
    slot.addEventListener("click", () => {
      if (slot.classList.contains("selected")) {
        slot.classList.remove("selected");
        points++;
      } else {
        if (points > 0) {
          slot.classList.add("selected");
          points--;
        }
      }
      pointsDisplay.textContent = points;
    });
  });
});
