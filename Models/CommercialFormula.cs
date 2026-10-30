using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutritionPlannerApp.Models
{
    public class CommercialFormula : INotifyPropertyChanged
    {
        private int _id;
        private string _name = "";
        private string _spec = "";
        private double _protein;
        private double _fat;
        private double _carbs;
        private double _calories;
        private double _actualPortions;
        private double _suggestedPortions;
        private string _notes = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Spec
        {
            get => _spec;
            set { _spec = value; OnPropertyChanged(); }
        }

        public double Protein
        {
            get => _protein;
            set { _protein = value; OnPropertyChanged(); Recalculate(); }
        }

        public double Fat
        {
            get => _fat;
            set { _fat = value; OnPropertyChanged(); Recalculate(); }
        }

        public double Carbs
        {
            get => _carbs;
            set { _carbs = value; OnPropertyChanged(); Recalculate(); }
        }

        public double Calories
        {
            get => _calories;
            set { _calories = value; OnPropertyChanged(); Recalculate(); }
        }

        public double ActualPortions
        {
            get => _actualPortions;
            set { _actualPortions = Math.Max(0, value); OnPropertyChanged(); Recalculate(); }
        }

        public double SuggestedPortions
        {
            get => _suggestedPortions;
            set { _suggestedPortions = Math.Max(0, value); OnPropertyChanged(); Recalculate(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public double ActualTotalProtein => Math.Round(ActualPortions * Protein, 1);
        public double ActualTotalFat => Math.Round(ActualPortions * Fat, 1);
        public double ActualTotalCarbs => Math.Round(ActualPortions * Carbs, 1);
        public double ActualTotalCalories => Math.Round(ActualPortions * Calories, 1);

        public double SuggestedTotalProtein => Math.Round(SuggestedPortions * Protein, 1);
        public double SuggestedTotalFat => Math.Round(SuggestedPortions * Fat, 1);
        public double SuggestedTotalCarbs => Math.Round(SuggestedPortions * Carbs, 1);
        public double SuggestedTotalCalories => Math.Round(SuggestedPortions * Calories, 1);

        public void Recalculate()
        {
            OnPropertyChanged(nameof(ActualTotalProtein));
            OnPropertyChanged(nameof(ActualTotalFat));
            OnPropertyChanged(nameof(ActualTotalCarbs));
            OnPropertyChanged(nameof(ActualTotalCalories));
            OnPropertyChanged(nameof(SuggestedTotalProtein));
            OnPropertyChanged(nameof(SuggestedTotalFat));
            OnPropertyChanged(nameof(SuggestedTotalCarbs));
            OnPropertyChanged(nameof(SuggestedTotalCalories));
        }
    }
}
