#!/bin/bash

echo "=== Тест производительности веб-серверов ==="
echo

# Функция для тестирования сервера
test_server() {
    local server_name=$1
    local port=$2
    local url="http://localhost:$port/users"
    
    echo "Тестирование $server_name на порту $port..."
    
    # Тест получения всех пользователей
    echo "GET запросы:"
    time curl -s "$url" > /dev/null
    time curl -s "$url" > /dev/null
    time curl -s "$url" > /dev/null
    
    echo
    
    # Тест создания пользователя
    echo "POST запросы:"
    time curl -s -X POST "$url" \
        -H "Content-Type: application/json" \
        -d '{"name": "Benchmark User", "email": "bench@test.com", "age": 25}' > /dev/null
    
    time curl -s -X POST "$url" \
        -H "Content-Type: application/json" \
        -d '{"name": "Benchmark User 2", "email": "bench2@test.com", "age": 30}' > /dev/null
    
    echo
    echo "---"
    echo
}

# Проверяем, что серверы запущены
if ! curl -s http://localhost:8080/ > /dev/null; then
    echo "ОШИБКА: Go сервер не отвечает на порту 8080"
    exit 1
fi

if ! curl -s http://localhost:8081/ > /dev/null; then
    echo "ОШИБКА: Rust сервер не отвечает на порту 8081"
    exit 1
fi

# Запускаем тесты
test_server "Go сервер" 8080
test_server "Rust сервер" 8081

echo "=== Сравнение размеров бинарников ==="
echo "Go сервер: $(ls -lh ../go_server/go_server | awk '{print $5}')"
echo "Rust сервер: $(ls -lh target/release/rust-web-server | awk '{print $5}')"
