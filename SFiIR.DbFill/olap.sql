drop table public.olap;

create table public.olap
(
    id                     bigserial
        primary key,
    arrival_lambda         double precision   not null,
    mem_lambda             double precision   not null,
    cpu_lambda             integer            not null,
    arrival_time_limit     integer            not null,
    test_time              integer            not null,
    max_mem                integer            not null,
    cpu_quant              integer            not null,
    seed                   integer default 43 not null,
    total_requests         integer            not null,
    completed_requests     integer            not null,
    rejected_requests      integer            not null,
    completed_ratio        double precision   not null,
    system_utilization     double precision   not null,
    avg_mem_usage          double precision   not null,
    avg_completion_ratio   double precision   not null,
    avg_request_queue_time double precision   not null,
    avg_process_queue_time double precision   not null
);

alter table public.olap
    owner to postgres;

create index idx_cache_arrival
    on public.olap (arrival_lambda);

create index idx_cache_mem
    on public.olap (mem_lambda);

create index idx_cache_cpu
    on public.olap (cpu_lambda);

create index idx_cache_maxmem
    on public.olap (max_mem);

create index idx_cache_quant
    on public.olap (cpu_quant);

create index idx_cache_testtime
    on public.olap (test_time);

create index idx_cache_params_composite
    on public.olap (arrival_lambda, mem_lambda, cpu_lambda, max_mem, cpu_quant);

SELECT
    relname AS table_name,
    (SELECT count(*) FROM olap) AS количество_строк,
    pg_size_pretty(pg_relation_size(oid)) AS размер_данных,
    pg_size_pretty(pg_total_relation_size(oid)) AS размер_с_индексами
FROM pg_class
WHERE relname = 'olap';