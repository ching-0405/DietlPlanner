using System;

namespace NutritionPlannerApp.Models
{
    public class ClientProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "小明";
        public string ChartNumber { get; set; } = "P001";
        public string Gender { get; set; } = "男"; // "男" or "女"
        public double Age { get; set; } = 30;
        public double Height { get; set; } = 170; // cm
        public double Weight { get; set; } = 68; // kg
        public double Creatinine { get; set; } = 0.9; // mg/dL
        
        // 估算因子
        public double ActivityFactor { get; set; } = 1.3; // 輕度活動
        public string ActivityDescription { get; set; } = "輕度活動 (1.3)";
        public double StressFactor { get; set; } = 1.0; // 正常無壓力
        public string StressDescription { get; set; } = "正常無壓力 (1.0)";

        // 推估輔助指標
        public double KneeHeight { get; set; } = 50; // cm
        public double UlnaLength { get; set; } = 25; // cm
        public double HipCircumference { get; set; } = 90; // cm
        public double ArmCircumference { get; set; } = 28; // cm

        // 目標熱量與三大營養素比例
        public double TargetCalories { get; set; } = 2000;
        public double CarbRatio { get; set; } = 50; // 50%
        public double ProteinRatio { get; set; } = 20; // 20%
        public double FatRatio { get; set; } = 30; // 30%

        public string Notes { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
