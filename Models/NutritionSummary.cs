using System;

namespace NutritionPlannerApp.Models
{
    public class DietPlanRecord
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
        public string Title { get; set; } = "飲食計算與菜單計劃";
        public DateTime PlanDate { get; set; } = DateTime.Now;
        public double TargetCalories { get; set; }
        public string ExchangesJson { get; set; } = "";
        public string FormulasJson { get; set; } = "";
        public string SummaryNotes { get; set; } = "";
    }

    public class NutritionSummary
    {
        // 實際攝取總計
        public double ActualProtein { get; set; }
        public double ActualFat { get; set; }
        public double ActualCarbs { get; set; }
        public double ActualCalories { get; set; }

        public double ActualProteinRatio { get; set; }
        public double ActualFatRatio { get; set; }
        public double ActualCarbRatio { get; set; }

        // 建議飲食總計
        public double SuggestedProtein { get; set; }
        public double SuggestedFat { get; set; }
        public double SuggestedCarbs { get; set; }
        public double SuggestedCalories { get; set; }

        public double SuggestedProteinRatio { get; set; }
        public double SuggestedFatRatio { get; set; }
        public double SuggestedCarbRatio { get; set; }

        // 目標差距
        public double TargetCalories { get; set; }
        public double TargetProteinGrams { get; set; }
        public double TargetFatGrams { get; set; }
        public double TargetCarbGrams { get; set; }

        public double CalorieAchievementRate => TargetCalories > 0 ? Math.Round((SuggestedCalories / TargetCalories) * 100, 1) : 0;
    }
}
