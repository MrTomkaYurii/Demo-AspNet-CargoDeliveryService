# 📄 `Pages/` — Карта Razor Pages UI для Duende IdentityServer (03.2)

Ця папка містить веб-інтерфейс (Razor Pages) на базі офіційного **Duende IdentityServer Quickstart UI**. Сам движок Duende реалізує протокольні ендпоінти (`/connect/authorize`, `/connect/token`, `/.well-known/openid-configuration`), а сторінки в `Pages/` відповідають за **інтерактивну взаємодію з користувачем** (форма логіну, вихід, перегляд клеймів, помилки тощо).

> Усі сторінки за замовчуванням захищені через `.RequireAuthorization()` у [`HostingExtensions.cs`](../HostingExtensions.cs), крім тих, де явно вказано `[AllowAnonymous]` (наприклад, `/Account/Login`, `/Home/Error`).

---

## 🟢 1. Активні блоки (використовуються в нашому демо `03.2`)

| Папка / файл | Маршрут | За що відповідає |
| :--- | :--- | :--- |
| **`Index.cshtml` (+ `.cs`)** | `/` | Головна сторінка STS (`https://localhost:5012`): посилання на OIDC Discovery, JWKS, Diagnostics, Grants і шпаргалка по демо-акаунтах (`alice`/`bob`). |
| **`Account/Login/`** | `/Account/Login` | **Ключова сторінка Authorization Code + PKCE.** Сюди Duende редиректить браузер із `/connect/authorize`. Перевіряє логін/пароль через `TestUserStore` ([`TestUsers.cs`](../TestUsers.cs)), виставляє cookie (`SignInAsync`) і повертає браузер у `/connect/authorize/callback` для видачі `code`. |
| **`Account/Logout/`** | `/Account/Logout` | Завершення сесії користувача (`SignOutAsync`), очищення cookie STS, сторінка підтвердження `LoggedOut.cshtml` та перенаправлення на `post_logout_redirect_uri` (Front-Channel Sign-Out). |
| **`Account/AccessDenied.*`** | `/Account/AccessDenied` | Сторінка помилки `403 Forbidden`, якщо користувач автентифікований, але не має прав на захищену Razor Page всередині STS. |
| **`Home/Error/`** | `/Home/Error` | Сторінка відображення протокольних помилок OIDC/OAuth2 (наприклад, `invalid_redirect_uri`, `unauthorized_client`, `invalid_scope`) із деталями `ErrorMessage` у `Development`. |
| **`Redirect/`** | `/Redirect` | Проміжна сторінка мета-редиректу для native/desktop/mobile клієнтів (кращий UX при поверненні з браузера у локальний callback). |
| **`Diagnostics/`** | `/Diagnostics` | **Дуже корисно на демо:** показує всі клейми (`sub`, `name`, `role`, `idp`) та метадані поточної cookie-сесії залогіненого користувача (доступно лише з `localhost`). |
| **`Grants/`** | `/Grants` | Особистий кабінет користувача: список клієнтів (напр. `craftsman.swagger`), яким видано доступ або Refresh Tokens, із кнопкою **Revoke Access**. |
| **`Shared/`** | — | Спільний макет (`_Layout.cshtml`), верхня навігація з ім'ям юзера й кнопкою Logout (`_Nav.cshtml`) та блок помилок валідації (`_ValidationSummary.cshtml`). |
| **`SecurityHeadersAttribute.cs`** | *(фільтр)* | Атрибут `[SecurityHeaders]`: додає захисні HTTP-заголовки (`X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`, `Referrer-Policy`) на всі UI-сторінки STS. |
| **`Extensions.cs`** | *(хелпери)* | Допоміжні методи для Razor Pages (перевірка `IsNativeClient()`, генерація `LoadingPage()` для редиректу). |

---

## 🟡 2. Опційні блоки (зарезервовані з шаблону Duende, зараз вимкнені)

Ці папки залишені на випадок розширення демо — вони не заважають роботі `03.2`, але активуються лише за додаткового налаштування:

| Папка | Маршрут | Коли стає потрібною |
| :--- | :--- | :--- |
| **`Consent/`** | `/Consent` | Екран згоди користувача («Дозволити клієнту X доступ до скоупів Y?»). Активується, якщо в [`Config.cs`](../Config.cs) для клієнта виставити `RequireConsent = true`. |
| **`ExternalLogin/`** | `/ExternalLogin/Challenge`, `/Callback` | Федерація із зовнішніми провайдерами (Google, Keycloak, Azure AD). У нашому стенді `03.2` працює автономно, тому блок зовнішнього входу прихований. |
| **`Account/Create/`** | `/Account/Create` | Проста форма реєстрації нового тестового користувача в пам'яті (для демо-сценаріїв без БД). |
| **`Device/`** | `/Device` | **Device Authorization Flow** (RFC 8628) — вхід на пристроях без браузера (Smart TV, CLI, IoT) через введення короткого коду на іншому пристрої. |
| **`Ciba/`** | `/Ciba` | **CIBA (Client-Initiated Backchannel Authentication)** — асинхронне підтвердження входу на іншому пристрої (наприклад, push-підтвердження у банківському застосунку). |
| **`ServerSideSessions/`** | `/ServerSideSessions` | Адмін-панель активних серверних сесій. Активується після розкоментування `isBuilder.AddServerSideSessions()` у [`HostingExtensions.cs`](../HostingExtensions.cs). |
