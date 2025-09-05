\ Минимальный демонстрационный веб-сервер на Forth
\ Показывает базовые возможности Forth для создания API

\ Демонстрационные данные
: USER1-NAME S" Иван Иванов" ;
: USER1-EMAIL S" ivan@example.com" ;
: USER1-AGE 25 ;
: USER1-DATE S" 2024-01-01T10:00:00Z" ;

: USER2-NAME S" Мария Петрова" ;
: USER2-EMAIL S" maria@example.com" ;
: USER2-AGE 30 ;
: USER2-DATE S" 2024-01-02T11:00:00Z" ;

: USER3-NAME S" Алексей Сидоров" ;
: USER3-EMAIL S" alex@example.com" ;
: USER3-AGE 28 ;
: USER3-DATE S" 2024-01-03T12:00:00Z" ;

\ JSON генерация
: JSON-STRING ( addr u -- )
  [CHAR] " EMIT TYPE [CHAR] " EMIT ;

: JSON-NUMBER ( n -- )
  S>D <# #S #> TYPE ;

\ Генерация JSON для пользователя
: USER1-TO-JSON
  S" {" TYPE
  S" \"id\":" TYPE 1 JSON-NUMBER S" ," TYPE
  S" \"name\":" TYPE USER1-NAME JSON-STRING S" ," TYPE
  S" \"email\":" TYPE USER1-EMAIL JSON-STRING S" ," TYPE
  S" \"age\":" TYPE USER1-AGE JSON-NUMBER S" ," TYPE
  S" \"created_at\":" TYPE USER1-DATE JSON-STRING
  S" }" TYPE ;

: USER2-TO-JSON
  S" {" TYPE
  S" \"id\":" TYPE 2 JSON-NUMBER S" ," TYPE
  S" \"name\":" TYPE USER2-NAME JSON-STRING S" ," TYPE
  S" \"email\":" TYPE USER2-EMAIL JSON-STRING S" ," TYPE
  S" \"age\":" TYPE USER2-AGE JSON-NUMBER S" ," TYPE
  S" \"created_at\":" TYPE USER2-DATE JSON-STRING
  S" }" TYPE ;

: USER3-TO-JSON
  S" {" TYPE
  S" \"id\":" TYPE 3 JSON-NUMBER S" ," TYPE
  S" \"name\":" TYPE USER3-NAME JSON-STRING S" ," TYPE
  S" \"email\":" TYPE USER3-EMAIL JSON-STRING S" ," TYPE
  S" \"age\":" TYPE USER3-AGE JSON-NUMBER S" ," TYPE
  S" \"created_at\":" TYPE USER3-DATE JSON-STRING
  S" }" TYPE ;

\ Генерация ответа для всех пользователей
: GET-USERS-RESPONSE
  S" {\"success\":true,\"message\":\"Пользователи успешно получены\",\"data\":[" TYPE
  USER1-TO-JSON S" ," TYPE
  USER2-TO-JSON S" ," TYPE
  USER3-TO-JSON
  S" ]}" TYPE ;

\ Генерация ответа для одного пользователя
: GET-USER-RESPONSE ( id -- )
  DUP 1 = IF
    DROP S" {\"success\":true,\"message\":\"Пользователь найден\",\"data\":" TYPE
    USER1-TO-JSON S" }" TYPE
  ELSE
    DUP 2 = IF
      DROP S" {\"success\":true,\"message\":\"Пользователь найден\",\"data\":" TYPE
      USER2-TO-JSON S" }" TYPE
    ELSE
      DUP 3 = IF
        DROP S" {\"success\":true,\"message\":\"Пользователь найден\",\"data\":" TYPE
        USER3-TO-JSON S" }" TYPE
      ELSE
        DROP S" {\"success\":false,\"message\":\"Пользователь не найден\"}" TYPE
      THEN
    THEN
  THEN ;

\ Главная страница
: HOME-RESPONSE
  S" {\"success\":true,\"message\":\"Добро пожаловать в API веб-сервера на Forth!\",\"data\":{\"version\":\"1.0.0\",\"endpoints\":[\"GET / - Главная страница\",\"GET /users - Получить всех пользователей\",\"GET /users/{id} - Получить пользователя по ID\",\"POST /users - Создать нового пользователя\",\"PUT /users/{id} - Обновить пользователя\",\"DELETE /users/{id} - Удалить пользователя\"]}}" TYPE ;

\ HTTP заголовки
: SEND-HEADERS
  S" HTTP/1.1 200 OK" TYPE CR
  S" Content-Type: application/json" TYPE CR
  S" Access-Control-Allow-Origin: *" TYPE CR
  S" Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS" TYPE CR
  S" Access-Control-Allow-Headers: Content-Type, Authorization" TYPE CR
  CR ;

: SEND-ERROR
  S" HTTP/1.1 404 Not Found" TYPE CR
  S" Content-Type: application/json" TYPE CR
  CR ;

\ Демонстрационная функция
: DEMO-SERVER
  S" 🚀 Forth веб-сервер (демо версия)" TYPE CR
  S" 📖 Доступные эндпоинты:" TYPE CR
  S" GET / - Главная страница" TYPE CR
  S" GET /users - Получить всех пользователей" TYPE CR
  S" GET /users/{id} - Получить пользователя по ID" TYPE CR
  CR
  S" Демонстрация ответов:" TYPE CR
  CR
  S" === GET / ===" TYPE CR
  SEND-HEADERS HOME-RESPONSE CR CR
  S" === GET /users ===" TYPE CR
  SEND-HEADERS GET-USERS-RESPONSE CR CR
  S" === GET /users/1 ===" TYPE CR
  SEND-HEADERS 1 GET-USER-RESPONSE CR CR
  S" === GET /users/2 ===" TYPE CR
  SEND-HEADERS 2 GET-USER-RESPONSE CR CR
  S" === GET /users/3 ===" TYPE CR
  SEND-HEADERS 3 GET-USER-RESPONSE CR CR
  S" === GET /users/999 (несуществующий) ===" TYPE CR
  SEND-ERROR CR CR
  S" Демонстрация завершена!" TYPE CR ;

\ Запуск демо
DEMO-SERVER
