begin;
alter table public."JV_PRODUCTO_TB" enable row level security;
grant select on public."JV_PRODUCTO_TB" to anon;
drop policy if exists jv_catalogo_publico on public."JV_PRODUCTO_TB";
drop policy if exists jv_catalogo_solo_activos on public."JV_PRODUCTO_TB";
create policy jv_catalogo_publico on public."JV_PRODUCTO_TB"
    for select to anon using (activo);
create policy jv_catalogo_solo_activos on public."JV_PRODUCTO_TB"
    as restrictive for select to anon using (activo);
commit;
