using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionPlannerApp.Models
{
    public class MenuItemDetail : INotifyPropertyChanged
    {
        private string _mealTime = "早餐";
        private string _mealPlannedSummary = "";
        private string _dishName = "";
        private string _ingredientName = "";
        private string _category = "米食類";
        private double _exchangePortion = 1.0;
        private double _rawWeightGrams = 50.0;
        private string _foodColor = "白色";
        private string _colorHex = "#F8F9FA";
        private string _notes = "";

        // 合併儲存格專用屬性
        private bool _isFirstInDishGroup = true;
        private bool _isLastInDishGroup = true;
        private int _dishGroupSpan = 1;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string MealTime
        {
            get => _mealTime;
            set { _mealTime = value; OnPropertyChanged(); }
        }

        public string MealPlannedSummary
        {
            get => _mealPlannedSummary;
            set { _mealPlannedSummary = value; OnPropertyChanged(); }
        }

        public string DishName
        {
            get => _dishName;
            set { _dishName = value; OnPropertyChanged(); }
        }

        public string IngredientName
        {
            get => _ingredientName;
            set
            {
                _ingredientName = value;
                OnPropertyChanged();
                AutoRecognizeColor();
            }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public double ExchangePortion
        {
            get => _exchangePortion;
            set { _exchangePortion = value; OnPropertyChanged(); }
        }

        public double RawWeightGrams
        {
            get => _rawWeightGrams;
            set { _rawWeightGrams = value; OnPropertyChanged(); }
        }

        public string FoodColor
        {
            get => _foodColor;
            set { _foodColor = value; OnPropertyChanged(); }
        }

        public string ColorHex
        {
            get => _colorHex;
            set { _colorHex = value; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public bool IsFirstInDishGroup
        {
            get => _isFirstInDishGroup;
            set { _isFirstInDishGroup = value; OnPropertyChanged(); }
        }

        public bool IsLastInDishGroup
        {
            get => _isLastInDishGroup;
            set { _isLastInDishGroup = value; OnPropertyChanged(); }
        }

        public int DishGroupSpan
        {
            get => _dishGroupSpan;
            set { _dishGroupSpan = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 智慧AI食材顏色與色碼自動辨識引擎
        /// </summary>
        public void AutoRecognizeColor()
        {
            if (string.IsNullOrWhiteSpace(_ingredientName)) return;

            string name = _ingredientName.Trim().ToLower();

            // 0. 特殊蔬菜專用指定色
            if (name.Contains("小白菜"))
            {
                FoodColor = "嫩綠色";
                ColorHex = "#A8D878";
                return;
            }

            if (name.Contains("高麗菜") || name.Contains("甘藍") || name.Contains("包心菜"))
            {
                FoodColor = "粉綠/嫩綠";
                ColorHex = "#D5E8B3";
                return;
            }

            // 1. 橙色 (Orange)
            if (name.Contains("胡蘿蔔") || name.Contains("紅蘿蔔") || name.Contains("南瓜") || 
                name.Contains("木瓜") || name.Contains("柳丁") || name.Contains("橘子") || 
                name.Contains("柑橘") || name.Contains("柿子") || name.Contains("哈密瓜") || 
                name.Contains("鮭魚") || name.Contains("地瓜") || name.Contains("黃肉地瓜") ||
                name.Contains("金針花") || name.Contains("蛋黃") || name.Contains("橙"))
            {
                FoodColor = "橙色";
                ColorHex = "#F97316";
                return;
            }

            // 2. 黃色 (Yellow)
            if (name.Contains("香蕉") || name.Contains("玉米") || name.Contains("黃豆") || 
                name.Contains("檸檬") || name.Contains("鳳梨") || name.Contains("黃椒") || 
                name.Contains("彩椒(黃)") || name.Contains("楊桃") || name.Contains("小米") || 
                name.Contains("薑黃") || name.Contains("生薑") || name.Contains("波羅蜜") ||
                name.Contains("芥末") || name.Contains("起司") || name.Contains("乳酪") ||
                name.Contains("黃瓜") && !name.Contains("小黃瓜"))
            {
                FoodColor = "黃色";
                ColorHex = "#EAB308";
                return;
            }

            // 3. 紅色 (Red)
            if (name.Contains("番茄") || name.Contains("紅椒") || name.Contains("甜椒(紅)") || 
                name.Contains("辣椒") || name.Contains("草莓") || name.Contains("西瓜") || 
                name.Contains("蘋果") || name.Contains("櫻桃") || name.Contains("牛肉") || 
                name.Contains("豬肉") || name.Contains("羊肉") || name.Contains("紅豆") || 
                name.Contains("紅麴") || name.Contains("洛神花") || name.Contains("蔓越莓") ||
                name.Contains("紅棗") || name.Contains("枸杞") || name.Contains("火龍果(紅)"))
            {
                FoodColor = "紅色";
                ColorHex = "#EF4444";
                return;
            }

            // 4. 綠色 (Green) - 包含白菜、小白菜、青江菜、菠菜等各類綠葉蔬菜
            if (name.Contains("青菜") || name.Contains("蔬菜") || name.Contains("時蔬") ||
                name.Contains("小白菜") || name.Contains("白菜") || name.Contains("菠菜") || name.Contains("青江菜") || 
                name.Contains("空心菜") || (name.Contains("花椰菜") && !name.Contains("白花")) || 
                name.Contains("青花菜") || name.Contains("小黃瓜") || name.Contains("芹菜") || 
                name.Contains("萵苣") || name.Contains("地瓜葉") || name.Contains("青椒") || 
                name.Contains("毛豆") || name.Contains("綠豆") || name.Contains("奇異果") || 
                name.Contains("芭樂") || name.Contains("蔥") || name.Contains("九層塔") || 
                name.Contains("芥蘭") || name.Contains("芥菜") || name.Contains("茼蒿") ||
                name.Contains("蘆筍") || name.Contains("絲瓜") || name.Contains("苦瓜") ||
                name.Contains("秋葵") || name.Contains("酪梨") || name.Contains("綠茶") ||
                name.Contains("抹茶") || name.Contains("皇冠菜") || name.Contains("莧菜") ||
                name.Contains("油菜") || name.Contains("韭菜") || name.Contains("豆苗") ||
                name.Contains("龍鬚菜") || name.Contains("水蓮") || name.Contains("山蘇") ||
                name.Contains("過貓") || name.Contains("川七") || name.Contains("綠蘆筍") ||
                name.Contains("筊白筍葉") || name.Contains("荷蘭豆") || name.Contains("甜豆") ||
                name.Contains("四季豆") || name.Contains("菜豆") || name.Contains("敏豆"))
            {
                FoodColor = "綠色";
                ColorHex = "#22C55E";
                return;
            }

            // 5. 紫色/黑/藍 (Purple / Black)
            if (name.Contains("茄子") || name.Contains("紫甘藍") || name.Contains("紫洋蔥") || 
                name.Contains("葡萄") || name.Contains("藍莓") || name.Contains("桑椹") || 
                name.Contains("紫米") || name.Contains("黑米") || name.Contains("黑豆") || 
                name.Contains("黑木耳") || name.Contains("木耳") || name.Contains("紫菜") || 
                name.Contains("海苔") || name.Contains("海帶") || name.Contains("黑芝麻") ||
                name.Contains("香菇") || name.Contains("紫地瓜"))
            {
                FoodColor = "紫色/黑色";
                ColorHex = "#8B5CF6";
                return;
            }

            // 6. 白色 (White)
            if (name.Contains("米") || name.Contains("飯") || name.Contains("麵") || 
                name.Contains("豆腐") || name.Contains("豆漿") || name.Contains("牛奶") || 
                name.Contains("白蘿蔔") || name.Contains("洋蔥") || name.Contains("大蒜") || 
                name.Contains("蒜") || name.Contains("山藥") || name.Contains("白花椰") || 
                name.Contains("冬瓜") || name.Contains("竹筍") || name.Contains("筊白筍") || 
                name.Contains("杏鮑菇") || name.Contains("金針菇") || name.Contains("銀耳") || 
                name.Contains("白木耳") || name.Contains("雞肉") || name.Contains("雞胸") || 
                name.Contains("魚") || name.Contains("蛋白") || name.Contains("燕麥") ||
                name.Contains("水梨") || name.Contains("白芝麻") || name.Contains("馬鈴薯") ||
                name.Contains("蓮藕"))
            {
                FoodColor = "白色";
                ColorHex = "#94A3B8";
                return;
            }

            // 7. 棕褐色 (Brown)
            if (name.Contains("堅果") || name.Contains("核桃") || name.Contains("杏仁") || 
                name.Contains("腰果") || name.Contains("花生") || name.Contains("全麥") || 
                name.Contains("糙米") || name.Contains("蕎麥") || name.Contains("咖啡") || 
                name.Contains("可可") || name.Contains("巧克力") || name.Contains("牛肉") ||
                name.Contains("茶葉蛋"))
            {
                FoodColor = "棕褐色";
                ColorHex = "#A16207";
                return;
            }

            // 預設：若分類是蔬菜類，絕不顯示天然原色，自動辨識為綠色蔬菜
            if (_category.Contains("蔬菜") || name.Contains("菜") || name.Contains("葉") || name.Contains("苗") || name.Contains("筍"))
            {
                FoodColor = "綠色";
                ColorHex = "#22C55E";
                return;
            }

            // 預設其他食材
            FoodColor = "白色";
            ColorHex = "#94A3B8";
        }
    }
}