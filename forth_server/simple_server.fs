\ Простой веб-сервер на Forth
\ Базовая реализация HTTP сервера для демонстрации

\ Константы
8082 CONSTANT SERVER-PORT
1024 CONSTANT BUFFER-SIZE
3 CONSTANT MAX-USERS

\ Структуры данных для пользователей
CREATE USER-STRUCT
  CELL , \ id
  CELL , \ name (адрес строки)
  CELL , \ email (адрес строки)  
  CELL , \ age
  CELL , \ created_at (адрес строки)
CONSTANT USER-SIZE

\ Хранилище пользователей
CREATE USERS MAX-USERS USER-SIZE * ALLOT
VARIABLE USER-COUNT
0 USER-COUNT !

\ Строки для пользователей
CREATE USER1-NAME ," Иван Иванов"
CREATE USER1-EMAIL ," ivan@example.com"
CREATE USER1-DATE ," 2024-01-01T10:00:00Z"

CREATE USER2-NAME ," Мария Петрова"
CREATE USER2-EMAIL ," maria@example.com"
CREATE USER2-DATE ," 2024-01-02T11:00:00Z"

CREATE USER3-NAME ," Алексей Сидоров"
CREATE USER3-EMAIL ," alex@example.com"
CREATE USER3-DATE ," 2024-01-03T12:00:00Z"

\ Инициализация пользователей
: INIT-USERS
  0 USER-COUNT !
  \ Пользователь 1
  USERS 1 OVER ! CELL+ USER1-NAME OVER ! CELL+ USER1-EMAIL OVER ! CELL+ 25 OVER ! CELL+ USER1-DATE OVER ! DROP
  \ Пользователь 2  
  USERS USER-SIZE + 2 OVER ! CELL+ USER2-NAME OVER ! CELL+ USER2-EMAIL OVER ! CELL+ 30 OVER ! CELL+ USER2-DATE OVER ! DROP
  \ Пользователь 3
  USERS USER-SIZE 2* + 3 OVER ! CELL+ USER3-NAME OVER ! CELL+ USER3-EMAIL OVER ! CELL+ 28 OVER ! CELL+ USER3-DATE OVER ! DROP
  3 USER-COUNT !
;

\ Буферы для работы с HTTP
CREATE HTTP-BUFFER BUFFER-SIZE ALLOT
CREATE RESPONSE-BUFFER 4096 ALLOT

\ Функции для работы со строками
: STR-LEN ( addr -- len )
  DUP BEGIN DUP C@ WHILE 1+ REPEAT SWAP - ;

: STR-EQUAL ( addr1 addr2 -- flag )
  2DUP STR-LEN SWAP STR-LEN <> IF 2DROP FALSE EXIT THEN
  STR-LEN 0 DO
    2DUP I + C@ SWAP I + C@ <> IF 2DROP FALSE UNLOOP EXIT THEN
  LOOP 2DROP TRUE ;

\ Поиск подстроки в строке
: FIND-SUBSTRING ( str sub -- addr flag )
  SWAP DUP STR-LEN ROT DUP STR-LEN ROT
  2DUP < IF 2DROP 2DROP 0 FALSE EXIT THEN
  OVER - 1+ 0 DO
    2DUP I + SWAP OVER STR-LEN STR-EQUAL IF
      DROP I + TRUE UNLOOP EXIT
    THEN
  LOOP 2DROP 0 FALSE ;

\ JSON генерация
: JSON-ESCAPE ( addr -- )
  DUP STR-LEN 0 DO
    DUP I + C@
    DUP [CHAR] " = IF DROP S" \"" TYPE ELSE
    DUP [CHAR] \ = IF DROP S" \\" TYPE ELSE
    DUP [CHAR] / = IF DROP S" \/" TYPE ELSE
    DUP [CHAR] \n = IF DROP S" \n" TYPE ELSE
    DUP [CHAR] \r = IF DROP S" \r" TYPE ELSE
    DUP [CHAR] \t = IF DROP S" \t" TYPE ELSE
    EMIT THEN THEN THEN THEN THEN THEN
  LOOP DROP ;

: JSON-STRING ( addr -- )
  [CHAR] " EMIT JSON-ESCAPE [CHAR] " EMIT ;

: JSON-NUMBER ( n -- )
  S>D <# #S #> TYPE ;

\ Генерация JSON для пользователя
: USER-TO-JSON ( user-addr -- )
  DUP @ JSON-NUMBER S" ," TYPE
  CELL+ DUP @ JSON-STRING S" ," TYPE
  CELL+ DUP @ JSON-STRING S" ," TYPE
  CELL+ DUP @ JSON-NUMBER S" ," TYPE
  CELL+ @ JSON-STRING ;

