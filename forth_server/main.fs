\ Веб-сервер на Forth
\ Простая реализация HTTP сервера для сравнения с Go и Rust

\ Подключаем необходимые библиотеки
require unix/socket.fs

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
  USERS USER-SIZE + 2 OVER ! CELL+ USER2-NAME OVER ! CELL+ USER2-EMAIL OVER ! CELL+ 30 OVER ! CELL+ USER3-DATE OVER ! DROP
  \ Пользователь 3
  USERS USER-SIZE 2* + 3 OVER ! CELL+ USER3-NAME OVER ! CELL+ USER3-EMAIL OVER ! CELL+ 28 OVER ! CELL+ USER3-DATE OVER ! DROP
  3 USER-COUNT !
;

\ Буферы для работы с HTTP
CREATE HTTP-BUFFER BUFFER-SIZE ALLOT
CREATE RESPONSE-BUFFER 4096 ALLOT

\ Сокет сервера
VARIABLE SERVER-SOCKET

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

\ HTTP заголовки
: SEND-HEADERS ( socket -- )
  DUP S" HTTP/1.1 200 OK" ROT WRITE-LINE DROP
  DUP S" Content-Type: application/json" ROT WRITE-LINE DROP
  DUP S" Access-Control-Allow-Origin: *" ROT WRITE-LINE DROP
  DUP S" Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS" ROT WRITE-LINE DROP
  DUP S" Access-Control-Allow-Headers: Content-Type, Authorization" ROT WRITE-LINE DROP
  S" " ROT WRITE-LINE DROP ;

: SEND-ERROR ( socket code -- )
  DUP 404 = IF
    DUP S" HTTP/1.1 404 Not Found" ROT WRITE-LINE DROP
  ELSE
    DUP S" HTTP/1.1 400 Bad Request" ROT WRITE-LINE DROP
  THEN
  DUP S" Content-Type: application/json" ROT WRITE-LINE DROP
  S" " ROT WRITE-LINE DROP ;

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
: GET-USERS-RESPONSE ( -- addr len )
  S" {" RESPONSE-BUFFER SWAP MOVE
  RESPONSE-BUFFER 1+ \ Пропускаем открывающую скобку
  S" \"success\":true,\"message\":\"Пользователи успешно получены\",\"data\":[" SWAP MOVE
  RESPONSE-BUFFER 1+ 60 + \ Пропускаем уже записанную часть
  USER-COUNT @ 0 DO
    I 0> IF S" ," SWAP MOVE SWAP 1+ SWAP THEN
    S" {" SWAP MOVE SWAP 1+ SWAP
    S" \"id\":" SWAP MOVE SWAP 4+ SWAP
    USERS I USER-SIZE * + USER-TO-JSON
    S" }" SWAP MOVE SWAP 1+ SWAP
  LOOP
  S" ]}" SWAP MOVE SWAP 2+ SWAP
  RESPONSE-BUFFER SWAP ;

\ Генерация ответа для одного пользователя
: GET-USER-RESPONSE ( id -- addr len )
  DUP 0 USER-COUNT @ WITHIN IF
    S" {" RESPONSE-BUFFER SWAP MOVE
    RESPONSE-BUFFER 1+
    S" \"success\":true,\"message\":\"Пользователь найден\",\"data\":{" SWAP MOVE
    RESPONSE-BUFFER 1+ 50+
    S" \"id\":" SWAP MOVE SWAP 4+ SWAP
    USERS SWAP 1- USER-SIZE * + USER-TO-JSON
    S" }}" SWAP MOVE SWAP 2+ SWAP
    RESPONSE-BUFFER SWAP
  ELSE
    S" {\"success\":false,\"message\":\"Пользователь не найден\"}" RESPONSE-BUFFER SWAP MOVE
    RESPONSE-BUFFER 50
  THEN ;

\ Главная страница
: HOME-RESPONSE ( -- addr len )
  S" {\"success\":true,\"message\":\"Добро пожаловать в API веб-сервера на Forth!\",\"data\":{\"version\":\"1.0.0\",\"endpoints\":[\"GET / - Главная страница\",\"GET /users - Получить всех пользователей\",\"GET /users/{id} - Получить пользователя по ID\",\"POST /users - Создать нового пользователя\",\"PUT /users/{id} - Обновить пользователя\",\"DELETE /users/{id} - Удалить пользователя\"]}}" 
  RESPONSE-BUFFER SWAP MOVE
  RESPONSE-BUFFER 200 ;

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

: HANDLE-REQUEST ( socket -- )
  DUP HTTP-BUFFER BUFFER-SIZE READ-LINE DROP DROP
  HTTP-BUFFER PARSE-REQUEST 2DROP
  PARSE-PATH
  DUP S" /" STR-EQUAL IF
    DROP SEND-HEADERS HOME-RESPONSE ROT WRITE-LINE DROP
  ELSE
    DUP S" /users" STR-EQUAL IF
      DROP SEND-HEADERS GET-USERS-RESPONSE ROT WRITE-LINE DROP
    ELSE
      DUP S" /users/" FIND-SUBSTRING IF
        DROP 6 + DUP STR-LEN 1- SWAP 1+ SWAP
        S>NUMBER DROP DROP
        SEND-HEADERS GET-USER-RESPONSE ROT WRITE-LINE DROP
      ELSE
        DROP 404 SEND-ERROR
      THEN
    THEN
  THEN ;

\ Основной цикл сервера
: START-SERVER
  INIT-USERS
  S" 🚀 Forth веб-сервер запущен на порту " TYPE SERVER-PORT . CR
  S" 📖 API документация доступна по адресу: http://localhost:" TYPE SERVER-PORT . CR
  SERVER-PORT CREATE-SOCKET SERVER-SOCKET !
  SERVER-SOCKET @ BIND-SOCKET
  SERVER-SOCKET @ LISTEN-SOCKET
  BEGIN
    SERVER-SOCKET @ ACCEPT-SOCKET
    DUP HANDLE-REQUEST
    CLOSE-SOCKET
  AGAIN ;

\ Запуск сервера
START-SERVER
