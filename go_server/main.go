package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"strconv"
	"time"
)

// User представляет пользователя в системе
type User struct {
	ID       int    `json:"id"`
	Name     string `json:"name"`
	Email    string `json:"email"`
	Age      int    `json:"age"`
	CreateAt string `json:"created_at"`
}

// Response представляет стандартный ответ API
type Response struct {
	Success bool        `json:"success"`
	Message string      `json:"message"`
	Data    interface{} `json:"data,omitempty"`
}

// In-memory хранилище пользователей
var users = []User{
	{ID: 1, Name: "Иван Иванов", Email: "ivan@example.com", Age: 25, CreateAt: "2024-01-01T10:00:00Z"},
	{ID: 2, Name: "Мария Петрова", Email: "maria@example.com", Age: 30, CreateAt: "2024-01-02T11:00:00Z"},
	{ID: 3, Name: "Алексей Сидоров", Email: "alex@example.com", Age: 28, CreateAt: "2024-01-03T12:00:00Z"},
}

var nextID = 4

// Middleware для логирования запросов
func loggingMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		next(w, r)
		log.Printf("%s %s %s", r.Method, r.URL.Path, time.Since(start))
	}
}

// Middleware для установки CORS заголовков
func corsMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")

		if r.Method == "OPTIONS" {
			w.WriteHeader(http.StatusOK)
			return
		}

		next(w, r)
	}
}

// Обработчик для получения всех пользователей
func getUsers(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	response := Response{
		Success: true,
		Message: "Пользователи успешно получены",
		Data:    users,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

// Обработчик для получения пользователя по ID
func getUserByID(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// Извлекаем ID из URL
	idStr := r.URL.Path[len("/users/"):]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		response := Response{
			Success: false,
			Message: "Неверный ID пользователя",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	// Ищем пользователя
	for _, user := range users {
		if user.ID == id {
			response := Response{
				Success: true,
				Message: "Пользователь найден",
				Data:    user,
			}
			w.Header().Set("Content-Type", "application/json")
			json.NewEncoder(w).Encode(response)
			return
		}
	}

	// Пользователь не найден
	response := Response{
		Success: false,
		Message: "Пользователь не найден",
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusNotFound)
	json.NewEncoder(w).Encode(response)
}

// Обработчик для создания нового пользователя
func createUser(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	var newUser User
	if err := json.NewDecoder(r.Body).Decode(&newUser); err != nil {
		response := Response{
			Success: false,
			Message: "Неверный формат JSON",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	// Проверяем обязательные поля
	if newUser.Name == "" || newUser.Email == "" {
		response := Response{
			Success: false,
			Message: "Имя и email обязательны",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	// Устанавливаем ID и время создания
	newUser.ID = nextID
	nextID++
	newUser.CreateAt = time.Now().Format(time.RFC3339)

	// Добавляем пользователя
	users = append(users, newUser)

	response := Response{
		Success: true,
		Message: "Пользователь успешно создан",
		Data:    newUser,
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(response)
}

// Обработчик для обновления пользователя
func updateUser(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPut {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// Извлекаем ID из URL
	idStr := r.URL.Path[len("/users/"):]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		response := Response{
			Success: false,
			Message: "Неверный ID пользователя",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	var updatedUser User
	if err := json.NewDecoder(r.Body).Decode(&updatedUser); err != nil {
		response := Response{
			Success: false,
			Message: "Неверный формат JSON",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	// Ищем и обновляем пользователя
	for i, user := range users {
		if user.ID == id {
			updatedUser.ID = id
			updatedUser.CreateAt = user.CreateAt // Сохраняем оригинальное время создания
			users[i] = updatedUser

			response := Response{
				Success: true,
				Message: "Пользователь успешно обновлен",
				Data:    updatedUser,
			}
			w.Header().Set("Content-Type", "application/json")
			json.NewEncoder(w).Encode(response)
			return
		}
	}

	// Пользователь не найден
	response := Response{
		Success: false,
		Message: "Пользователь не найден",
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusNotFound)
	json.NewEncoder(w).Encode(response)
}

// Обработчик для удаления пользователя
func deleteUser(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodDelete {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	// Извлекаем ID из URL
	idStr := r.URL.Path[len("/users/"):]
	id, err := strconv.Atoi(idStr)
	if err != nil {
		response := Response{
			Success: false,
			Message: "Неверный ID пользователя",
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(response)
		return
	}

	// Ищем и удаляем пользователя
	for i, user := range users {
		if user.ID == id {
			users = append(users[:i], users[i+1:]...)
			response := Response{
				Success: true,
				Message: "Пользователь успешно удален",
			}
			w.Header().Set("Content-Type", "application/json")
			json.NewEncoder(w).Encode(response)
			return
		}
	}

	// Пользователь не найден
	response := Response{
		Success: false,
		Message: "Пользователь не найден",
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusNotFound)
	json.NewEncoder(w).Encode(response)
}

// Обработчик для корневого пути
func homeHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	response := Response{
		Success: true,
		Message: "Добро пожаловать в API веб-сервера на Go!",
		Data: map[string]interface{}{
			"version": "1.0.0",
			"endpoints": []string{
				"GET / - Главная страница",
				"GET /users - Получить всех пользователей",
				"GET /users/{id} - Получить пользователя по ID",
				"POST /users - Создать нового пользователя",
				"PUT /users/{id} - Обновить пользователя",
				"DELETE /users/{id} - Удалить пользователя",
			},
		},
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func main() {
	// Настраиваем маршруты
	http.HandleFunc("/", corsMiddleware(loggingMiddleware(homeHandler)))
	http.HandleFunc("/users", corsMiddleware(loggingMiddleware(func(w http.ResponseWriter, r *http.Request) {
		switch r.Method {
		case http.MethodGet:
			getUsers(w, r)
		case http.MethodPost:
			createUser(w, r)
		default:
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		}
	})))
	http.HandleFunc("/users/", corsMiddleware(loggingMiddleware(func(w http.ResponseWriter, r *http.Request) {
		switch r.Method {
		case http.MethodGet:
			getUserByID(w, r)
		case http.MethodPut:
			updateUser(w, r)
		case http.MethodDelete:
			deleteUser(w, r)
		default:
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		}
	})))

	port := ":8080"
	fmt.Printf("🚀 Go веб-сервер запущен на порту %s\n", port)
	fmt.Printf("📖 API документация доступна по адресу: http://localhost%s\n", port)

	log.Fatal(http.ListenAndServe(port, nil))
}
