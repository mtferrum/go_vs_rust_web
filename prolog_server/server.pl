% Веб-сервер на Prolog
% Демонстрационная реализация HTTP сервера для сравнения с Go, Rust и Forth

:- use_module(library(http/thread_httpd)).
:- use_module(library(http/http_dispatch)).
:- use_module(library(http/http_parameters)).
:- use_module(library(http/http_json)).
:- use_module(library(http/json)).
:- use_module(library(http/cors)).

% Константы
server_port(8083).

% База данных пользователей
user(1, "Иван Иванов", "ivan@example.com", 25, "2024-01-01T10:00:00Z").
user(2, "Мария Петрова", "maria@example.com", 30, "2024-01-02T11:00:00Z").
user(3, "Алексей Сидоров", "alex@example.com", 28, "2024-01-03T12:00:00Z").

% Получить всех пользователей
get_all_users(Users) :-
    findall(
        json([id=Id, name=Name, email=Email, age=Age, created_at=CreatedAt]),
        user(Id, Name, Email, Age, CreatedAt),
        Users
    ).

% Получить пользователя по ID
get_user_by_id(Id, User) :-
    user(Id, Name, Email, Age, CreatedAt),
    User = json([id=Id, name=Name, email=Email, age=Age, created_at=CreatedAt]).

% Проверить существование пользователя
user_exists(Id) :-
    user(Id, _, _, _, _).

% Создать JSON ответ
create_response(Success, Message, Data, Response) :-
    Response = json([
        success=Success,
        message=Message,
        data=Data
    ]).

% Обработчик главной страницы
handle_root(_Request) :-
    Endpoints = [
        "GET / - Главная страница",
        "GET /users - Получить всех пользователей",
        "GET /users/{id} - Получить пользователя по ID",
        "POST /users - Создать нового пользователя",
        "PUT /users/{id} - Обновить пользователя",
        "DELETE /users/{id} - Удалить пользователя"
    ],
    HomeData = json([
        version="1.0.0",
        endpoints=Endpoints
    ]),
    create_response(true, "Добро пожаловать в API веб-сервера на Prolog!", HomeData, Response),
    cors_enable,
    reply_json(Response).

% Обработчик получения всех пользователей
handle_users(_Request) :-
    get_all_users(Users),
    create_response(true, "Пользователи успешно получены", Users, Response),
    cors_enable,
    reply_json(Response).

% Обработчик получения пользователя по ID
handle_user_id(Request) :-
    http_parameters(Request, [id(Id, [integer])]),
    (   user_exists(Id)
    ->  get_user_by_id(Id, User),
        create_response(true, "Пользователь найден", User, Response)
    ;   create_response(false, "Пользователь не найден", null, Response)
    ),
    cors_enable,
    reply_json(Response).

% Обработчик создания пользователя
handle_create_user(Request) :-
    http_read_json(Request, Json),
    Json = json([name=Name, email=Email, age=Age]),
    % В реальном приложении здесь была бы логика сохранения
    NewUser = json([id=4, name=Name, email=Email, age=Age, created_at="2024-01-04T10:00:00Z"]),
    create_response(true, "Пользователь успешно создан", NewUser, Response),
    cors_enable,
    reply_json(Response).

% Обработчик обновления пользователя
handle_update_user(Request) :-
    http_parameters(Request, [id(Id, [integer])]),
    http_read_json(Request, Json),
    (   user_exists(Id)
    ->  % В реальном приложении здесь была бы логика обновления
        get_user_by_id(Id, User),
        create_response(true, "Пользователь успешно обновлен", User, Response)
    ;   create_response(false, "Пользователь не найден", null, Response)
    ),
    cors_enable,
    reply_json(Response).

% Обработчик удаления пользователя
handle_delete_user(Request) :-
    http_parameters(Request, [id(Id, [integer])]),
    (   user_exists(Id)
    ->  % В реальном приложении здесь была бы логика удаления
        create_response(true, "Пользователь успешно удален", null, Response)
    ;   create_response(false, "Пользователь не найден", null, Response)
    ),
    cors_enable,
    reply_json(Response).

% Обработчик OPTIONS запросов для CORS
handle_options(_Request) :-
    cors_enable,
    format('Access-Control-Allow-Origin: *~n'),
    format('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS~n'),
    format('Access-Control-Allow-Headers: Content-Type, Authorization~n'),
    format('~n').

% Регистрация маршрутов
:- http_handler(root(.), handle_root, []).
:- http_handler(root(users), handle_users, [methods([get])]).
:- http_handler(root(users/), handle_user_id, [methods([get])]).
:- http_handler(root(users), handle_create_user, [methods([post])]).
:- http_handler(root(users/), handle_update_user, [methods([put])]).
:- http_handler(root(users/), handle_delete_user, [methods([delete])]).
:- http_handler(root(.), handle_options, [methods([options])]).

% Запуск сервера
start_server :-
    server_port(Port),
    format('🚀 Prolog веб-сервер запущен на порту ~w~n', [Port]),
    format('📖 API документация доступна по адресу: http://localhost:~w~n', [Port]),
    http_server(http_dispatch, [port(Port)]).

% Демонстрационная функция
demo_server :-
    format('🚀 Prolog веб-сервер (демо версия)~n'),
    format('📖 Доступные эндпоинты:~n'),
    format('GET / - Главная страница~n'),
    format('GET /users - Получить всех пользователей~n'),
    format('GET /users/{id} - Получить пользователя по ID~n'),
    format('POST /users - Создать нового пользователя~n'),
    format('PUT /users/{id} - Обновить пользователя~n'),
    format('DELETE /users/{id} - Удалить пользователя~n'),
    format('~n'),
    format('Демонстрация ответов:~n'),
    format('~n'),
    format('=== GET / ===~n'),
    handle_root(_),
    format('~n'),
    format('=== GET /users ===~n'),
    handle_users(_),
    format('~n'),
    format('=== GET /users/1 ===~n'),
    handle_user_id(json([id=1])),
    format('~n'),
    format('=== GET /users/999 (несуществующий) ===~n'),
    handle_user_id(json([id=999])),
    format('~n'),
    format('Демонстрация завершена!~n').

% Запуск демо
:- demo_server.

