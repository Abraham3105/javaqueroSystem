begin;
grant select, insert, update, delete on public."JV_PRODUCTO_TB" to service_role;
grant usage on sequence public."JV_PRODUCTO_TB_id_seq" to service_role;
commit;
