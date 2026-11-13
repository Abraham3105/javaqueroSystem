const template = document.getElementById("quote-line-template");
const lines = document.getElementById("quote-lines");
const add = document.getElementById("add-quote-line");

function refreshControls() {
  add.disabled = lines.children.length >= 50;
  for (const button of lines.querySelectorAll("[data-remove]")) {
    button.disabled = lines.children.length === 1;
  }
}

function appendLine() {
  lines.append(template.content.cloneNode(true));
  refreshControls();
}

add.addEventListener("click", () => {
  appendLine();
  lines.lastElementChild.querySelector("select").focus();
});
lines.addEventListener("click", (event) => {
  if (event.target.matches("[data-remove]") && lines.children.length > 1) {
    event.target.closest("fieldset").remove();
    refreshControls();
    add.focus();
  }
});
appendLine();
