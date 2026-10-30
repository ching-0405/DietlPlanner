using System;

namespace NutritionPlannerApp.Services
{
    public static class NutritionCalculator
    {
        // 1. BMI 計算
        public static double CalculateBmi(double weightKg, double heightCm)
        {
            if (heightCm <= 0 || weightKg <= 0) return 0;
            double heightM = heightCm / 100.0;
            return Math.Round(weightKg / (heightM * heightM), 2);
        }

        // 2. 理想體重 (IBW = 22 * BH^2)
        public static double CalculateIbw(double heightCm)
        {
            if (heightCm <= 0) return 0;
            double heightM = heightCm / 100.0;
            return Math.Round(22.0 * heightM * heightM, 2);
        }

        // 理想體重百分比 IBW%
        public static double CalculateIbwPercent(double weightKg, double heightCm)
        {
            double ibw = CalculateIbw(heightCm);
            if (ibw <= 0) return 0;
            return Math.Round((weightKg / ibw) * 100.0, 1);
        }

        // BMI 分級與說明
        public static (string Category, string ColorHex, string Advice) GetBmiStatus(double bmi)
        {
            if (bmi < 18.5)
                return ("體重過輕", "#3A86FF", "建議適度增加熱量與優質蛋白質攝取，加強肌力訓練。");
            if (bmi < 24.0)
                return ("體重正常 (理想)", "#38B000", "體態維持優良！請繼續保持均衡飲食與規律運動。");
            if (bmi < 27.0)
                return ("體重過重", "#FB8500", "建議適度控制精製糖及高脂食物，增加高纖蔬菜。");
            return ("肥胖", "#E63946", "建議採熱量赤字控制，減少飽和脂肪，尋求營養師客製減重菜單。");
        }

        // 3. 膝長推算身高 (台灣長者/成年人常模)
        // 男: 85.10 + (1.73 * 膝長) - (0.11 * 年齡)
        // 女: 91.45 + (1.53 * 膝長) - (0.16 * 年齡)
        public static double EstimateHeightFromKnee(bool isFemale, double kneeCm, double age)
        {
            if (kneeCm <= 0) return 0;
            if (isFemale)
            {
                return Math.Round(91.45 + (1.53 * kneeCm) - (0.16 * age), 2);
            }
            else
            {
                return Math.Round(85.10 + (1.73 * kneeCm) - (0.11 * age), 2);
            }
        }

        // 4. 尺骨長推算身高 (BAPEN MUST / 台灣常模)
        public static double EstimateHeightFromUlna(bool isFemale, double ulnaCm, double age)
        {
            if (ulnaCm <= 0) return 0;
            if (isFemale)
            {
                if (age >= 65)
                    return Math.Round(90.6 + (2.98 * ulnaCm) - (0.15 * (age - 65)), 2);
                else
                    return Math.Round(84.5 + (3.21 * ulnaCm), 2);
            }
            else
            {
                if (age >= 65)
                    return Math.Round(86.3 + (3.15 * ulnaCm), 2);
                else
                    return Math.Round(79.2 + (3.60 * ulnaCm), 2);
            }
        }

        // 5. 臂圍與臀圍推估體重
        public static double EstimateWeightFromCircumference(bool isFemale, double hipCm, double armCm)
        {
            if (hipCm <= 0 || armCm <= 0) return 0;
            if (isFemale)
            {
                // 女: 0.44 * 臀圍 + 0.72 * 臂圍 - 2.03
                return Math.Round((hipCm * 0.44) + (armCm * 0.72) - 2.03, 2);
            }
            else
            {
                return Math.Round((hipCm * 0.45) + (armCm * 0.75) - 1.50, 2);
            }
        }

        // 6. eGFR (MDRD 簡化公式)
        // eGFR = 186 * (Cr ^ -1.154) * (Age ^ -0.203) * (0.742 if 女性 else 1.0)
        public static double CalculateEgfr(bool isFemale, double age, double creatinine)
        {
            if (age <= 0 || creatinine <= 0) return 0;
            double genderMultiplier = isFemale ? 0.742 : 1.0;
            double egfr = 186.0 * Math.Pow(creatinine, -1.154) * Math.Pow(age, -0.203) * genderMultiplier;
            return Math.Round(egfr, 1);
        }

        public static (string Stage, string Desc, string NutritionAdvice) GetCkdStage(double egfr)
        {
            if (egfr >= 90)
                return ("第 1 期 (G1)", "腎功能正常或微量蛋白尿", "維持健康生活型態、規律喝水、避免高鈉與亂服成藥。");
            if (egfr >= 60)
                return ("第 2 期 (G2)", "腎功能輕度受損", "定期追蹤腎功能，控制血壓與血糖，避免過量蛋白質攝取。");
            if (egfr >= 45)
                return ("第 3a 期 (G3a)", "腎功能中度受損", "建議開始適度低蛋白飲食 (0.8 g/kg/天)，注意水鈉與磷酸鹽控制。");
            if (egfr >= 30)
                return ("第 3b 期 (G3b)", "腎功能中重度受損", "嚴格低蛋白飲食 (0.6-0.8 g/kg/天)，搭配低氮澱粉與商業低蛋白配方，監控鉀與磷。");
            if (egfr >= 15)
                return ("第 4 期 (G4)", "腎功能重度受損", "低蛋白飲食 (0.6 g/kg/天) + 酮酸胺基酸，限制高鉀高磷食物，預防尿毒症。");
            return ("第 5 期 (G5)", "末期腎衰竭 (洗腎前/透析中)", "若未洗腎需極低蛋白；若已規則血液透析則需調高蛋白質 (1.2-1.4 g/kg/天)！");
        }

        // 7. Harris-Benedict BEE 基礎代謝率
        // 男: 66 + (13.7 * W) + (5.0 * H) - (6.8 * A)
        // 女: 655 + (9.6 * W) + (1.8 * H) - (4.7 * A)
        public static double CalculateBee(bool isFemale, double weightKg, double heightCm, double age)
        {
            if (weightKg <= 0 || heightCm <= 0 || age <= 0) return 0;
            if (isFemale)
            {
                return Math.Round(655.0 + (9.6 * weightKg) + (1.8 * heightCm) - (4.7 * age), 1);
            }
            else
            {
                return Math.Round(66.0 + (13.7 * weightKg) + (5.0 * heightCm) - (6.8 * age), 1);
            }
        }

        // TDEE = BEE * 活動因子 * 壓力因子
        public static double CalculateTdee(double bee, double activityFactor, double stressFactor)
        {
            if (bee <= 0) return 0;
            activityFactor = Math.Max(1.0, activityFactor);
            stressFactor = Math.Max(0.5, stressFactor);
            return Math.Round(bee * activityFactor * stressFactor, 0);
        }

        // 8. 三大營養素克數計算
        public static (double CarbGrams, double FatGrams, double ProteinGrams) CalculateMacroGrams(
            double totalCalories, double carbRatioPercent, double fatRatioPercent, double proteinRatioPercent)
        {
            if (totalCalories <= 0) return (0, 0, 0);
            double carbCal = totalCalories * (carbRatioPercent / 100.0);
            double fatCal = totalCalories * (fatRatioPercent / 100.0);
            double proteinCal = totalCalories * (proteinRatioPercent / 100.0);

            double carbG = Math.Round(carbCal / 4.0, 1);
            double fatG = Math.Round(fatCal / 9.0, 1);
            double proteinG = Math.Round(proteinCal / 4.0, 1);

            return (carbG, fatG, proteinG);
        }
    }
}
