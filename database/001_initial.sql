begin;

create table public."JV_ROL_TB" (
    id bigint generated always as identity primary key,
    codigo text not null unique,
    nombre text not null
);

create table public."JV_USUARIO_TB" (
    id uuid primary key references auth.users(id) on delete cascade,
    rol_id bigint not null references public."JV_ROL_TB"(id),
    nombre text not null check (btrim(nombre) <> ''),
    activo boolean not null default true,
    creado_en timestamptz not null default now(),
    actualizado_en timestamptz not null default now()
);

create table public."JV_PRODUCTO_TB" (
    id bigint generated always as identity primary key,
    slug text not null unique check (slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'),
    nombre text not null check (btrim(nombre) <> ''),
    origen text not null,
    proceso text not null,
    puntaje_sca numeric(4,1) not null check (puntaje_sca between 0 and 100),
    notas text[] not null default '{}',
    presentacion text not null,
    precio_usd numeric(12,2) not null check (precio_usd >= 0),
    precio_crc numeric(12,2) not null check (precio_crc >= 0),
    imagen text not null,
    activo boolean not null default true
);

alter table public."JV_ROL_TB" enable row level security;
alter table public."JV_USUARIO_TB" enable row level security;
alter table public."JV_PRODUCTO_TB" enable row level security;

insert into public."JV_ROL_TB" (codigo, nombre) values ('ADMIN', 'Administrador');

insert into public."JV_PRODUCTO_TB"
    (slug, nombre, origen, proceso, puntaje_sca, notas, presentacion, precio_usd, precio_crc, imagen)
values
    ('geisha-honey-reserva', 'Geisha Honey Reserva', 'Tarrazú • 1,900 msnm', 'Honey Process', 89.5,
     array['Jazmín', 'Miel de Azahar', 'Bergamota', 'Melocotón'], 'Bolsa hermética 340g', 24.50, 12500,
     '/img/products/geisha-honey-reserva.jpg'),
    ('bourbon-natural-huila', 'Bourbon Natural Huila', 'Huila, Colombia • 1,800 msnm', 'Natural 72h', 87.5,
     array['Chocolate Oscuro', 'Frutos Rojos', 'Mora Azul', 'Caramelo'], 'Bolsa Craft sellada 340g', 18.90, 9700,
     '/img/products/bourbon-natural-huila.jpg'),
    ('caturra-lavado-clasico', 'Caturra Lavado Clásico', 'Nariño • 1,950 msnm', 'Lavado Tradicional', 86.0,
     array['Naranja Dulce', 'Panela', 'Avellana', 'Final Sedoso'], 'Bolsa craft con válvula 340g', 16.50, 8500,
     '/img/products/caturra-lavado-clasico.jpg');

commit;