\ Генерация ответа для всех пользователей
: GET-USERS-RESPONSE
  S" {\"success\":true,\"message\":\"Пользователи успешно получены\",\"data\":[" TYPE
  USER-COUNT @ 0 DO
    I 0> IF S" ," TYPE THEN
    S" {" TYPE
    S" \"id\":" TYPE
    USERS I USER-SIZE * + USER-TO-JSON
    S" }" TYPE
  LOOP
  S" ]}" TYPE ;

\ Генерация ответа для одного пользователя
: GET-USER-RESPONSE ( id -- )
  DUP 0 USER-COUNT @ WITHIN IF
    S" {\"success\":true,\"message\":\"Пользователь найден\",\"data\":{" TYPE
    S" \"id\":" TYPE
    USERS SWAP 1- USER-SIZE * + USER-TO-JSON
    S" }}" TYPE
  ELSE
    S" {\"success\":false,\"message\":\"Пользователь не найден\"}" TYPE
  THEN ;

\ Главная страница
: HOME-RESPONSE
  S" {\"success\":true,\"message\":\"Добро пожаловать в API веб-сервера на Forth!\",\"data\":{\"version\":\"1.0.0\",\"endpoints\":[\"GET / - Главная страница\",\"GET /users - Получить всех пользователей\",\"GET /users/{id} - Получить пользователя по ID\",\"POST /users - Создать нового пользователя\",\"PUT /users/{id} - Обновить пользователя\",\"DELETE /users/{id} - Удалить пользователя\"]}}" TYPE ;

\ Обработка HTTP запросов
: PARSE-REQUEST ( -- method path )
  HTTP-BUFFER DUP STR-LEN 0 DO
    DUP I + C@ [CHAR] SPACE = IF
      I 1+ SWAP I - SWAP EXIT
    THEN
  LOOP
  DUP STR-LEN SWAP ;

: PARSE-PATH ( -- path )
  HTTP-BUFFER DUP STR-LEN 0 DO
    DUP I + C@ [CHAR] SPACE = IF
      I 1+ + DUP STR-LEN 0 DO
        DUP I + C@ [CHAR] SPACE = IF
          I SWAP - SWAP EXIT
        THEN
      LOOP
      DUP STR-LEN SWAP EXIT
    THEN
  LOOP
  DUP STR-LEN SWAP ;

\ HTTP заголовки
: SEND-HEADERS
  S" HTTP/1.1 200 OK" TYPE CR
  S" Content-Type: application/json" TYPE CR
  S" Access-Control-Allow-Origin: *" TYPE CR
  S" Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS" TYPE CR
  S" Access-Control-Allow-Headers: Content-Type, Authorization" TYPE CR
  CR ;

: SEND-ERROR ( code -- )
  DUP 404 = IF
    S" HTTP/1.1 404 Not Found" TYPE CR
  ELSE
    S" HTTP/1.1 400 Bad Request" TYPE CR
  THEN
  S" Content-Type: application/json" TYPE CR
  CR ;

\ Обработка запросов (упрощенная версия)
: HANDLE-REQUEST ( path -- )
  DUP S" /" STR-EQUAL IF
    DROP SEND-HEADERS HOME-RESPONSE
  ELSE
    DUP S" /users" STR-EQUAL IF
      DROP SEND-HEADERS GET-USERS-RESPONSE
    ELSE
      DUP S" /users/" FIND-SUBSTRING IF
        DROP 6 + DUP STR-LEN 1- SWAP 1+ SWAP
        S>NUMBER DROP DROP
        SEND-HEADERS GET-USER-RESPONSE
      ELSE
        DROP 404 SEND-ERROR
      THEN
    THEN
  THEN ;

\ Демонстрационная функция
: DEMO-SERVER
  INIT-USERS
  S" 🚀 Forth веб-сервер (демо версия)" TYPE CR
  S" 📖 Доступные эндпоинты:" TYPE CR
  S" GET / - Главная страница" TYPE CR
  S" GET /users - Получить всех пользователей" TYPE CR
  S" GET /users/{id} - Получить пользователя по ID" TYPE CR
  CR
  S" Демонстрация ответов:" TYPE CR
  CR
  S" === GET / ===" TYPE CR
  S" /" HANDLE-REQUEST CR CR
  S" === GET /users ===" TYPE CR
  S" /users" HANDLE-REQUEST CR CR
  S" === GET /users/1 ===" TYPE CR
  S" /users/1" HANDLE-REQUEST CR CR
  S" === GET /users/999 ===" TYPE CR
  S" /users/999" HANDLE-REQUEST CR CR
  S" Демонстрация завершена!" TYPE CR ;

\ Запуск демо
DEMO-SERVER
