namespace NutritionPlannerApp.Models
{
    public class FoodCompositionItem
    {
        public string Code { get; set; } = "";             // 整合編號
        public string Category { get; set; } = "";         // 食品分類
        public string Name { get; set; } = "";             // 樣品名稱
        public string Description { get; set; } = "";      // 內容物描述
        public string CommonName { get; set; } = "";       // 俗名
        public double WasteRate { get; set; }              // 廢棄率(%)
        public double Calories { get; set; }               // 熱量(kcal)
        public double ModifiedCalories { get; set; }       // 修正熱量(kcal)
        public double Water { get; set; }                  // 水分(g)
        public double Protein { get; set; }                // 粗蛋白(g)
        public double Fat { get; set; }                    // 粗脂肪(g)
        public double SaturatedFat { get; set; }           // 飽和脂肪(g)
        public double Carbs { get; set; }                  // 總碳水化合物(g)
        public double DietaryFiber { get; set; }           // 膳食纖維(g)
        public double Sugars { get; set; }                 // 糖質總量(g)
        public double Sodium { get; set; }                 // 鈉(mg)
        public double Potassium { get; set; }              // 鉀(mg)
        public double Calcium { get; set; }                // 鈣(mg)
        public double Magnesium { get; set; }              // 鎂(mg)
        public double Iron { get; set; }                   // 鐵(mg)
        public double Zinc { get; set; }                   // 鋅(mg)
        public double Phosphorus { get; set; }             // 磷(mg)
        public double Cholesterol { get; set; }            // 膽固醇(mg)
    }
}
