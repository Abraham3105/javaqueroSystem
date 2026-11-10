const WHATSAPP_NUMBER = "50685838921";

function waLink(message) {
  return `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(message)}`;
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
  initGeneralWhatsappLinks();
  initSmoothScroll();
  initActiveNavOnScroll();
});
