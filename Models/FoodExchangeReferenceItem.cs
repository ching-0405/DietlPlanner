using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionPlannerApp.Models
{
    public class FoodExchangeReferenceItem : INotifyPropertyChanged
    {
        private string _category = "";
        private string _subCategory = "";
        private string _foodName = "";
        private string _portionDesc = "";
        private double _rawWeightGrams;
        private double _cookedWeightGrams;
        private double _calories;
        private double _protein;
        private double _fat;
        private double _carbs;
        private string _notes = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public string SubCategory
        {
            get => _subCategory;
            set { _subCategory = value; OnPropertyChanged(); }
        }

        public string FoodName
        {
            get => _foodName;
            set { _foodName = value; OnPropertyChanged(); }
        }

        public string PortionDesc
        {
            get => _portionDesc;
            set { _portionDesc = value; OnPropertyChanged(); }
        }

        public double RawWeightGrams
        {
            get => _rawWeightGrams;
            set { _rawWeightGrams = value; OnPropertyChanged(); }
        }

        public double CookedWeightGrams
        {
            get => _cookedWeightGrams;
            set { _cookedWeightGrams = value; OnPropertyChanged(); }
        }

        public double Calories
        {
            get => _calories;
            set { _calories = value; OnPropertyChanged(); }
        }

        public double Protein
        {
            get => _protein;
            set { _protein = value; OnPropertyChanged(); }
        }

        public double Fat
        {
            get => _fat;
            set { _fat = value; OnPropertyChanged(); }
        }

        public double Carbs
        {
            get => _carbs;
            set { _carbs = value; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        // 方便 UI 顯示的計算或重量字串
        public string PortionWeightDisplay =>
            RawWeightGrams > 0 ? $"{RawWeightGrams:0.#} g" : (CookedWeightGrams > 0 ? $"{CookedWeightGrams:0.#} g (熟)" : PortionDesc);
    }
}
