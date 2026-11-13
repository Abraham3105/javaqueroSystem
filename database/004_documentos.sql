begin;

create table public."JV_PRODUCTO_PRECIO_TB" (
    producto_id bigint not null references public."JV_PRODUCTO_TB"(id) on delete cascade,
    gramos integer not null check (gramos in (250, 500, 1000)),
    precio_crc numeric(12,2) not null check (precio_crc > 0),
    primary key (producto_id, gramos)
);

create table public."JV_COTIZACION_TB" (
    id bigint generated always as identity primary key,
    solicitud_id uuid not null unique,
    cliente text not null check (char_length(btrim(cliente)) between 1 and 120),
    creado_en timestamptz not null default now(),
    lineas jsonb not null,
    total numeric(16,2) not null check (total > 0)
);

alter table public."JV_PRODUCTO_PRECIO_TB" enable row level security;
alter table public."JV_COTIZACION_TB" enable row level security;
revoke all on public."JV_PRODUCTO_PRECIO_TB", public."JV_COTIZACION_TB" from anon, authenticated;
grant select, insert, update on public."JV_PRODUCTO_PRECIO_TB" to service_role;
grant select, insert on public."JV_COTIZACION_TB" to service_role;
grant usage on sequence public."JV_COTIZACION_TB_id_seq" to service_role;

create function public.jv_crear_cotizacion(p_solicitud uuid, p_cliente text, p_lineas jsonb)
returns bigint
language plpgsql
set search_path = ''
as $$
declare
    cotizacion_id bigint;
    detalle jsonb;
    total_crc numeric;
    cantidad_lineas integer;
begin
    select id into cotizacion_id from public."JV_COTIZACION_TB" where solicitud_id = p_solicitud;
    if found then return cotizacion_id; end if;

    if jsonb_array_length(p_lineas) not between 1 and 50 then
        raise exception 'Cantidad de líneas inválida' using errcode = '22023';
    end if;

    select jsonb_agg(jsonb_build_object(
               'nombre', p.nombre, 'gramos', r.gramos, 'cantidad', r.cantidad,
               'precio_crc', v.precio_crc, 'subtotal', v.precio_crc * r.cantidad
           ) order by r.posicion),
           sum(v.precio_crc * r.cantidad), count(*)
    into detalle, total_crc, cantidad_lineas
    from jsonb_to_recordset(p_lineas)
        as r(producto_id bigint, gramos integer, cantidad integer, posicion integer)
    join public."JV_PRODUCTO_TB" p on p.id = r.producto_id and p.activo
    join public."JV_PRODUCTO_PRECIO_TB" v on v.producto_id = p.id and v.gramos = r.gramos
    where r.cantidad between 1 and 999;

    if cantidad_lineas <> jsonb_array_length(p_lineas) then
        raise exception 'Hay líneas sin precio vigente o con cantidad inválida' using errcode = '22023';
    end if;

    insert into public."JV_COTIZACION_TB" (solicitud_id, cliente, lineas, total)
    values (p_solicitud, btrim(p_cliente), detalle, total_crc)
    on conflict (solicitud_id) do nothing
    returning id into cotizacion_id;

    if cotizacion_id is null then
        select id into cotizacion_id from public."JV_COTIZACION_TB" where solicitud_id = p_solicitud;
    end if;
    return cotizacion_id;
end;
$$;

revoke all on function public.jv_crear_cotizacion(uuid,text,jsonb) from public, anon, authenticated;
grant execute on function public.jv_crear_cotizacion(uuid,text,jsonb) to service_role;

notify pgrst, 'reload schema';
commit;
