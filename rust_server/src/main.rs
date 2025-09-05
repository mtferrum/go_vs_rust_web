use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use warp::http::StatusCode;
use warp::reply::json;
use warp::Filter;

// User представляет пользователя в системе
#[derive(Debug, Clone, Serialize, Deserialize)]
struct User {
    id: u32,
    name: String,
    email: String,
    age: u32,
    created_at: String,
}

// Response представляет стандартный ответ API
#[derive(Debug, Serialize, Deserialize)]
struct Response<T> {
    success: bool,
    message: String,
    data: Option<T>,
}

// In-memory хранилище пользователей
type Users = Arc<RwLock<HashMap<u32, User>>>;

// Создаем начальные данные пользователей
async fn create_initial_users() -> Users {
    let users = Arc::new(RwLock::new(HashMap::new()));
    {
        let mut users_map = users.write().await;
        
        users_map.insert(1, User {
            id: 1,
            name: "Иван Иванов".to_string(),
            email: "ivan@example.com".to_string(),
            age: 25,
            created_at: "2024-01-01T10:00:00Z".to_string(),
        });
        
        users_map.insert(2, User {
            id: 2,
            name: "Мария Петрова".to_string(),
            email: "maria@example.com".to_string(),
            age: 30,
            created_at: "2024-01-02T11:00:00Z".to_string(),
        });
        
        users_map.insert(3, User {
            id: 3,
            name: "Алексей Сидоров".to_string(),
            email: "alex@example.com".to_string(),
            age: 28,
            created_at: "2024-01-03T12:00:00Z".to_string(),
        });
    }
    
    users
}

// Middleware для CORS
fn cors() -> warp::cors::Builder {
    warp::cors()
        .allow_any_origin()
        .allow_headers(vec!["content-type", "authorization"])
        .allow_methods(vec!["GET", "POST", "PUT", "DELETE", "OPTIONS"])
}

// Обработчик для получения всех пользователей
async fn get_users(users: Users) -> Result<impl warp::Reply, warp::Rejection> {
    let users_map = users.read().await;
    let users_list: Vec<User> = users_map.values().cloned().collect();
    
    let response = Response {
        success: true,
        message: "Пользователи успешно получены".to_string(),
        data: Some(users_list),
    };
    
    Ok(warp::reply::with_status(json(&response), StatusCode::OK))
}

// Обработчик для получения пользователя по ID
async fn get_user_by_id(id: u32, users: Users) -> Result<impl warp::Reply, warp::Rejection> {
    let users_map = users.read().await;
    
    match users_map.get(&id) {
        Some(user) => {
            let response = Response {
                success: true,
                message: "Пользователь найден".to_string(),
                data: Some(user.clone()),
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::OK))
        }
        None => {
            let response = Response::<User> {
                success: false,
                message: "Пользователь не найден".to_string(),
                data: None,
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::NOT_FOUND))
        }
    }
}

// Структура для создания пользователя (без ID и created_at)
#[derive(Debug, Deserialize)]
struct CreateUserRequest {
    name: String,
    email: String,
    age: u32,
}

// Обработчик для создания нового пользователя
async fn create_user(
    create_req: CreateUserRequest,
    users: Users,
) -> Result<impl warp::Reply, warp::Rejection> {
    // Проверяем обязательные поля
    if create_req.name.is_empty() || create_req.email.is_empty() {
        let response = Response::<User> {
            success: false,
            message: "Имя и email обязательны".to_string(),
            data: None,
        };
        return Ok(warp::reply::with_status(json(&response), StatusCode::BAD_REQUEST));
    }
    
    let mut users_map = users.write().await;
    
    // Находим следующий доступный ID
    let next_id = users_map.keys().max().unwrap_or(&0) + 1;
    
    let new_user = User {
        id: next_id,
        name: create_req.name,
        email: create_req.email,
        age: create_req.age,
        created_at: chrono::Utc::now().to_rfc3339(),
    };
    
    users_map.insert(next_id, new_user.clone());
    
    let response = Response {
        success: true,
        message: "Пользователь успешно создан".to_string(),
        data: Some(new_user),
    };
    
    Ok(warp::reply::with_status(json(&response), StatusCode::CREATED))
}

