using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace NexusIMS.API.Custom_Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger; // خلتها readonly للأمان

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // 🔥 التصحيح الأول: تغيير الاسم إلى InvokeAsync ليقراه الـ .NET صراحة
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع في السيستم: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // الافتراضي لو ملقناش أي شرط
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "حدث خطأ داخلي في السيرفر، يرجى المحاولة لاحقاً.";

            // تحويل الخطأ بالكامل لنص شامل عشان نفتش فيه بدقة
            var fullExceptionText = exception.ToString();

            // 🔥 التصحيح الثاني: فحص ذكي وشامل للـ Foreign Key
            if (fullExceptionText.Contains("FOREIGN KEY constraint") || fullExceptionText.Contains("FK_"))
            {
                statusCode = HttpStatusCode.BadRequest;
                message = "فشل الحفظ بسبب عدم ترابط البيانات (تأكد من صحة المعرفات المرسلة كـ UserId أو CategoryId ومطابقتها للداتابيز).";
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "عذراً، أنت غير مصرح لك بالقيام بهذا الإجراء.";
            }
            else
            {
                // الـ else الجميلة بتاعتك اللي هتمسك أي حاجة تانية
                statusCode = HttpStatusCode.BadRequest;
                message = "عذراً، حدث خطأ ما يرجى التواصل مع المسؤول.";
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                IsSuccess = false,
                StatusCode = context.Response.StatusCode,
                Message = message,
                Details = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}