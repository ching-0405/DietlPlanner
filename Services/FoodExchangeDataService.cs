using System.Collections.Generic;
using NutritionPlannerApp.Models;

namespace NutritionPlannerApp.Services
{
    public static class FoodExchangeDataService
    {
        public static List<FoodExchangeReferenceItem> GetOfficialFoodDatabase()
        {
            var list = new List<FoodExchangeReferenceItem>
            {
                // ==========================================
                // 附-1 乳品類
                // ==========================================
                // 全脂 (蛋白質8, 脂肪8, 醣類12, 熱量150)
                new() { Category = "乳品類", SubCategory = "全脂奶類", FoodName = "全脂奶", PortionDesc = "1 杯 (240 毫升)", RawWeightGrams = 240, Calories = 150, Protein = 8, Fat = 8, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "全脂奶類", FoodName = "全脂奶粉", PortionDesc = "4 湯匙 (30 公克)", RawWeightGrams = 30, Calories = 150, Protein = 8, Fat = 8, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "全脂奶類", FoodName = "蒸發奶", PortionDesc = "1/2 杯 (120 毫升)", RawWeightGrams = 120, Calories = 150, Protein = 8, Fat = 8, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "全脂奶類", FoodName = "起司片", PortionDesc = "2 片 (45 公克)", RawWeightGrams = 45, Calories = 150, Protein = 8, Fat = 8, Carbs = 2.9, Notes = "醣類較低 2.9g" },
                new() { Category = "乳品類", SubCategory = "全脂奶類", FoodName = "乳酪絲", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 150, Protein = 8, Fat = 8, Carbs = 2.1, Notes = "醣類較低 2.1g" },

                // 低脂 (蛋白質8, 脂肪4, 醣類12, 熱量120)
                new() { Category = "乳品類", SubCategory = "低脂奶類", FoodName = "低脂奶", PortionDesc = "1 杯 (240 毫升)", RawWeightGrams = 240, Calories = 120, Protein = 8, Fat = 4, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "低脂奶類", FoodName = "低脂奶粉", PortionDesc = "3 湯匙 (25 公克)", RawWeightGrams = 25, Calories = 120, Protein = 8, Fat = 4, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "低脂奶類", FoodName = "優格 (無糖)", PortionDesc = "3/4 杯 (210 公克)", RawWeightGrams = 210, Calories = 120, Protein = 8, Fat = 4, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "低脂奶類", FoodName = "優酪乳 (無糖)", PortionDesc = "1 杯 (240 毫升)", RawWeightGrams = 240, Calories = 120, Protein = 8, Fat = 4, Carbs = 12 },

                // 脫脂 (蛋白質8, 脂肪0, 醣類12, 熱量80)
                new() { Category = "乳品類", SubCategory = "脫脂奶類", FoodName = "脫脂奶", PortionDesc = "1 杯 (240 毫升)", RawWeightGrams = 240, Calories = 80, Protein = 8, Fat = 0, Carbs = 12 },
                new() { Category = "乳品類", SubCategory = "脫脂奶類", FoodName = "脫脂奶粉", PortionDesc = "2.5 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 80, Protein = 8, Fat = 0, Carbs = 12 },

                // ==========================================
                // 附-2-1 豆、魚、蛋、肉類 (低脂，蛋白質7, 脂肪3以下, 熱量55)
                // ==========================================
                // 水產
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "蝦米", PortionDesc = "15 公克", RawWeightGrams = 15, Calories = 55, Protein = 7, Fat = 2, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "小魚干", PortionDesc = "10 公克", RawWeightGrams = 10, Calories = 55, Protein = 7, Fat = 2, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "蝦皮", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 55, Protein = 7, Fat = 2, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "魚脯", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "鰹魚、鮪魚", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "一般魚類", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "白鯧", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "蝦仁", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 55, Protein = 7, Fat = 1, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "小卷 (鹹)", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 55, Protein = 7, Fat = 1, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "花枝", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 55, Protein = 7, Fat = 1, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "章魚", PortionDesc = "55 公克", RawWeightGrams = 55, Calories = 55, Protein = 7, Fat = 1, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "魚丸 (不包肉)", PortionDesc = "55 公克", RawWeightGrams = 55, CookedWeightGrams = 55, Calories = 95, Protein = 7, Fat = 2, Carbs = 10, Notes = "含碳水化合物 10g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "牡蠣", PortionDesc = "生重 65g / 熟重 35g", RawWeightGrams = 65, CookedWeightGrams = 35, Calories = 55, Protein = 7, Fat = 2, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "文蛤", PortionDesc = "160 公克", RawWeightGrams = 160, Calories = 55, Protein = 7, Fat = 1, Carbs = 2, Notes = "文蛤 160 克為 1 份" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂水產", FoodName = "白海參", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 55, Protein = 7, Fat = 0, Carbs = 0 },

                // 家畜、家禽、內臟、蛋
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬大里肌 (瘦豬後腿肉、瘦前腿肉)", PortionDesc = "生重 35g / 熟重 30g", RawWeightGrams = 35, CookedWeightGrams = 30, Calories = 55, Protein = 7, Fat = 3, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "牛腱", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 55, Protein = 7, Fat = 3, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "牛肉干", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 75, Protein = 7, Fat = 2, Carbs = 5, Notes = "含碳水化合物 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬肉干", PortionDesc = "15 公克", RawWeightGrams = 15, Calories = 75, Protein = 7, Fat = 2, Carbs = 5, Notes = "含碳水化合物 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "火腿", PortionDesc = "45 公克", RawWeightGrams = 45, Calories = 75, Protein = 7, Fat = 3, Carbs = 5, Notes = "含碳水化合物 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "雞里肉、雞胸肉", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "雞腿 (去皮)", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 55, Protein = 7, Fat = 3, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "牛肚", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 55, Protein = 7, Fat = 2, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "雞肫", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 55, Protein = 7, Fat = 1, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬心", PortionDesc = "45 公克", RawWeightGrams = 45, Calories = 55, Protein = 7, Fat = 3, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬肝", PortionDesc = "生重 30g / 熟重 20g", RawWeightGrams = 30, CookedWeightGrams = 20, Calories = 55, Protein = 7, Fat = 2, Carbs = 1, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "雞肝", PortionDesc = "生重 40g / 熟重 30g", RawWeightGrams = 40, CookedWeightGrams = 30, Calories = 55, Protein = 7, Fat = 2, Carbs = 1, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "膽肝", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 55, Protein = 7, Fat = 2, Carbs = 1, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬腎 (豬腰)", PortionDesc = "45 公克", RawWeightGrams = 45, Calories = 55, Protein = 7, Fat = 2, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "豬血", PortionDesc = "110 公克", RawWeightGrams = 110, Calories = 55, Protein = 7, Fat = 0, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂肉類", FoodName = "雞蛋白", PortionDesc = "60 公克 (約 2 顆蛋白)", RawWeightGrams = 60, Calories = 55, Protein = 7, Fat = 0, Carbs = 0 },

                // 豆類及其製品
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "黃豆", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 75, Protein = 7, Fat = 3, Carbs = 5, Notes = "含碳水 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "黑豆", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 95, Protein = 7, Fat = 3, Carbs = 10, Notes = "含碳水 10g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "毛豆", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 75, Protein = 7, Fat = 3, Carbs = 5, Notes = "含碳水 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "豆包", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 55, Protein = 7, Fat = 3, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "干絲", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 55, Protein = 7, Fat = 3, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "臭豆腐", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 55, Protein = 7, Fat = 3, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "無糖豆漿", PortionDesc = "190 毫升", RawWeightGrams = 190, Calories = 55, Protein = 7, Fat = 3, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "麵腸", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 55, Protein = 7, Fat = 1, Carbs = 3 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "麵丸", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 55, Protein = 7, Fat = 1, Carbs = 3 },
                new() { Category = "豆魚蛋肉", SubCategory = "低脂豆類", FoodName = "烤麩", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 55, Protein = 7, Fat = 1, Carbs = 3 },

                // ==========================================
                // 附-2-2 豆、魚、蛋、肉類 (中脂，蛋白質7, 脂肪5, 熱量75)
                // ==========================================
                new() { Category = "豆魚蛋肉", SubCategory = "中脂水產", FoodName = "虱目魚、烏魚、肉鯽、鮭魚", PortionDesc = "生重 35g / 熟重 30g", RawWeightGrams = 35, CookedWeightGrams = 30, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂水產", FoodName = "魚肉鬆", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 115, Protein = 7, Fat = 5, Carbs = 10, Notes = "含碳水 10g" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂水產", FoodName = "鱈魚、比目魚", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂水產", FoodName = "虱目魚丸、花枝丸", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 105, Protein = 7, Fat = 5, Carbs = 7, Notes = "含碳水 7g" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂水產", FoodName = "旗魚丸、魚丸(包肉)", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 105, Protein = 7, Fat = 5, Carbs = 7, Notes = "含碳水 7g" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬大排、豬小排", PortionDesc = "生重 35g / 熟重 30g", RawWeightGrams = 35, CookedWeightGrams = 30, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬後腿肉、前腿肉、羊肉、豬腳", PortionDesc = "生重 35g / 熟重 30g", RawWeightGrams = 35, CookedWeightGrams = 30, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬肉鬆、肉脯", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 95, Protein = 7, Fat = 5, Carbs = 5, Notes = "含碳水 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "低脂培根", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "雞翅、雞排", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "雞爪", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "鴨賞", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬舌", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬肚", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 75, Protein = 7, Fat = 5, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬小腸", PortionDesc = "55 公克", RawWeightGrams = 55, Calories = 75, Protein = 7, Fat = 5, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂肉類", FoodName = "豬腦", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 75, Protein = 7, Fat = 5, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂蛋類", FoodName = "雞蛋", PortionDesc = "55 公克 (約 1 顆)", RawWeightGrams = 55, Calories = 75, Protein = 7, Fat = 5, Carbs = 0, Notes = "全蛋 1 顆 55g, 膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "豆枝", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 240, Protein = 7, Fat = 10, Carbs = 30, Notes = "+5g油脂 +30g碳水" },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "百頁結", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 75, Protein = 7, Fat = 5, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "油豆腐", PortionDesc = "55 公克", RawWeightGrams = 55, Calories = 75, Protein = 7, Fat = 5, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "豆豉", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 75, Protein = 7, Fat = 5, Carbs = 3 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "五香豆干", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 75, Protein = 7, Fat = 5, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "小方豆干", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 75, Protein = 7, Fat = 5, Carbs = 1 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "黃豆干", PortionDesc = "70 公克", RawWeightGrams = 70, Calories = 75, Protein = 7, Fat = 5, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "傳統豆腐", PortionDesc = "80 公克 (約 1/2 塊)", RawWeightGrams = 80, Calories = 75, Protein = 7, Fat = 5, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂豆類", FoodName = "嫩豆腐", PortionDesc = "140 公克 (1/2 盒)", RawWeightGrams = 140, Calories = 75, Protein = 7, Fat = 5, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂素料", FoodName = "素獅子頭", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 95, Protein = 7, Fat = 5, Carbs = 5 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂素料", FoodName = "素火腿", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 85, Protein = 7, Fat = 5, Carbs = 3 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂素料", FoodName = "素油雞", PortionDesc = "55 公克", RawWeightGrams = 55, Calories = 105, Protein = 7, Fat = 5, Carbs = 7 },
                new() { Category = "豆魚蛋肉", SubCategory = "中脂素料", FoodName = "素香鬆", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 125, Protein = 7, Fat = 5, Carbs = 12 },

                // ==========================================
                // 附-2-3 豆、魚、蛋、肉類 (高脂，蛋白質7, 脂肪10, 熱量120)
                // ==========================================
                new() { Category = "豆魚蛋肉", SubCategory = "高脂魚肉", FoodName = "秋刀魚", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 120, Protein = 7, Fat = 10, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂魚肉", FoodName = "牛肉條", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 120, Protein = 7, Fat = 10, Carbs = 0 },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂魚肉", FoodName = "豬肉酥", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 140, Protein = 7, Fat = 10, Carbs = 5, Notes = "含碳水 5g" },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂魚肉", FoodName = "雞心", PortionDesc = "45 公克", RawWeightGrams = 45, Calories = 120, Protein = 7, Fat = 10, Carbs = 0, Notes = "膽固醇 50-99mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂素料", FoodName = "素雞", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 120, Protein = 7, Fat = 10, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂素料", FoodName = "素魚", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 120, Protein = 7, Fat = 10, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂素料", FoodName = "素雞塊", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 150, Protein = 7, Fat = 10, Carbs = 7, Notes = "含碳水 7g" },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂素料", FoodName = "百頁豆腐", PortionDesc = "70 公克", RawWeightGrams = 70, Calories = 120, Protein = 7, Fat = 10, Carbs = 2 },
                new() { Category = "豆魚蛋肉", SubCategory = "高脂豆類", FoodName = "麵筋泡", PortionDesc = "15 公克", RawWeightGrams = 15, Calories = 120, Protein = 7, Fat = 10, Carbs = 2 },

