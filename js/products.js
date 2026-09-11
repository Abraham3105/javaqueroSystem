// "Model": catálogo de cafés de Javaquero.
// Toda la data que antes estaba escrita a mano en cada <article> del HTML
// vive ahora aquí, y js/main.js (el "controlador/vista") la lee para
// construir las tarjetas de producto dinámicamente.

const WHATSAPP_NUMBER = "50685838921";

const PRODUCTS = [
  {
    id: "geisha-honey-reserva",
    name: "Geisha Honey Reserva",
    origin: "Tarrazú • 1,900 msnm",
    scaScore: "89.5",
    processBadge: {
      label: "Honey Process",
      classes: "bg-primary text-on-primary"
    },
    priceUsd: "24.50",
    priceCrc: "12,500",
    notes: ["Jazmín", "Miel de Azahar", "Bergamota", "Melocotón"],
    presentation: "Bolsa hermética 340g",
    image: "img/products/geisha-honey-reserva.jpg",
    imageAlt: "JAVAQUERO Geisha Honey Reserva bolsa de café de especialidad",
    whatsappMessage: "Hola Javaquero, quiero pedir el café Geisha Honey Reserva (340g)"
  },
  {
    id: "bourbon-natural-huila",
    name: "Bourbon Natural Huila",
    origin: "Huila, Colombia • 1,800 msnm",
    scaScore: "87.5",
    processBadge: {
      label: "Natural 72h",
      classes: "bg-secondary-container text-on-secondary-container"
    },
    priceUsd: "18.90",
    priceCrc: "9,700",
    notes: ["Chocolate Oscuro", "Frutos Rojos", "Mora Azul", "Caramelo"],
    presentation: "Bolsa Craft sellada 340g",
    image: "img/products/bourbon-natural-huila.jpg",
    imageAlt: "JAVAQUERO Bourbon Natural bolsa artesanal y granos tostados",
    whatsappMessage: "Hola Javaquero, deseo ordenar el café Bourbon Natural Huila (340g)"
  },
  {
    id: "caturra-lavado-clasico",
    name: "Caturra Lavado Clásico",
    origin: "Nariño • 1,950 msnm",
    scaScore: "86.0",
    processBadge: {
      label: "Lavado Tradicional",
      classes: "bg-surface-container-high text-on-surface"
    },
    priceUsd: "16.50",
    priceCrc: "8,500",
    notes: ["Naranja Dulce", "Panela", "Avellana", "Final Sedoso"],
    presentation: "Bolsa craft con válvula 340g",
    image: "img/products/caturra-lavado-clasico.jpg",
    imageAlt: "JAVAQUERO Caturra Lavado Clásico en bolsa artesanal y cafetera chemex",
    whatsappMessage: "Hola Javaquero, quisiera adquirir el café Caturra Lavado Clásico (340g)"
  }
];
