begin;

grant select on public."JV_USUARIO_TB", public."JV_ROL_TB" to service_role;

create function public.jv_guardar_usuario(
    p_id uuid, p_nombre text, p_activo boolean, p_nuevo boolean
) returns boolean
language plpgsql
set search_path = ''
as $$
begin
    perform pg_catalog.pg_advisory_xact_lock(972614);
    if p_nuevo then
        insert into public."JV_USUARIO_TB" (id, nombre, rol_id)
        select p_id, btrim(p_nombre), id from public."JV_ROL_TB" where codigo = 'ADMIN';
        return found;
    end if;

    update public."JV_USUARIO_TB" u
    set nombre = btrim(p_nombre), activo = p_activo, actualizado_en = now()
    from public."JV_ROL_TB" r
    where u.id = p_id and r.id = u.rol_id and r.codigo = 'ADMIN'
      and (p_activo or not u.activo or exists (
          select 1 from public."JV_USUARIO_TB" other
          where other.id <> u.id and other.rol_id = u.rol_id and other.activo
      ));
    return found;
end;
$$;

grant insert, update on public."JV_USUARIO_TB" to service_role;
revoke all on function public.jv_guardar_usuario(uuid,text,boolean,boolean)
from public, anon, authenticated;
grant execute on function public.jv_guardar_usuario(uuid,text,boolean,boolean) to service_role;

commit;