                // 超高脂類 (應少食用)
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "豬蹄膀", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 135, Protein = 7, Fat = 12, Carbs = 0, Notes = "脂肪高應少食用" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "梅花肉", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 135, Protein = 7, Fat = 12, Carbs = 0, Notes = "脂肪高應少食用" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "牛腩", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 135, Protein = 7, Fat = 12, Carbs = 0, Notes = "脂肪高應少食用" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "豬大腸", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 135, Protein = 7, Fat = 12, Carbs = 0, Notes = "膽固醇 ≧ 100mg" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "香腸、蒜味香腸、五花臘肉", PortionDesc = "40 公克", RawWeightGrams = 40, Calories = 135, Protein = 7, Fat = 12, Carbs = 1, Notes = "加工肉品" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂肉類", FoodName = "熱狗、五花肉", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 135, Protein = 7, Fat = 12, Carbs = 1, Notes = "加工肉品" },
                new() { Category = "豆魚蛋肉", SubCategory = "超高脂素料", FoodName = "素肉燥", PortionDesc = "65 公克", RawWeightGrams = 65, Calories = 175, Protein = 7, Fat = 12, Carbs = 10, Notes = "含碳水 10g" },

                // ==========================================
                // 附-3 全榖雜糧類 (每份含蛋白質2, 醣類15, 熱量70)
                // ==========================================
                // 米類與主食
                new() { Category = "全穀雜糧", SubCategory = "米類", FoodName = "米、黑米、小米、糯米", PortionDesc = "1/8 杯 (米杯) (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米類", FoodName = "糙米、什穀米、胚芽米", PortionDesc = "1/8 杯 (米杯) (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米類", FoodName = "飯 (白飯、糙米飯)", PortionDesc = "1/4 碗 (40 公克)", RawWeightGrams = 40, Calories = 70, Protein = 2, Fat = 0, Carbs = 15, Notes = "1 碗飯 = 4 份主食 (160g)" },
                new() { Category = "全穀雜糧", SubCategory = "米類", FoodName = "稠粥", PortionDesc = "1/2 碗 (125 公克)", RawWeightGrams = 125, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米製品", FoodName = "白年糕", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米製品", FoodName = "芋頭糕", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米製品", FoodName = "蘿蔔糕", PortionDesc = "1 塊 (6x8x1.5 公分, 50 公克)", RawWeightGrams = 50, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米製品", FoodName = "豬血糕", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "米製品", FoodName = "小湯圓 (無餡)", PortionDesc = "約 10 粒 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },

                // 麥類與麵食
                new() { Category = "全穀雜糧", SubCategory = "麥類", FoodName = "大麥、小麥、蕎麥", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麥類", FoodName = "麥粉", PortionDesc = "4 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麥類", FoodName = "燕麥、麥片", PortionDesc = "3 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麥類", FoodName = "麵粉", PortionDesc = "3 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "麵條 (乾)", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "麵條 (濕)", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "麵條 (熟)", PortionDesc = "1/2 碗 (60 公克)", RawWeightGrams = 60, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "拉麵", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "油麵", PortionDesc = "1/2 碗 (45 公克)", RawWeightGrams = 45, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "鍋燒麵 (熟)", PortionDesc = "60 公克", RawWeightGrams = 60, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "通心粉 (乾)", PortionDesc = "1/3 杯 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2.5, Fat = 0, Carbs = 15, Notes = "蛋白質 2.5g" },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "義大利麵 (乾)、全麥", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 70, Protein = 2.7, Fat = 0, Carbs = 15, Notes = "蛋白質 2.7g" },
                new() { Category = "全穀雜糧", SubCategory = "麵食類", FoodName = "麵線 (乾)", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },

                // 麵皮與麵包點心
                new() { Category = "全穀雜糧", SubCategory = "麵食點心", FoodName = "水餃皮", PortionDesc = "3 張 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食點心", FoodName = "餛飩皮", PortionDesc = "3-7 張 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食點心", FoodName = "春捲皮", PortionDesc = "1 1/2 張 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食點心", FoodName = "饅頭", PortionDesc = "1/3 個 (中) (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵食點心", FoodName = "山東饅頭", PortionDesc = "1/6 個 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "吐司、全麥吐司", PortionDesc = "1/2 ~ 1/3 片 (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "餐包", PortionDesc = "1 個 (小) (30 公克)", RawWeightGrams = 30, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "漢堡麵包", PortionDesc = "1/2 個 (25 公克)", RawWeightGrams = 25, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "菠蘿麵包", PortionDesc = "1/3 個 (小) (30 公克)", RawWeightGrams = 30, Calories = 90, Protein = 2, Fat = 2.5, Carbs = 15, Notes = "+1 茶匙油" },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "奶酥麵包", PortionDesc = "1/3 個 (小) (30 公克)", RawWeightGrams = 30, Calories = 90, Protein = 2, Fat = 2.5, Carbs = 15, Notes = "+1 茶匙油" },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "蘇打餅干", PortionDesc = "3 片 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 1, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "燒餅", PortionDesc = "1/4 個 (20 公克)", RawWeightGrams = 20, Calories = 80, Protein = 2, Fat = 2.5, Carbs = 15, Notes = "+1/2 茶匙油" },
                new() { Category = "全穀雜糧", SubCategory = "麵包點心", FoodName = "油條", PortionDesc = "2/3 根 (40 公克)", RawWeightGrams = 40, Calories = 205, Protein = 2, Fat = 15, Carbs = 15, Notes = "+3 茶匙油" },
                new() { Category = "全穀雜糧", SubCategory = "澱粉點心", FoodName = "甜不辣", PortionDesc = "70 公克", RawWeightGrams = 70, Calories = 95, Protein = 8.8, Fat = 1, Carbs = 15, Notes = "蛋白質較高 8.8g" },

                // 根莖類與雜糧
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "馬鈴薯", PortionDesc = "1/2 個 (中) (90 公克)", RawWeightGrams = 90, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "蕃薯 (地瓜)", PortionDesc = "1/2 個 (小) (55 公克)", RawWeightGrams = 55, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "山藥", PortionDesc = "1 塊 (80 公克)", RawWeightGrams = 80, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "芋頭", PortionDesc = "1/5 個 (中) (滾刀塊 3-4 塊, 55 公克)", RawWeightGrams = 55, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "荸薺", PortionDesc = "8 粒 (100 公克)", RawWeightGrams = 100, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "根莖類", FoodName = "蓮藕", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "玉米、玉米粒", PortionDesc = "2/3 根 (85 公克)", RawWeightGrams = 85, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "爆米花 (不加油)", PortionDesc = "1 杯 (15 公克)", RawWeightGrams = 15, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "薏仁", PortionDesc = "1 1/2 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2.8, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "蓮子 (乾)", PortionDesc = "40 粒 (25 公克)", RawWeightGrams = 25, Calories = 70, Protein = 4.8, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "栗子 (乾)", PortionDesc = "3 粒 (大) (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "菱角", PortionDesc = "8 粒 (60 公克)", RawWeightGrams = 60, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "雜糧類", FoodName = "南瓜", PortionDesc = "85 公克", RawWeightGrams = 85, Calories = 70, Protein = 2, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "高蛋白乾豆", FoodName = "豌豆仁", PortionDesc = "70 公克", RawWeightGrams = 70, Calories = 70, Protein = 5.4, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "高蛋白乾豆", FoodName = "皇帝豆", PortionDesc = "65 公克", RawWeightGrams = 65, Calories = 70, Protein = 5.1, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "高蛋白乾豆", FoodName = "紅豆、綠豆、花豆", PortionDesc = "2 湯匙 (乾) (25 公克)", RawWeightGrams = 25, Calories = 70, Protein = 5.3, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "高蛋白乾豆", FoodName = "蠶豆、刀豆", PortionDesc = "2 湯匙 (乾) (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 4.9, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "高蛋白乾豆", FoodName = "鷹嘴豆", PortionDesc = "2 湯匙 (乾) (25 公克)", RawWeightGrams = 25, Calories = 70, Protein = 4.7, Fat = 0, Carbs = 15 },

                // 其他澱粉製品 (低蛋白質澱粉，腎臟病適用)
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "冬粉 (乾)", PortionDesc = "1/2 把 (15 公克)", RawWeightGrams = 15, Calories = 70, Protein = 0.02, Fat = 0, Carbs = 15, Notes = "低蛋白主食 蛋白質極微" },
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "藕粉", PortionDesc = "3 湯匙 (20 公克)", RawWeightGrams = 20, Calories = 70, Protein = 0.02, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "西谷米 (粉圓)", PortionDesc = "1 1/2 湯匙 (15 公克)", RawWeightGrams = 15, Calories = 70, Protein = 0.02, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "米苔目 (濕)", PortionDesc = "50 公克", RawWeightGrams = 50, Calories = 70, Protein = 0.3, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "米粉 (乾)", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 70, Protein = 0.1, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "低蛋白澱粉", FoodName = "米粉 (濕)", PortionDesc = "1/2 碗 (30~50 公克)", RawWeightGrams = 40, Calories = 70, Protein = 0.1, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "澱粉製品", FoodName = "芋圓、地瓜圓 (冷凍)", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 70, Protein = 0.5, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "澱粉製品", FoodName = "河粉 (濕)", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 70, Protein = 1, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "澱粉製品", FoodName = "越南春捲皮 (乾)", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 70, Protein = 0.5, Fat = 0, Carbs = 15 },
                new() { Category = "全穀雜糧", SubCategory = "澱粉製品", FoodName = "蛋餅皮、蔥油餅皮 (冷凍)", PortionDesc = "35 公克", RawWeightGrams = 35, Calories = 70, Protein = 2, Fat = 1, Carbs = 15 },

                // ==========================================
                // 附-4 蔬菜類 (每份可食100g, 蛋白質1, 醣類5, 熱量25)
                // ==========================================
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "黃豆芽", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2.5, Fat = 0, Carbs = 5, Notes = "蛋白質較高" },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "胡瓜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "葫蘆瓜、蒲瓜 (扁蒲)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "黑木耳", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "茭白筍", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "綠豆芽", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "蛋白質較高" },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "洋蔥", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "高麗菜 (甘藍、包心白菜)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "冬瓜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "玉米筍", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "小黃瓜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "苦瓜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "甜椒 (青椒)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "澎湖絲瓜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "胡蘿蔔", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "蘿蔔 (白蘿蔔)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "綠蘆筍、麻竹筍", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "小白菜、青江菜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "空心菜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "油菜、油菜花", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1.5, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "花椰菜 (白、綠花椰)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1.5, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "龍鬚菜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "蛋白質較高" },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "茄子", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "黃秋葵", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "番茄 (大番茄)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "香菇、柳松菇、洋菇、金針菇", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "蛋白質較高" },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "牛蒡、竹筍", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "低中高鉀蔬菜", FoodName = "芹菜、西洋芹", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 1, Fat = 0, Carbs = 5 },
                new() { Category = "蔬菜類", SubCategory = "高鉀蔬菜", FoodName = "地瓜葉 (番薯葉)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "蛋白質較高、高鉀" },
                new() { Category = "蔬菜類", SubCategory = "高鉀蔬菜", FoodName = "莧菜 (紅莧菜、白莧菜)", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "高鉀蔬菜" },
                new() { Category = "蔬菜類", SubCategory = "高鉀蔬菜", FoodName = "菠菜", PortionDesc = "100 公克", RawWeightGrams = 100, Calories = 25, Protein = 2, Fat = 0, Carbs = 5, Notes = "高鉀蔬菜、蛋白質較高" },

                // ==========================================
                // 附-5 水果類 (每份含醣類15, 熱量60)
                // ==========================================
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "油柑 (金棗)", PortionDesc = "6 個 (120 公克)", RawWeightGrams = 120, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "柳丁", PortionDesc = "1 個 (4 個/斤, 可食 130g)", RawWeightGrams = 130, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "香吉士", PortionDesc = "1 個 (可食 130g)", RawWeightGrams = 130, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "椪柑", PortionDesc = "1 個 (3 個/斤, 可食 150g)", RawWeightGrams = 150, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "桶柑 (海梨)", PortionDesc = "1 個 (4 個/斤, 可食 155g)", RawWeightGrams = 155, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "白柚", PortionDesc = "2 片 (可食 165g)", RawWeightGrams = 165, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "柑橘類", FoodName = "葡萄柚", PortionDesc = "3/4 個 (可食 165g)", RawWeightGrams = 165, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "蘋果類", FoodName = "青龍蘋果、五爪蘋果、富士蘋果", PortionDesc = "小 1 個 (可食 115~130g)", RawWeightGrams = 120, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "哈密瓜", PortionDesc = "1/4 個 (可食 150g)", RawWeightGrams = 150, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 ≧ 400mg" },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "木瓜", PortionDesc = "1/3 個 (可食 150g)", RawWeightGrams = 150, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "香瓜 (美濃)", PortionDesc = "2/3 個 (可食 165g)", RawWeightGrams = 165, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 ≧ 400mg" },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "紅西瓜", PortionDesc = "1 片 (可食 180g)", RawWeightGrams = 180, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "黃西瓜", PortionDesc = "1/3 個 (可食 195g)", RawWeightGrams = 195, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "瓜類", FoodName = "太陽瓜、新疆哈密瓜", PortionDesc = "可食 215~245g", RawWeightGrams = 230, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 ≧ 400mg" },
                new() { Category = "水果類", SubCategory = "芒果類", FoodName = "金煌芒果", PortionDesc = "1 片 (可食 105g)", RawWeightGrams = 105, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "芒果類", FoodName = "愛文芒果", PortionDesc = "1 1/2 片 (可食 150g)", RawWeightGrams = 150, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "芭樂類", FoodName = "葫蘆芭樂、土芭樂", PortionDesc = "1 個 (可食 155g)", RawWeightGrams = 155, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "芭樂類", FoodName = "泰國芭樂", PortionDesc = "1/3 個 (可食 160g)", RawWeightGrams = 160, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "梨類", FoodName = "西洋梨、粗梨、水梨", PortionDesc = "小 1 個 或 3/4 個 (可食 105~145g)", RawWeightGrams = 120, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "桃李類", FoodName = "水蜜桃、玫瑰桃", PortionDesc = "1 個 (可食 145g)", RawWeightGrams = 145, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "桃李類", FoodName = "桃子", PortionDesc = "1 個 (可食 220g)", RawWeightGrams = 220, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 ≧ 400mg" },
                new() { Category = "水果類", SubCategory = "桃李類", FoodName = "加州李、李子", PortionDesc = "小 1 個 ~ 4 個 (可食 120~145g)", RawWeightGrams = 130, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "棗柿類", FoodName = "紅棗、黑棗", PortionDesc = "9~10 個 (可食 25g)", RawWeightGrams = 25, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "棗柿類", FoodName = "綠棗子 (蜜棗)", PortionDesc = "2 個 (可食 130g)", RawWeightGrams = 130, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "棗柿類", FoodName = "柿餅", PortionDesc = "3/4 個 (可食 33g)", RawWeightGrams = 33, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "棗柿類", FoodName = "紅柿", PortionDesc = "3/4 個 (可食 100g)", RawWeightGrams = 100, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "榴槤", PortionDesc = "1/4 瓣 (可食 45g)", RawWeightGrams = 45, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "釋迦", PortionDesc = "1/2 個 (可食 60g)", RawWeightGrams = 60, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "香蕉", PortionDesc = "大 1/2 根 或 小 1 根 (購買 95g, 可食 70g)", RawWeightGrams = 70, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "櫻桃", PortionDesc = "9 個 (可食 80g)", RawWeightGrams = 80, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "山竹", PortionDesc = "5 個 (可食 84g)", RawWeightGrams = 84, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "葡萄", PortionDesc = "13 個 (可食 85g)", RawWeightGrams = 85, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "龍眼", PortionDesc = "13 個 (可食 90g)", RawWeightGrams = 90, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "荔枝", PortionDesc = "9 個 (可食 100g)", RawWeightGrams = 100, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "火龍果 (紅、白)", PortionDesc = "可食 110g", RawWeightGrams = 110, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "奇異果", PortionDesc = "1 1/2 個 (可食 105g)", RawWeightGrams = 105, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "鳳梨", PortionDesc = "1/10 片 (可食 110g)", RawWeightGrams = 110, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "百香果", PortionDesc = "2 個 (可食 140g)", RawWeightGrams = 140, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "枇杷", PortionDesc = "可食 155g", RawWeightGrams = 155, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "草莓", PortionDesc = "小 16 個 (可食 160g)", RawWeightGrams = 160, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "蓮霧", PortionDesc = "2 個 (可食 165g)", RawWeightGrams = 165, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "楊桃", PortionDesc = "3/4 個 (可食 170g)", RawWeightGrams = 170, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "其他水果", FoodName = "聖女小番茄", PortionDesc = "23 個 (可食 220g)", RawWeightGrams = 220, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "含鉀量 200~399mg" },

                // 果乾類
                new() { Category = "水果類", SubCategory = "果乾類", FoodName = "葡萄乾、蔓越莓乾、芒果乾、芭樂乾、無花果乾、鳳梨乾", PortionDesc = "20 公克", RawWeightGrams = 20, Calories = 60, Protein = 0, Fat = 0, Carbs = 15, Notes = "果乾含添加糖" },
                new() { Category = "水果類", SubCategory = "果乾類", FoodName = "龍眼乾", PortionDesc = "22 公克", RawWeightGrams = 22, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "果乾類", FoodName = "黑棗梅", PortionDesc = "25 公克", RawWeightGrams = 25, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },
                new() { Category = "水果類", SubCategory = "果乾類", FoodName = "芒果青", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 60, Protein = 0, Fat = 0, Carbs = 15 },

                // ==========================================
                // 附-6 油脂與堅果種子類 (每份含脂肪5, 熱量45)
                // ==========================================
                // 植物油
                new() { Category = "油脂與堅果種子", SubCategory = "植物油", FoodName = "大豆油、玉米油、花生油、葵花油、麻油、橄欖油、芥花油", PortionDesc = "1 茶匙 (5 公克/5 毫升)", RawWeightGrams = 5, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "植物油", FoodName = "椰漿", PortionDesc = "30 公克", RawWeightGrams = 30, Calories = 51, Protein = 0, Fat = 5, Carbs = 1.5, Notes = "+1.5g 碳水" },
                new() { Category = "油脂與堅果種子", SubCategory = "植物油", FoodName = "椰奶", PortionDesc = "55 公克", RawWeightGrams = 55, Calories = 53, Protein = 0, Fat = 5, Carbs = 2, Notes = "+2g 碳水" },

                // 動物油與其他
                new() { Category = "油脂與堅果種子", SubCategory = "動物油", FoodName = "牛油、豬油、雞油", PortionDesc = "1 茶匙 (5~6 公克)", RawWeightGrams = 5, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "培根", PortionDesc = "1 片 (25x3.5x0.1 公分, 15 公克)", RawWeightGrams = 15, Calories = 45, Protein = 1, Fat = 5, Carbs = 0, Notes = "蛋白質 ≧ 1g" },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "奶油乳酪 (cream cheese)", PortionDesc = "2 茶匙 (12 公克)", RawWeightGrams = 12, Calories = 45, Protein = 1, Fat = 5, Carbs = 0, Notes = "蛋白質 ≧ 1g" },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "瑪琪琳、酥油", PortionDesc = "1 茶匙 (6 公克)", RawWeightGrams = 6, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "蛋黃醬 (美乃滋)", PortionDesc = "1 茶匙 (8 公克)", RawWeightGrams = 8, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "沙拉醬 (法國式、義大利式)", PortionDesc = "2 茶匙 (10 公克)", RawWeightGrams = 10, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "花生醬", PortionDesc = "1 茶匙 (9 公克)", RawWeightGrams = 9, Calories = 45, Protein = 1, Fat = 5, Carbs = 0, Notes = "蛋白質 ≧ 1g" },
                new() { Category = "油脂與堅果種子", SubCategory = "加工油脂", FoodName = "鮮奶油", PortionDesc = "1 湯匙 (13 公克)", RawWeightGrams = 13, Calories = 45, Protein = 0, Fat = 5, Carbs = 0 },
                new() { Category = "油脂與堅果種子", SubCategory = "天然油脂", FoodName = "加州酪梨", PortionDesc = "2 湯匙 (1/6 個, 可食 40 公克)", RawWeightGrams = 40, Calories = 57, Protein = 0, Fat = 5, Carbs = 3, Notes = "+3g 碳水" },

                // 堅果類
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "瓜子", PortionDesc = "1 湯匙 (約 50 粒, 可食 15 公克)", RawWeightGrams = 15, Calories = 45, Protein = 4, Fat = 5, Carbs = 1, Notes = "蛋白質 4g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "南瓜子、葵花子", PortionDesc = "1 湯匙 (約 30 粒, 可食 10 公克)", RawWeightGrams = 10, Calories = 45, Protein = 2, Fat = 5, Carbs = 1, Notes = "蛋白質 2g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "各式花生仁", PortionDesc = "10 粒 (13 公克)", RawWeightGrams = 13, Calories = 45, Protein = 4, Fat = 5, Carbs = 1, Notes = "蛋白質 4g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "花生粉", PortionDesc = "2 湯匙 (13 公克)", RawWeightGrams = 13, Calories = 45, Protein = 4, Fat = 5, Carbs = 1, Notes = "蛋白質 4g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "黑(白)芝麻", PortionDesc = "4 茶匙 (10 公克)", RawWeightGrams = 10, Calories = 45, Protein = 1, Fat = 5, Carbs = 1, Notes = "蛋白質 1g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "杏仁果", PortionDesc = "5 粒 (7 公克)", RawWeightGrams = 7, Calories = 45, Protein = 2, Fat = 5, Carbs = 1, Notes = "蛋白質 2g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "腰果", PortionDesc = "5 粒 (10 公克)", RawWeightGrams = 10, Calories = 45, Protein = 2, Fat = 5, Carbs = 1, Notes = "蛋白質 2g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "開心果", PortionDesc = "15 粒 (可食 10 公克)", RawWeightGrams = 10, Calories = 45, Protein = 2, Fat = 5, Carbs = 1, Notes = "蛋白質 2g" },
                new() { Category = "油脂與堅果種子", SubCategory = "堅果類", FoodName = "核桃仁", PortionDesc = "2 粒 (7 公克)", RawWeightGrams = 7, Calories = 45, Protein = 1, Fat = 5, Carbs = 1, Notes = "蛋白質 1g" }
            };

            return list;
        }
    }
}
