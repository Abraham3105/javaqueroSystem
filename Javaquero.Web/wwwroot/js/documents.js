const product = document.getElementById("productId");
const grams = document.getElementById("grams");
const amount = document.getElementById("amount");

function loadPrice() {
  amount.value = product.selectedOptions[0]?.getAttribute("data-price-" + grams.value) ?? "";
}

product?.addEventListener("change", loadPrice);
grams?.addEventListener("change", loadPrice);

const template = document.getElementById("quote-line-template");
const lines = document.getElementById("quote-lines");
const add = document.getElementById("add-quote-line");
const total = document.getElementById("quote-total");
const currency = new Intl.NumberFormat("es-CR", { style: "currency", currency: "CRC" });

function updateTotal() {
  let cents = 0;
  for (const line of lines.children) {
    const price = Number(line.querySelector("select").selectedOptions[0]?.dataset.price ?? 0);
    const quantity = line.querySelector("input").valueAsNumber;
    if (Number.isInteger(quantity) && quantity >= 1 && quantity <= 999) {
      cents += Math.round(price * 100) * quantity;
    }
  }
  total.value = currency.format(cents / 100);
}

function refreshControls() {
  add.disabled = lines.children.length >= 50;
  for (const button of lines.querySelectorAll("[data-remove]")) {
    button.disabled = lines.children.length === 1;
  }
  updateTotal();
}

function appendLine() {
  lines.append(template.content.cloneNode(true));
  refreshControls();
}

if (template && lines && add && total) {
  add.addEventListener("click", () => {
    appendLine();
    lines.lastElementChild.querySelector("select").focus();
  });
  lines.addEventListener("input", updateTotal);
  lines.addEventListener("change", updateTotal);
  lines.addEventListener("click", (event) => {
    if (event.target.matches("[data-remove]") && lines.children.length > 1) {
      event.target.closest("fieldset").remove();
      refreshControls();
      add.focus();
    }
  });
  appendLine();
}
