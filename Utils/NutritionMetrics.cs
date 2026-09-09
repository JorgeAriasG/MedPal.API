using System;

namespace MedPal.API.Utils
{
    /// <summary>
    /// Cálculos canónicos del módulo de Nutrición.
    /// Convención única de unidades: peso en kg, altura en centímetros (cm),
    /// circunferencias en centímetros (cm). Todas las rutas (create/update)
    /// deben usar estas fórmulas para garantizar consistencia clínica.
    /// </summary>
    public static class NutritionMetrics
    {
        /// <summary>
        /// IMC = peso(kg) / altura(m)², con altura en cm.
        /// Devuelve null si no hay suficientes datos válidos.
        /// </summary>
        public static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
        {
            if (!weightKg.HasValue || weightKg.Value <= 0 || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var heightM = heightCm.Value / 100m;
            return Math.Round(weightKg.Value / (heightM * heightM), 1);
        }

        /// <summary>
        /// Relación cintura-altura (WC/Ht), ambas en cm (referencia: &lt; 0.5).
        /// </summary>
        public static decimal? CalculateWaistHeightRatio(decimal? waistCm, decimal? heightCm)
        {
            if (!waistCm.HasValue || waistCm.Value <= 0 || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            return Math.Round(waistCm.Value / heightCm.Value, 2);
        }

        /// <summary>
        /// Relación cintura-cadera (WC/HC), ambas en cm.
        /// </summary>
        public static decimal? CalculateWaistHipRatio(decimal? waistCm, decimal? hipCm)
        {
            if (!waistCm.HasValue || waistCm.Value <= 0 || !hipCm.HasValue || hipCm.Value <= 0)
                return null;

            return Math.Round(waistCm.Value / hipCm.Value, 3);
        }
    }
}