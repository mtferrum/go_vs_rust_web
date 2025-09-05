# Сравнение веб-серверов на Go и Rust

Этот проект содержит два идентичных веб-сервера, реализованных на Go и Rust, для демонстрации различий между языками программирования.

## Функциональность

Оба сервера предоставляют REST API для управления пользователями:

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

## Тестирование API

### Примеры запросов

#### Получить всех пользователей
```bash
curl http://localhost:8080/users  # Go сервер
curl http://localhost:8081/users  # Rust сервер
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

## Производительность

Для тестирования производительности можно использовать инструменты вроде `wrk` или `ab`:

```bash
# Установка wrk (macOS)
brew install wrk

# Тест Go сервера
wrk -t12 -c400 -d30s http://localhost:8080/users

# Тест Rust сервера
wrk -t12 -c400 -d30s http://localhost:8081/users
```
