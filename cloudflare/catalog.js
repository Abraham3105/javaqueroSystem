const statusMessage = document.getElementById("catalog-status");
const catalog = document.getElementById("products-list");
const endpoint = new URL("https://nepqayipscvqbiwqcdoi.supabase.co/rest/v1/JV_PRODUCTO_TB");
endpoint.searchParams.set("select", "nombre,origen,proceso,puntaje_sca,notas,presentacion,precio_usd,precio_crc,imagen");
endpoint.searchParams.set("activo", "eq.true");
endpoint.searchParams.set("order", "nombre,id");

function element(tag, classes, text) {
  const node = document.createElement(tag);
  node.className = classes;
  if (text !== undefined) node.textContent = text;
  return node;
}

function productCard(product) {
  const card = element("article", "flex flex-col rounded-2xl bg-surface-container overflow-hidden shadow-xl");
  const figure = element("div", "relative h-64 bg-surface-container-lowest");
  const image = element("img", "w-full h-full object-cover");
  const imageUrl = new URL(product.imagen || "/img/hero-espresso.jpg", location.origin);
  image.src = imageUrl.protocol === "https:" || imageUrl.origin === location.origin
    ? imageUrl.href : "/img/hero-espresso.jpg";
  image.alt = product.nombre;
  image.loading = "lazy";
  figure.append(image,
    element("span", "absolute top-3 right-3 bg-surface-container-highest text-primary rounded-full px-3 py-1 text-sm", `SCA ${Number(product.puntaje_sca).toFixed(1)} Pts`),
    element("span", "absolute bottom-3 left-3 bg-primary text-on-primary rounded-full px-3 py-1 text-sm", product.proceso));

  const details = element("div", "p-space-md flex flex-1 flex-col gap-space-sm");
  const prices = `$${Number(product.precio_usd).toFixed(2)} · ₡${Number(product.precio_crc).toLocaleString("en-US")} CRC`;
  const notes = element("div", "flex flex-wrap gap-1.5");
  for (const note of product.notas) {
    notes.append(element("span", "rounded-full px-2 py-1 bg-surface-container-highest text-label-sm", note));
  }
  const order = element("a", "mt-auto rounded-xl bg-primary text-on-primary text-center font-bold py-3 px-4", "Pedir por WhatsApp");
  order.href = waLink(`Hola Javaquero, quiero pedir el café ${product.nombre} (${product.presentacion}).`);
  order.target = "_blank";
  order.rel = "noopener noreferrer";
  details.append(
    element("h3", "font-title-lg text-title-lg", product.nombre),
    element("p", "text-on-surface-variant text-body-sm", product.origen),
    element("p", "text-primary font-bold", prices),
    notes,
    element("p", "text-on-surface-variant text-body-sm", product.presentacion + " · Grano o molido"),
    order);
  card.append(figure, details);
  return card;
}

async function loadCatalog() {
  try {
    const response = await fetch(endpoint, {
      headers: { apikey: "sb_publishable_8sgzdN6EOjBw1ZSuwoXRaw_dG6yvIok" },
      cache: "no-store"
    });
    if (!response.ok) throw new Error("No se pudo consultar el catálogo.");
    const products = await response.json();
    catalog.replaceChildren(...products.map(productCard));
    statusMessage.textContent = products.length ? "" : "Por el momento no hay cafés disponibles.";
  } catch {
    statusMessage.textContent = "El catálogo no está disponible en este momento. Puedes consultarnos por WhatsApp.";
  }
}

loadCatalog();
