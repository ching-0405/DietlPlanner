using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionPlannerApp.Models
{
    public class FoodExchangeItem : INotifyPropertyChanged
    {
        private string _category = "";
        private string _icon = "🥗";
        private string _unitName = "份";
        private double _proteinPerUnit;
        private double _fatPerUnit;
        private double _carbPerUnit;
        private double _caloriesPerUnit;
        
        // 實際飲食份數
        private double _actualPortions;
        
        // 建議飲食各餐份數
        private double _breakfast;
        private double _morningSnack;
        private double _lunch;
        private double _afternoonSnack;
        private double _dinner;
        private double _nightSnack;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        public string UnitName
        {
            get => _unitName;
            set { _unitName = value; OnPropertyChanged(); }
        }

        public double ProteinPerUnit
        {
            get => _proteinPerUnit;
            set { _proteinPerUnit = value; OnPropertyChanged(); Recalculate(); }
        }

        public double FatPerUnit
        {
            get => _fatPerUnit;
            set { _fatPerUnit = value; OnPropertyChanged(); Recalculate(); }
        }

        public double CarbPerUnit
        {
            get => _carbPerUnit;
            set { _carbPerUnit = value; OnPropertyChanged(); Recalculate(); }
        }

        public double CaloriesPerUnit
        {
            get => _caloriesPerUnit;
            set { _caloriesPerUnit = value; OnPropertyChanged(); Recalculate(); }
        }

        // 實際飲食
        public double ActualPortions
        {
            get => _actualPortions;
            set { _actualPortions = Math.Max(0, value); OnPropertyChanged(); Recalculate(); }
        }

        public double ActualProtein => Math.Round(ActualPortions * ProteinPerUnit, 1);
        public double ActualFat => Math.Round(ActualPortions * FatPerUnit, 1);
        public double ActualCarbs => Math.Round(ActualPortions * CarbPerUnit, 1);
        public double ActualCalories => Math.Round(ActualPortions * CaloriesPerUnit, 1);

        // 建議飲食各餐
        public double Breakfast
        {
            get => _breakfast;
            set { _breakfast = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double MorningSnack
        {
            get => _morningSnack;
            set { _morningSnack = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double Lunch
        {
            get => _lunch;
            set { _lunch = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double AfternoonSnack
        {
            get => _afternoonSnack;
            set { _afternoonSnack = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double Dinner
        {
            get => _dinner;
            set { _dinner = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double NightSnack
        {
            get => _nightSnack;
            set { _nightSnack = Math.Max(0, value); OnPropertyChanged(); RecalculateSuggested(); }
        }

        public double SuggestedTotalPortions => Math.Round(Breakfast + MorningSnack + Lunch + AfternoonSnack + Dinner + NightSnack, 2);
        public double SuggestedProtein => Math.Round(SuggestedTotalPortions * ProteinPerUnit, 1);
        public double SuggestedFat => Math.Round(SuggestedTotalPortions * FatPerUnit, 1);
        public double SuggestedCarbs => Math.Round(SuggestedTotalPortions * CarbPerUnit, 1);
        public double SuggestedCalories => Math.Round(SuggestedTotalPortions * CaloriesPerUnit, 1);

        public void Recalculate()
        {
            OnPropertyChanged(nameof(ActualProtein));
            OnPropertyChanged(nameof(ActualFat));
            OnPropertyChanged(nameof(ActualCarbs));
            OnPropertyChanged(nameof(ActualCalories));
            RecalculateSuggested();
        }

        public void RecalculateSuggested()
        {
            OnPropertyChanged(nameof(SuggestedTotalPortions));
            OnPropertyChanged(nameof(SuggestedProtein));
            OnPropertyChanged(nameof(SuggestedFat));
            OnPropertyChanged(nameof(SuggestedCarbs));
            OnPropertyChanged(nameof(SuggestedCalories));
        }
    }
}
