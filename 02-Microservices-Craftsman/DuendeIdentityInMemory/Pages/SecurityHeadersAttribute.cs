// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

// ====================================================================================
// 🛡️ ФІЛЬТР ЗАГОЛОВКІВ БЕЗПЕКИ (SECURITY HEADERS) ДЛЯ RAZOR PAGES STS
// ------------------------------------------------------------------------------------
// Додає обов'язкові HTTP-заголовки захисту до кожної сторінки входу / згоди / виходу:
//   • X-Content-Type-Options: nosniff — забороняє браузеру «вгадувати» MIME-тип
//   • X-Frame-Options: SAMEORIGIN     — захист від Clickjacking (вбудовування в чужий iframe)
//   • Content-Security-Policy (CSP)   — обмежує джерела скриптів/об'єктів лише 'self'
//   • Referrer-Policy: no-referrer    — не передає URL (зокрема query-параметри) іншим сайтам
// ====================================================================================

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DuendeIdentityInMemory.Pages
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result;
            if (result is PageResult)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.HttpContext.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                var csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";

                // Стандартний заголовок для сучасних браузерів
                if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Append("Content-Security-Policy", csp);
                }
                // Сумісність зі старими версіями IE
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
                {
                    context.HttpContext.Response.Headers.Append("X-Content-Security-Policy", csp);
                }

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                var referrer_policy = "no-referrer";
                if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.HttpContext.Response.Headers.Append("Referrer-Policy", referrer_policy);
                }
            }
        }
    }
}
