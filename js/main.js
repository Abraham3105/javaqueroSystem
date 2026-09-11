// "Controlador/Vista": toma la data de js/products.js y la pinta en el DOM,
// además de manejar el scroll suave y el estado activo de la barra inferior.

function waLink(message) {
  return `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(message)}`;
}

function renderProductCard(product) {
  const notesHtml = product.notes
    .map(
      (note) =>
        `<span class="px-2 py-0.5 rounded-full bg-surface-container-highest text-on-surface-variant font-label-sm text-label-sm">${note}</span>`
    )
    .join("");

  return `
    <article class="flex flex-col rounded-2xl bg-surface-container overflow-hidden shadow-xl transition-all duration-300">
      <div class="relative w-full h-64 bg-surface-container-lowest overflow-hidden">
        <img alt="${product.imageAlt}" class="w-full h-full object-cover" src="${product.image}"/>
        <div class="absolute top-3 right-3 px-space-xs py-1 rounded-full bg-surface-container-highest/90 backdrop-blur-md">
          <span class="font-label-sm text-label-sm text-primary uppercase font-bold tracking-wider">SCA ${product.scaScore} Pts</span>
        </div>
        <div class="absolute bottom-3 left-3 px-space-xs py-1 rounded-full ${product.processBadge.classes}">
          <span class="font-label-sm text-label-sm font-bold uppercase tracking-wider">${product.processBadge.label}</span>
        </div>
      </div>
      <div class="p-space-md flex flex-col gap-space-sm">
        <div class="flex items-start justify-between gap-space-xs">
          <div>
            <h3 class="font-title-lg text-title-lg text-on-surface font-semibold">${product.name}</h3>
            <p class="font-body-sm text-body-sm text-on-surface-variant flex items-center gap-1 mt-0.5">
              <span class="material-symbols-outlined text-sm text-primary">location_on</span>
              ${product.origin}
            </p>
          </div>
          <div class="text-right">
            <span class="font-title-lg text-title-lg text-primary font-bold block">$${product.priceUsd}</span>
            <span class="font-label-sm text-label-sm text-outline uppercase">₡${product.priceCrc} CRC</span>
          </div>
        </div>
        <div class="p-space-xs rounded-xl bg-surface-container-low flex flex-col gap-1">
          <span class="font-label-sm text-label-sm text-primary uppercase font-semibold">Perfil Sensorial</span>
          <div class="flex flex-wrap gap-1.5">${notesHtml}</div>
        </div>
        <div class="flex items-center justify-between text-on-surface-variant font-body-sm text-body-sm pt-space-2xs">
          <span class="flex items-center gap-1.5">
            <span class="material-symbols-outlined text-base text-primary">inventory_2</span>
            ${product.presentation}
          </span>
          <span class="font-label-sm text-label-sm uppercase text-outline">Grano o Molido</span>
        </div>
        <a class="mt-space-xs w-full py-3 px-space-md rounded-xl bg-primary text-on-primary font-label-md text-label-md uppercase tracking-wider flex items-center justify-center gap-space-xs hover:brightness-105 active:scale-98 transition-all shadow-[0_0_16px_rgba(242,202,80,0.25)]" href="${waLink(
          product.whatsappMessage
        )}" rel="noopener noreferrer" target="_blank">
          <span class="material-symbols-outlined text-lg">chat</span>
          <span>Pedir por WhatsApp</span>
        </a>
      </div>
    </article>`;
}

function renderProducts() {
  const container = document.getElementById("products-list");
  if (!container) return;
  container.innerHTML = PRODUCTS.map(renderProductCard).join("");
}

function initGeneralWhatsappLinks() {
  document.querySelectorAll("[data-wa-message]").forEach((el) => {
    el.setAttribute("href", waLink(el.getAttribute("data-wa-message")));
  });
}

function initSmoothScroll() {
  document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
    anchor.addEventListener("click", function (e) {
      const targetId = this.getAttribute("href");
      if (targetId.length > 1) {
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
          e.preventDefault();
          targetElement.scrollIntoView({ behavior: "smooth" });
        }
      }
    });
  });
}

function initActiveNavOnScroll() {
  const navLinks = document.querySelectorAll("nav [data-path]");
  if (!navLinks.length) return;

  const sections = Array.from(navLinks)
    .map((link) => document.getElementById(link.dataset.path))
    .filter(Boolean);

  if (!sections.length) return;

  const sectionLabels = {
    inicio: "Inicio",
    cafes: "Cafés",
    nosotros: "Nosotros",
    contacto: "Contacto"
  };
  const headerSubtitle = document.getElementById("header-subtitle");

  const setActive = (id) => {
    navLinks.forEach((link) => {
      const isActive = link.dataset.path === id;
      link.classList.toggle("text-primary", isActive);
      link.classList.toggle("text-on-surface-variant", !isActive);
      if (isActive) {
        link.setAttribute("aria-current", "page");
      } else {
        link.removeAttribute("aria-current");
      }
    });
    if (headerSubtitle && sectionLabels[id]) {
      headerSubtitle.textContent = sectionLabels[id];
    }
  };

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          setActive(entry.target.id);
        }
      });
    },
    { rootMargin: "-45% 0px -45% 0px" }
  );

  sections.forEach((section) => observer.observe(section));
}

document.addEventListener("DOMContentLoaded", () => {
  renderProducts();
  initGeneralWhatsappLinks();
  initSmoothScroll();
  initActiveNavOnScroll();
});
