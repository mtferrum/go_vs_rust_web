# Сравнение веб-серверов на Go, Rust, Forth и Prolog

Этот проект содержит четыре веб-сервера с REST API для управления пользователями, реализованных на Go, Rust, Forth и Prolog, для демонстрации различий между языками программирования.

## Функциональность

Все четыре сервера предоставляют REST API для управления пользователями:

- `GET /` - Главная страница с информацией об API
- `GET /users` - Получить всех пользователей
- `GET /users/{id}` - Получить пользователя по ID
- `POST /users` - Создать нового пользователя
- `PUT /users/{id}` - Обновить пользователя
- `DELETE /users/{id}` - Удалить пользователя

## Запуск серверов

### Go сервер (порт 8080)

```bash
cd go_server
go run main.go
```

### Rust сервер (порт 8081)

```bash
cd rust_server
cargo run
```

### Forth сервер (демо)

```bash
cd forth_server
gforth final_demo.fs
```

### Prolog сервер (порт 8083)

```bash
cd prolog_server
swipl final_server.pl
```

**Примечание**: Prolog сервер использует декларативный подход с базой знаний.

## Тестирование API

### Примеры запросов

#### Получить всех пользователей
```bash
curl http://localhost:8080/users  # Go сервер
curl http://localhost:8081/users  # Rust сервер
curl http://localhost:8083/users  # Prolog сервер
```

#### Создать нового пользователя
```bash
curl -X POST http://localhost:8080/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Тест Тестов", "email": "test@example.com", "age": 25}'

curl -X POST http://localhost:8081/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Тест Тестов", "email": "test@example.com", "age": 25}'
```

#### Получить пользователя по ID
```bash
curl http://localhost:8080/users/1  # Go сервер
curl http://localhost:8081/users/1  # Rust сервер
```

#### Обновить пользователя
```bash
curl -X PUT http://localhost:8080/users/1 \
  -H "Content-Type: application/json" \
  -d '{"name": "Обновленное Имя", "age": 30}'

curl -X PUT http://localhost:8081/users/1 \
  -H "Content-Type: application/json" \
  -d '{"name": "Обновленное Имя", "age": 30}'
```

#### Удалить пользователя
```bash
curl -X DELETE http://localhost:8080/users/1  # Go сервер
curl -X DELETE http://localhost:8081/users/1  # Rust сервер
```

## Структура проекта

```
go_vs_rust_web/
├── go_server/
│   ├── main.go
│   └── go.mod
├── rust_server/
│   ├── src/
│   │   └── main.rs
│   └── Cargo.toml
├── forth_server/
│   ├── final_demo.fs
│   └── README.md
├── prolog_server/
│   ├── final_server.pl
│   └── README.md
├── ANALYSIS.md
├── TESTING.md
└── README.md
```

## Ключевые различия

### Go
- Простота и читаемость кода
- Встроенная поддержка HTTP сервера
- Минимальные зависимости
- Быстрая компиляция
- Автоматическое управление памятью (GC)

### Rust
- Безопасность памяти без сборщика мусора
- Сильная система типов
- Высокая производительность
- Асинхронное программирование с tokio
- Более сложный синтаксис

### Forth
- Минималистичный стековый синтаксис
- Интерпретируемый язык
- Максимальный контроль над системой
- Ограниченная экосистема для веб-разработки

### Prolog
- Декларативный подход к программированию
- База знаний с фактами и правилами
- Унификация и автоматический поиск решений
- Ограниченная применимость для веб-разработки
- Сложная кривая обучения

## Производительность

### Результаты тестирования
- **Go сервер**: ~16ms время отклика, 7.8MB бинарник
- **Rust сервер**: ~16ms время отклика, 3.0MB бинарник
- **Forth сервер**: Демонстрационная версия (интерпретируемый)
- **Prolog сервер**: ~20ms время отклика (интерпретируемый)

### Тестирование производительности

Для тестирования производительности можно использовать инструменты вроде `wrk` или `ab`:

```bash
# Установка wrk (macOS)
brew install wrk

# Тест Go сервера
wrk -t12 -c400 -d30s http://localhost:8080/users

# Тест Rust сервера
wrk -t12 -c400 -d30s http://localhost:8081/users
```

## Документация

- [Детальный анализ](DETAILED_ANALYSIS.md) - Комплексный анализ языков программирования
- [Метрический анализ](METRICS_ANALYSIS.md) - Количественные показатели и метрики
- [Подробный анализ](ANALYSIS.md) - Сравнительный анализ всех трех языков
- [Результаты тестирования](TESTING.md) - Детальные результаты тестирования
- [Forth сервер](forth_server/README.md) - Документация по Forth реализации
- [Prolog сервер](prolog_server/README.md) - Документация по Prolog реализации
