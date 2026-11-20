# 🏗️ KeycloakPulumiAuth — Infrastructure as Code (IaC) для Keycloak на C#

Цей проєкт демонструє **Кейс B модуля 03.3**: декларативне керування конфігурацією **Keycloak 26** як кодом (IaC) за допомогою **Pulumi (.NET 10)**.

Замість ручного налаштування клієнтів, скоупів і користувачів в адмін-панелі Keycloak, усе описується на C# у [`RealmBuild.cs`](RealmBuild.cs), версіонується в Git і точково накочується на живий Keycloak через Admin REST API.

---

## 📁 Структура проєкту

| Файл | Призначення |
| :--- | :--- |
| [`Program.cs`](Program.cs) | Точка входу Pulumi (`Deployment.RunAsync<RealmBuild>()`). |
| [`RealmBuild.cs`](RealmBuild.cs) | Головний стек IaC: описує реалм **`DevRealmDrivers-Pulumi`**, скоупи, OIDC-клієнтів, тестових юзерів (`alice`/`bob`) та блок **UPDATE** для демо. |
| [`Pulumi.yaml`](Pulumi.yaml) | Метадані Pulumi-проєкту (`runtime: dotnet`). |
| [`Pulumi.dev.yaml`](Pulumi.dev.yaml) | Конфігурація стеку `dev`: адреса Keycloak (`http://localhost:5080/auth`), `admin-cli`, `admin`/`admin`. |
| [`Factories/ClientFactory.cs`](Factories/ClientFactory.cs) | Фабрика OIDC-клієнтів: `CreateCodeFlowClient` (Authorization Code + PKCE `S256`) та `CreateClientCredentialsFlowClient` (M2M). |
| [`Factories/ScopeFactory.cs`](Factories/ScopeFactory.cs) | Фабрика кастомних Client Scopes (`drivers_management`, `blog_management`, `cargo_ordering`). |
| [`Extensions/ClientExtensions.cs`](Extensions/ClientExtensions.cs) | Розширення для клієнтів: `ExtendDefaultScopes(...)` та `AddAudienceMapper(...)`. |
| [`Extensions/UserExtensions.cs`](Extensions/UserExtensions.cs) | Розширення для призначення ролей користувачам (`SetRoles(...)`). |

---

## ✅ Передумови

1. **.NET 10 SDK** (для збірки проєкту).
2. **Запущений Keycloak на `http://localhost:5080/auth`** (найпростіше — запустити **`CargoDelivery.AppHost`** через Aspire, він сам підійме `keycloak-pg` + `keycloak:26.0` з адміном `admin`/`admin`).
3. **Pulumi CLI** (встановити один раз, якщо ще немає):
   ```powershell
   winget install Pulumi.Pulumi
   # або: choco install pulumi
   ```
   Після встановлення перевірте версію у новому терміналі:
   ```powershell
   pulumi version
   ```

---

## 🚀 Покроковий запуск

Відкрийте термінал у папці **`02-Microservices-Craftsman\KeycloakPulumiAuth`**:

### 1. Локальний backend стану (без хмарного акаунта Pulumi)
```powershell
pulumi login --local
```

### 2. Вибір або створення стеку `dev`
Якщо стек `dev` запускається вперше на цьому ПК:
```powershell
pulumi stack init dev
```
Якщо вже створювався раніше:
```powershell
pulumi stack select dev
```
> При першому `pulumi stack init dev` Pulumi може попросити passphrase для шифрування локального стану (для локального демо можна натиснути `Enter` для порожнього пароля або задати будь-який і передавати його через `$env:PULUMI_CONFIG_PASSPHRASE=""`).

### 3. Перевірка плану змін (dry-run)
```powershell
pulumi preview
```
Pulumi збере .NET-проєкт і покаже список ресурсів (`+ create`), які будуть створені в Keycloak, **нічого ще не змінюючи**.

### 4. Застосування конфігурації до живого Keycloak
```powershell
pulumi up
```
Підтвердіть **`yes`** → у адмін-панелі Keycloak (`http://localhost:5080/auth`, логін `admin` / `admin`) поруч із `DevRealmDrivers` з'явиться новий реалм **`DevRealmDrivers-Pulumi`** з усіма клієнтами, скоупами та користувачами `alice`/`bob`.

---

## 🔄 Демонстрація Update (оновлення без перезапуску Keycloak)

1. Відкрийте [`RealmBuild.cs`](RealmBuild.cs) і розкоментуйте блок **`6. КЕЙС АПДЕЙТА (UPDATE)`** (додає скоуп `cargo_analytics`, M2M-клієнт `ordering_management.analytics` і роль `Dispatcher`).
2. Перевірте точковий diff:
   ```powershell
   pulumi preview
   ```
   У плані з'явиться **`+ 3 to create`** — існуючі ресурси та користувачі не зачіпаються.
3. Застосуйте оновлення:
   ```powershell
   pulumi up
   ```
4. Оновіть сторінку адмінки Keycloak (`http://localhost:5080/auth`) — новий скоуп, клієнт і роль одразу з'являться в `DevRealmDrivers-Pulumi`.

---

## 🧹 Видалення створеного реалму (опційно)

Щоб чисто видалити `DevRealmDrivers-Pulumi` і всі його ресурси з Keycloak:
```powershell
pulumi destroy
```

---

## 🛠️ Часті проблеми

| Проблема | Причина та рішення |
| :--- | :--- |
| `error: could not connect to http://localhost:5080/auth` | Контейнер `keycloak` не запущений — запустіть `CargoDelivery.AppHost` і дочекайтеся готовності `keycloak` в Aspire Dashboard. |
| `no stack named 'dev' found` | При першому запуску на новій машині виконайте `pulumi stack init dev` (файл `Pulumi.dev.yaml` підхопиться автоматично). |
| При вході `alice`/`bob` у `DevRealmDrivers-Pulumi` Keycloak просить змінити пароль | У `RealmBuild.cs` для тестових користувачів задано `Temporary = true` — це очікувана поведінка для першого входу (або змініть на `false`). |
