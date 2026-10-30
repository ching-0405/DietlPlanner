using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NutritionPlannerApp.Models;

namespace NutritionPlannerApp
{
    public partial class FoodExchangeQueryWindow : Window
    {
        private readonly List<FoodExchangeReferenceItem> _allFoods;
        private readonly ObservableCollection<FoodExchangeReferenceItem> _filteredFoods;

        public event Action<FoodExchangeReferenceItem>? OnItemSelected;

        public FoodExchangeQueryWindow(List<FoodExchangeReferenceItem> allFoods)
        {
            InitializeComponent();
            _allFoods = allFoods ?? new List<FoodExchangeReferenceItem>();
            _filteredFoods = new ObservableCollection<FoodExchangeReferenceItem>(_allFoods);
            GridOfficialFoods.ItemsSource = _filteredFoods;
            UpdateCount();
        }

        private void TxtFoodSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CboFoodCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtFoodSearch.Text = "";
            CboFoodCategoryFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allFoods == null || _filteredFoods == null) return;

            string keyword = TxtFoodSearch.Text.Trim().ToLower();
            string selectedCat = "全部類別";
            if (CboFoodCategoryFilter?.SelectedItem is ComboBoxItem catItem && catItem.Content != null)
            {
                selectedCat = catItem.Content.ToString() ?? "全部類別";
            }

            var results = _allFoods.Where(f =>
            {
                bool matchCat = selectedCat == "全部類別" || f.Category == selectedCat;
                bool matchKeyword = string.IsNullOrWhiteSpace(keyword) ||
                                    f.FoodName.ToLower().Contains(keyword) ||
                                    f.SubCategory.ToLower().Contains(keyword) ||
                                    f.PortionDesc.ToLower().Contains(keyword) ||
                                    (!string.IsNullOrEmpty(f.Notes) && f.Notes.ToLower().Contains(keyword));
                return matchCat && matchKeyword;
            }).ToList();

            _filteredFoods.Clear();
            foreach (var item in results)
            {
                _filteredFoods.Add(item);
            }
            UpdateCount();
        }

        private void UpdateCount()
        {
            if (TxtResultCount != null && _filteredFoods != null)
            {
                TxtResultCount.Text = $"共有 {_filteredFoods.Count} 項食物代換資料";
            }
        }

        private void BtnAddToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (GridOfficialFoods.SelectedItem is FoodExchangeReferenceItem selected)
            {
                OnItemSelected?.Invoke(selected);
                MessageBox.Show($"已將「{selected.FoodName}」帶入菜單！✨", "加入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("請先點選列表中要帶入菜單的食物項目！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GridOfficialFoods_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridOfficialFoods.SelectedItem is FoodExchangeReferenceItem selected)
            {
                OnItemSelected?.Invoke(selected);
                MessageBox.Show($"已將「{selected.FoodName}」帶入菜單！✨", "加入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