// Структура для обновления пользователя
#[derive(Debug, Deserialize)]
struct UpdateUserRequest {
    name: Option<String>,
    email: Option<String>,
    age: Option<u32>,
}

// Обработчик для обновления пользователя
async fn update_user(
    id: u32,
    update_req: UpdateUserRequest,
    users: Users,
) -> Result<impl warp::Reply, warp::Rejection> {
    let mut users_map = users.write().await;
    
    match users_map.get_mut(&id) {
        Some(user) => {
            // Обновляем только переданные поля
            if let Some(name) = update_req.name {
                user.name = name;
            }
            if let Some(email) = update_req.email {
                user.email = email;
            }
            if let Some(age) = update_req.age {
                user.age = age;
            }
            
            let response = Response {
                success: true,
                message: "Пользователь успешно обновлен".to_string(),
                data: Some(user.clone()),
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::OK))
        }
        None => {
            let response = Response::<User> {
                success: false,
                message: "Пользователь не найден".to_string(),
                data: None,
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::NOT_FOUND))
        }
    }
}

// Обработчик для удаления пользователя
async fn delete_user(id: u32, users: Users) -> Result<impl warp::Reply, warp::Rejection> {
    let mut users_map = users.write().await;
    
    match users_map.remove(&id) {
        Some(_) => {
            let response = Response::<User> {
                success: true,
                message: "Пользователь успешно удален".to_string(),
                data: None,
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::OK))
        }
        None => {
            let response = Response::<User> {
                success: false,
                message: "Пользователь не найден".to_string(),
                data: None,
            };
            Ok(warp::reply::with_status(json(&response), StatusCode::NOT_FOUND))
        }
    }
}

// Обработчик для корневого пути
async fn home_handler() -> Result<impl warp::Reply, warp::Rejection> {
    let endpoints = vec![
        "GET / - Главная страница",
        "GET /users - Получить всех пользователей",
        "GET /users/{id} - Получить пользователя по ID",
        "POST /users - Создать нового пользователя",
        "PUT /users/{id} - Обновить пользователя",
        "DELETE /users/{id} - Удалить пользователя",
    ];
    
    let home_data = serde_json::json!({
        "version": "1.0.0",
        "endpoints": endpoints
    });
    
    let response = Response {
        success: true,
        message: "Добро пожаловать в API веб-сервера на Rust!".to_string(),
        data: Some(home_data),
    };
    
    Ok(warp::reply::with_status(json(&response), StatusCode::OK))
}

#[tokio::main]
async fn main() {
    // Создаем хранилище пользователей
    let users = create_initial_users().await;
    
    // Создаем фильтр для передачи users в обработчики
    let users_filter = warp::any().map(move || users.clone());
    
    // Определяем маршруты
    let home = warp::path::end()
        .and(warp::get())
        .and_then(home_handler);
    
    let get_users_route = warp::path("users")
        .and(warp::path::end())
        .and(warp::get())
        .and(users_filter.clone())
        .and_then(get_users);
    
    let get_user_by_id_route = warp::path("users")
        .and(warp::path::param::<u32>())
        .and(warp::get())
        .and(users_filter.clone())
        .and_then(get_user_by_id);
    
    let create_user_route = warp::path("users")
        .and(warp::path::end())
        .and(warp::post())
        .and(warp::body::json())
        .and(users_filter.clone())
        .and_then(create_user);
    
    let update_user_route = warp::path("users")
        .and(warp::path::param::<u32>())
        .and(warp::put())
        .and(warp::body::json())
        .and(users_filter.clone())
        .and_then(update_user);
    
    let delete_user_route = warp::path("users")
        .and(warp::path::param::<u32>())
        .and(warp::delete())
        .and(users_filter.clone())
        .and_then(delete_user);
    
    // Объединяем все маршруты
    let routes = home
        .or(get_users_route)
        .or(get_user_by_id_route)
        .or(create_user_route)
        .or(update_user_route)
        .or(delete_user_route)
        .with(cors());
    
    let port = 8081;
    println!("🚀 Rust веб-сервер запущен на порту {}", port);
    println!("📖 API документация доступна по адресу: http://localhost:{}", port);
    
    warp::serve(routes)
        .run(([127, 0, 0, 1], port))
        .await;
}
