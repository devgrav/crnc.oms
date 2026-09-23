# Обновление стека мониторинга: Prometheus и Grafana

Тикет: [#18 Update Prometeus and Grafana](https://github.com/devgrav/crnc.oms/issues/18).
Ветка: `18-update-prometeus-and-grafana`.

## Контекст

Мониторинг в репозитории не трогали с момента появления: образы датированы 2020 и 2018
годами, тогда как метрики на стороне сервисов давно переехали на современный
`prometheus-net` 8.2.1 вместе с миграцией контекстов на .NET 10.

| Компонент | Сейчас | Дата релиза |
|---|---|---|
| `prom/prometheus` | `v2.16.0` | февраль 2020 |
| `grafana/grafana` | `5.4.3` | декабрь 2018 |
| `prometheus-net` | `8.2.1` | актуальная |

**Метрики переписывать не нужно.** Запросы дашборда (`http_requests_received_total`,
`http_request_duration_seconds_bucket`, `http_requests_in_progress`,
`dotnet_total_memory_bytes`, `dotnet_collection_count_total`, `process_num_threads`,
`process_open_handles`, `process_working_set_bytes`) — это имена, которые отдаёт
`prometheus-net` 8, плюс три кастомных счётчика репозитория
(`notification_email_sent_total`, `notification_push_sent_total`,
`prometheus_request_total`). Вся работа лежит в двух Docker-контекстах и в JSON дашборда.

### Согласованные решения

1. **Целевые версии — Prometheus `v3.5.5` (LTS) и Grafana `12.4.11`.** Самые свежие на
   момент написания — `v3.14.0` и `13.2.2`, но это демонстрационный стенд, который должен
   просто работать; LTS и предыдущий мажор дают меньше сюрпризов при том же объёме работ.
   Выбор версий не меняет ни одной фазы плана.
2. **Dockerfile'ы мониторинга удаляются, конфиги монтируются volume'ами.** Сейчас
   `prometheus.yml` и провижининг Grafana запекаются в образы через `ADD`, из-за чего
   после правки конфига обязателен `docker-compose build prometheus` — иначе таргеты
   остаются старыми. Эта ловушка отдельно описана в AGENTS.md. Раз оба Dockerfile всё
   равно переписываются, дешевле убрать их совсем: `image:` плюс `volumes:` в compose.
3. **`uid` дашборда остаётся `zyAf4i4Zz`.** На него ссылаются README.md и AGENTS.md
   (`http://localhost:3000/d/zyAf4i4Zz/prometheus-net`). Смена uid ломает документированные
   ссылки и ничего не даёт.
4. **Фазы — отдельными коммитами.** Prometheus и Grafana обновляются раздельно, чтобы
   поломка всегда атрибутировалась одному компоненту.
5. **Набор метрик не меняется.** См. «Что сознательно не делаем».

## Инвентаризация (подтверждено чтением файлов)

- `prometheus/Dockerfile` — `FROM prom/prometheus:v2.16.0` + `ADD prometheus.yml`.
- `prometheus/prometheus.yml` — шесть job'ов со статическими таргетами на `:8080`.
  Глобальной секции нет, интервал по умолчанию.
- `grafana/Dockerfile` — `FROM grafana/grafana:5.4.3` + три `ADD`.
- `grafana/config.ini` — `provisioning`, `enable_gzip`, `default_theme = light`,
  `admin_password = p@ssw0rd`.
- `grafana/provisioning/datasources/all.yml` — формат до v5: список верхнего уровня,
  `org_id`, `is_default`, без `apiVersion`.
- `grafana/provisioning/dashboards/all.yml` — тот же старый формат, ключ `options.folder`.
- `grafana/dashboards/defaultdashboard.json` — `schemaVersion: 16`, `uid: zyAf4i4Zz`,
  **9 панелей типа `graph`**, две переменные (`interval` — интервальная,
  `instances` — `label_values(http_requests_received_total, instance)`), алертов нет,
  `refresh: 5s`, ссылки на datasource по имени `"Prometheus"` и `null`.
- `docker-compose.yml` — `prometheus` собирается из `prometheus/`, без `container_name`
  (контейнер зовётся `crncoms-prometheus-1`); `grafana` собирается из `./grafana`,
  `container_name: crnc-oms-grafana`, пароль задан ещё и через
  `GF_SECURITY_ADMIN_PASSWORD`. Оба в профилях `monitoring`, `server`, `full`.

## Что именно сломается при обновлении

**1. Провижининг Grafana не будет прочитан.** Оба YAML написаны для Grafana 4/5.
Современный формат требует `apiVersion: 1`, camelCase и — для дашбордов — обёртку
`providers:` с `options.path` вместо `options.folder`.

**2. Все 9 панелей — Angular.** Тип `graph` признан устаревшим в Grafana 9, отключён по
умолчанию в 11 и удалён в 12. Без конвертации в `timeseries` дашборд откроется пустым.

**3. Ссылки на datasource.** `"datasource": "Prometheus"` (по имени) и `null` — наследие
старой схемы. Современная Grafana адресует источник по `uid`.

Конфиг Prometheus, наоборот, переезд переживает без единой правки: статические
`scrape_configs` валидны и в 3.x.

## Фаза 0: зафиксировать baseline

До любых правок поднять текущий стенд и сохранить:

```
docker-compose --profile monitoring up -d
curl -s http://localhost:9090/api/v1/targets | jq '[.data.activeTargets[] | {job: .labels.job, health}]'
```

- скриншот дашборда `http://localhost:3000/d/zyAf4i4Zz/prometheus-net` под нагрузкой
  (прокликать SPA, иначе половина панелей пуста законно);
- список таргетов и их `health`.

Без этого «после» не с чем сравнивать: панель может остаться пустой и по причине,
существовавшей до миграции.

**Статус: сделано.** На v2.16.0 все шесть таргетов `up`. Метрики дашборда присутствуют
(`http_requests_received_total`, `http_request_duration_seconds_bucket`,
`http_requests_in_progress`, `dotnet_total_memory_bytes`, `dotnet_collection_count_total`,
`process_num_threads`, `process_open_handles`, `process_working_set_bytes`), кроме двух:
`notification_push_sent_total` и `notification_email_sent_total` на свежем стенде дают
ноль серий — они появляются после первой отправки уведомления, и пустые панели в этих
двух местах ожидаемы. `prometheus_request_total` — одна серия, что согласуется с
неподключённым `MonitoringRequestMiddleware` в сервисах Notification.

Отдельно зафиксировать в заметках: панель **Total count of requests for routes**
(`prometheus_request_total`) показывает данные только по Security, Sales и Production.
`MonitoringRequestMiddleware` в трёх сервисах Notification существует, но нигде не
подключён — это известный долг, описанный в AGENTS.md, а не регрессия обновления.

## Фаза 1: Prometheus 2.16 → 3.5.5

1. Удалить `prometheus/Dockerfile`.
2. В `docker-compose.yml` заменить сборку на образ с монтированием конфига:

```yaml
  prometheus:
    image: prom/prometheus:v3.5.5
    container_name: 'crnc-oms-prometheus'
    restart: always
    profiles: ["monitoring", "server", "full"]
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml:ro
    ports:
      - "9090:9090"
```

`container_name` добавляется заодно — для симметрии с `crnc-oms-grafana` и предсказуемых
`docker logs`. Аргументы командной строки по умолчанию образа (`--config.file`,
`--storage.tsdb.path`) совпадают с тем, что нужно, поэтому `command:` не требуется.

3. `prometheus.yml` в части таргетов не трогать.

   Отдельным вопросом стоял `global.scrape_interval`: секции `global` в конфиге не было,
   и baseline на живом стенде показал фактический интервал **1 минута** (умолчание
   Prometheus), тогда как README и AGENTS.md обещают 5 секунд. **Решено в пользу
   конфига** — документация описывает намерение, дашборд обновляется раз в 5 секунд, и
   при сборе раз в минуту он показывает ступеньки вместо графика. Ловушка: дефолтный
   `scrape_timeout` равен 10s и стал бы больше нового интервала, поэтому задан явно.

   ```yaml
   global:
     scrape_interval: 5s
     scrape_timeout: 4s
   ```

   Нагрузка не имеет значения: 6 таргетов, у контейнера Prometheus нет тома, данные
   живут только до пересоздания.

**Проверка фазы:** `docker-compose --profile monitoring up -d`, затем все шесть таргетов
в состоянии `up` на `http://localhost:9090/targets`, и `/api/v1/query?query=up` отдаёт
шесть серий. Отдельный коммит.

**Статус: сделано.** На стенде `--profile server`: `buildinfo.version` = `3.5.5`, все
шесть таргетов `up` (тот же набор, что в baseline на v2.16.0), `level=error`/`level=warn`
в логах контейнера нет. Отдельно проверено ради цели перехода на volume: временный job,
дописанный в `prometheus.yml`, появился в `/api/v1/targets` после
`docker-compose restart prometheus` **без** пересборки образа; после проверки конфиг
возвращён в исходное состояние. Имя контейнера сменилось с `crncoms-prometheus-1` на
`crnc-oms-prometheus`. Интервал сбора приведён к обещанным 5 секундам: эффективный
конфиг отдаёт `scrape_interval: 5s` / `scrape_timeout: 4s`, а `count_over_time(up[1m])`
даёт 11–12 сэмплов на серию вместо одного.

## Фаза 2: провижининг Grafana в современный формат

Правится на **старом** образе — формат `apiVersion: 1` понимает и Grafana 5, поэтому шаг
проверяется до подъёма мажорной версии и не смешивается с ней.

`grafana/provisioning/datasources/all.yml`:

```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    uid: prometheus
    type: prometheus
    access: proxy
    orgId: 1
    url: http://prometheus:9090
    isDefault: true
    editable: true
```

Фиксированный `uid: prometheus` — обязательная часть: на него будут ссылаться панели
дашборда в следующей фазе.

`grafana/provisioning/dashboards/all.yml`:

```yaml
apiVersion: 1

providers:
  - name: default
    orgId: 1
    folder: ''
    type: file
    options:
      path: /var/lib/grafana/dashboards
```

**Проверка фазы:** стенд поднимается, источник данных и дашборд на месте — то есть
поведение не изменилось. Отдельный коммит.

**Статус: сделано.** На 5.4.3 новый формат принят: в логах
`inserting datasource from configuration name=Prometheus`, ошибок провижининга нет,
`/api/datasources` отдаёт источник, `/api/search` — дашборд с `uid: zyAf4i4Zz`, а запрос
`up` через `/api/datasources/proxy/1/api/v1/query` возвращает шесть серий, то есть
источник действительно рабочий.

**Важное наблюдение для фазы 3:** Grafana 5.4.3 поле `uid` у источника данных
**игнорирует** — `/api/datasources` показывает `uid: None`. Строка не ломает старую
версию, но и не действует в ней: поддержка uid в провижининге источников появилась
позже. Значит проверять, что `uid: prometheus` реально присвоен, нужно уже после
подъёма Grafana 12 — и только после этого переводить панели дашборда на ссылку по uid.
Если сделать наоборот, панели будут ссылаться на несуществующий uid.

## Фаза 3: Grafana 5.4.3 → 12.4.11 и конвертация дашборда

Самая содержательная часть.

1. Удалить `grafana/Dockerfile`, в compose перейти на образ с монтированием:

```yaml
  grafana:
    image: grafana/grafana:12.4.11
    container_name: 'crnc-oms-grafana'
    restart: always
    profiles: ["monitoring", "server", "full"]
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=p@ssw0rd
    volumes:
      - ./grafana/provisioning:/etc/grafana/provisioning:ro
      - ./grafana/dashboards:/var/lib/grafana/dashboards:ro
      - ./grafana/config.ini:/etc/grafana/grafana.ini:ro
    ports:
      - '3000:3000'
```

Точка монтирования конфига — `/etc/grafana/grafana.ini`, то есть штатный путь образа;
старый `ADD ./config.ini /etc/grafana/config.ini` работал только потому, что путь
совпадал с `paths.provisioning` по умолчанию.

2. Из `config.ini` убрать `[security] admin_password`: он дублирует
   `GF_SECURITY_ADMIN_PASSWORD` из compose, а переменная окружения всё равно выигрывает.
   Остальные секции (`paths`, `server.enable_gzip`, `users.default_theme`) валидны.

3. **Конвертация дашборда.** Практичный путь — не переписывать JSON руками:
   - поднять новую Grafana со старым `defaultdashboard.json`;
   - открыть дашборд: Grafana автоматически мигрирует `graph` → `timeseries` и поднимет
     `schemaVersion`;
   - выгрузить результат через Share → Export → Save to file и положить его на место
     старого файла.

   После выгрузки проверить руками три вещи:
   - `uid` остался `zyAf4i4Zz` (экспорт его сохраняет, но проверить обязательно);
   - ссылки на datasource заменены на `{"type": "prometheus", "uid": "prometheus"}`,
     включая переменную `instances`; экспорт может подставить `${DS_PROMETHEUS}` —
     эту подстановку надо развернуть обратно в фиксированный uid, иначе провижининг
     потребует ввода значения при импорте;
   - `"id"` дашборда — `null`, иначе провижининг конфликтует с локальной базой Grafana.

4. Панель **The duration of HTTP requests** (`sum(increase(http_request_duration_seconds_bucket[$interval])) by (le)`)
   после автомиграции станет `timeseries` с линией на каждый бакет — читается плохо.
   Осознанно перевести её в `heatmap` с форматом `Heatmap buckets`. Это единственная
   панель, которой недостаточно автоматической конвертации.

**Проверка фазы:** см. «Проверка» ниже. Отдельный коммит.

**Статус: сделано, но не тем способом, что планировался.** Автомиграция через UI не
подходит: Grafana 12 отдаёт дашборд ровно так, как он лежит на диске
(`/api/dashboards/uid/zyAf4i4Zz` вернул `schemaVersion: 16` и девять `graph`), потому что
миграция схемы живёт во фронтенде, а не на сервере. Вместо клика в UI написан
детерминированный конвертер: 8 панелей → `timeseries`, панель гистограммы → `heatmap`,
`schemaVersion: 41`, ссылки на источник заменены на `{"type":"prometheus","uid":"prometheus"}`
во всех панелях, таргетах и переменной `instances`. Так результат воспроизводим и
читается в ревью, а не приезжает выгрузкой из браузера.

Отдельная находка про провижининг: монтировать **весь** `/etc/grafana/provisioning`
нельзя. В образе есть пустые `alerting/`, `plugins/`, `notifiers/`, `access-control/`, и
подмена всего каталога их прячет — Grafana пишет `level=error` на каждый отсутствующий.
Поэтому монтируются только `datasources/` и `dashboards/` по отдельности.

Проверено на стенде: `uid: prometheus` у источника действительно присвоен (на 5.4.3 он
игнорировался), дашборд провижинится с `uid: zyAf4i4Zz`, запросы всех девяти панелей
успешно отрабатывают через прокси Grafana, в логах запуска ноль `level=error`. Визуально:
браузер открывает дашборд, рисуются все девять панелей (9 canvas), ноль ошибок консоли,
ни одного «Panel plugin not found». Панель `Notifications` наполнена тремя отправленными
через Gateway уведомлениями.

## Фаза 4: документация

- **AGENTS.md** — убрать предупреждение про `docker-compose build prometheus` после
  правки `prometheus.yml` (с volume'ами оно больше не нужно, достаточно
  `docker-compose restart prometheus`); поправить упоминание `prometheus/`, `grafana/`
  как «Docker build contexts» — теперь это просто каталоги конфигурации.
- **README.md** — версии там не указаны, менять нечего; проверить, что ссылки и
  обещание «сбор метрик каждые 5 секунд» соответствуют итоговому конфигу (см. фазу 1).

**Статус: сделано.** README не потребовал ни одной правки: версий он не называет, ссылка
на дашборд ведёт на сохранённый `uid`, логин/пароль прежние, а обещание про пять секунд
стало правдой в фазе 1 — до неё документация расходилась с конфигом. Версии намеренно не
дублируются в README: единственное место, где они объявлены, — `docker-compose.yml`.

В AGENTS.md, помимо правок по ходу фаз, добавлены три ловушки, на которые эта миграция
наступила: почему монтируются только два подкаталога провижининга, почему дашборд надо
чинить в файле (Grafana отдаёт его дословно, серверной миграции схемы нет) и что uid
источника и uid дашборда зафиксированы и на них ссылается документация.

## Проверка

1. `docker-compose --profile monitoring up -d` поднимает оба контейнера без ошибок в
   `docker logs crnc-oms-prometheus` и `docker logs crnc-oms-grafana`.
2. Шесть таргетов `up` на `http://localhost:9090/targets`.
3. `docker-compose --profile full up` — стенд целиком, прокликать SPA (создать заказ,
   сменить статус, дождаться push), чтобы метрики наполнились.
4. `http://localhost:3000/d/zyAf4i4Zz/prometheus-net` открывается по прямой ссылке,
   логин `admin/p@ssw0rd` работает.
5. Все 9 панелей рисуют данные. Ожидаемое исключение — `Total count of requests for
   routes` без сервисов Notification (см. фазу 0).
6. Правка `prometheus.yml` + `docker-compose restart prometheus` подхватывается без
   пересборки образа — то есть цель перехода на volume'ы достигнута.
7. CI не затронут: мониторинг не покрыт ни `backend-ci.yml`, ни `client-ci.yml`.

## Что сознательно не делаем

- **Не подключаем `MonitoringRequestMiddleware` в сервисах Notification.** Это меняет
  набор метрик и должно ехать отдельным тикетом, иначе в обновлении стека смешаются две
  несвязанные причины изменения графиков.
- **Не добавляем экспортеры инфраструктуры** (Mongo, PostgreSQL, RabbitMQ). README прямо
  оговаривает, что метрики инфраструктуры не собираются; это отдельная фича.
- **Не настраиваем алертинг.** В дашборде нет ни одного алерта, а unified alerting в
  Grafana 12 — самостоятельная тема.
- **Не добавляем persistent volume для Grafana.** Дашборд и источник данных приходят из
  провижининга, состояние между перезапусками не нужно.

## Риски

- **Автомиграция дашборда может потерять оформление** отдельных панелей (единицы
  измерения, пороги, легенды). Лечится сравнением со скриншотом из фазы 0 — ради этого
  он и снимается.
- **Экспорт из UI склонен подставлять `${DS_PROMETHEUS}`** вместо конкретного uid;
  провижининг такой дашборд не поднимет. Проверяется грепом по итоговому JSON.
- **Volume-монтирование конфигов меняет способ доставки настроек**: если кто-то запускает
  стенд не из корня репозитория, относительные пути в `volumes:` не разрешатся. Для
  текущего `docker-compose.yml`, который и так опирается на относительные пути сборки,
  это не новое ограничение.
