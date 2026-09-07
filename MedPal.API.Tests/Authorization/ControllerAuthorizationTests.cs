using System.Linq;
using System.Reflection;
using MedPal.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Tests.Authorization
{
    /// <summary>
    /// Fase 1 (Endurecimiento crítico): evita regresiones de controladores anónimos.
    /// Todo controlador de la API debe tener [Authorize] a nivel de clase (directo o heredado
    /// de BaseController) o [AllowAnonymous] explícito a nivel de clase. La única excepción
    /// son los webhooks, que firman sus peticiones (Stripe / WhatsApp) pero también deben
    /// declarar [AllowAnonymous] explícito a nivel de clase.
    /// </summary>
    public class ControllerAuthorizationTests
    {
        // Webhooks: autenticados por firma del proveedor (Stripe-Signature / X-Hub-Signature-256),
        // no por JWT. Única categoría permitida sin [Authorize] a nivel de clase.
        private static readonly string[] AllowList =
        {
            nameof(StripeWebhookController),
            nameof(WhatsAppWebhookController)
        };

        public static TheoryData<System.Type> Controllers()
        {
            var data = new TheoryData<System.Type>();
            var assembly = typeof(InvoiceController).Assembly;

            var controllerTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "MedPal.API.Controllers")
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Where(t => t != typeof(BaseController))
                .OrderBy(t => t.Name);

            foreach (var type in controllerTypes)
            {
                data.Add(type);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(Controllers))]
        public void EveryController_IsProtectedOrExplicitlyAnonymous(System.Type controllerType)
        {
            var hasAuthorize = controllerType
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            var hasAllowAnonymous = controllerType
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
                .Any();

            if (AllowList.Contains(controllerType.Name))
            {
                Assert.True(hasAllowAnonymous,
                    $"{controllerType.Name} está en la allow-list de webhooks y debe declarar [AllowAnonymous] explícito a nivel de clase.");
            }
            else
            {
                Assert.True(hasAuthorize,
                    $"{controllerType.Name} debe tener [Authorize] a nivel de clase (directo o vía BaseController).");
            }
        }
    }
}