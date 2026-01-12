using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MedPal.API.Middleware
{
    /// <summary>
    /// Middleware para manejar excepciones de forma global y retornar respuestas consistentes
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Procesa la solicitud y maneja excepciones
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Una excepción no manejada ocurrió");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maneja la excepción y retorna una respuesta JSON consistente
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse { StatusCode = StatusCodes.Status500InternalServerError };

            switch (exception)
            {
                case ArgumentException argEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Error = argEx.Message;
                    response.ErrorType = "ArgumentException";
                    break;

                case InvalidOperationException invOpEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Error = invOpEx.Message;
                    response.ErrorType = "InvalidOperationException";
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Error = keyNotFoundEx.Message ?? "El recurso no fue encontrado";
                    response.ErrorType = "KeyNotFoundException";
                    break;

                case UnauthorizedAccessException unAuthEx:
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Error = unAuthEx.Message ?? "No autorizado";
                    response.ErrorType = "UnauthorizedAccessException";
                    break;

                case ValidationException valEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Error = "Validación fallida";
                    response.ErrorType = "ValidationException";
                    response.Details = valEx.Errors?.Select(e => (object)new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                    break;

                default:
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Error = "Error interno del servidor";
                    response.ErrorType = exception.GetType().Name;
                    break;
            }

            context.Response.StatusCode = response.StatusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>
    /// Modelo para respuestas de error consistentes
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>Código de estado HTTP</summary>
        public int StatusCode { get; set; }

        /// <summary>Mensaje de error</summary>
        public string Error { get; set; }

        /// <summary>Tipo de excepción</summary>
        public string ErrorType { get; set; }

        /// <summary>Detalles adicionales (ej: errores de validación)</summary>
        public List<object> Details { get; set; } = new();

        /// <summary>Timestamp de cuando ocurrió el error</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
