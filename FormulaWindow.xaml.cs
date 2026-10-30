using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NutritionPlannerApp.Models;
using NutritionPlannerApp.Services;

namespace NutritionPlannerApp
{
    public partial class FormulaWindow : Window
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<CommercialFormula> _formulasList;
        private CommercialFormula? _selectedFormula = null;

        public bool DataChanged { get; private set; } = false;

        public FormulaWindow(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            _formulasList = new ObservableCollection<CommercialFormula>(_dbService.GetAllFormulas());
            GridFormulas.ItemsSource = _formulasList;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("請輸入配方品名！", "提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double.TryParse(TxtProtein.Text, out double protein);
            double.TryParse(TxtFat.Text, out double fat);
            double.TryParse(TxtCarbs.Text, out double carbs);
            double.TryParse(TxtCalories.Text, out double calories);

            // 如果熱量為 0，可自動由三大營養素估算
            if (calories <= 0)
            {
                calories = (protein * 4) + (fat * 9) + (carbs * 4);
            }

            var formula = _selectedFormula ?? new CommercialFormula();
            formula.Name = name;
            formula.Spec = TxtSpec.Text.Trim();
            formula.Protein = protein;
            formula.Fat = fat;
            formula.Carbs = carbs;
            formula.Calories = calories;
            formula.Notes = TxtNotes.Text.Trim();

            _dbService.SaveFormula(formula);
            DataChanged = true;

            ReloadList();
            ClearForm();
            MessageBox.Show("配方已成功儲存！✨", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedFormula = null;
            TxtName.Text = "";
            TxtSpec.Text = "";
            TxtProtein.Text = "0";
            TxtFat.Text = "0";
            TxtCarbs.Text = "0";
            TxtCalories.Text = "0";
            TxtNotes.Text = "";
            GridFormulas.SelectedItem = null;
        }

        private void ReloadList()
        {
            _formulasList = new ObservableCollection<CommercialFormula>(_dbService.GetAllFormulas());
            GridFormulas.ItemsSource = _formulasList;
        }

        private void GridFormulas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridFormulas.SelectedItem is CommercialFormula item)
            {
                _selectedFormula = item;
                TxtName.Text = item.Name;
                TxtSpec.Text = item.Spec;
                TxtProtein.Text = item.Protein.ToString("0.#");
                TxtFat.Text = item.Fat.ToString("0.#");
                TxtCarbs.Text = item.Carbs.ToString("0.#");
                TxtCalories.Text = item.Calories.ToString("0.#");
                TxtNotes.Text = item.Notes;
            }
        }

        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CommercialFormula item)
            {
                var result = MessageBox.Show($"確定要刪除「{item.Name}」配方嗎？", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _dbService.DeleteFormula(item.Id);
                    DataChanged = true;
                    ReloadList();
                    ClearForm();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
