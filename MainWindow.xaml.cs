using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Data;
using NutritionPlannerApp.Models;
using NutritionPlannerApp.Services;

namespace NutritionPlannerApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;
        private ClientProfile _currentClient;
        private ObservableCollection<FoodExchangeItem> _exchanges;
        private ObservableCollection<CommercialFormula> _formulas;
        private ObservableCollection<ClientProfile> _clients;
        private ObservableCollection<MenuItemDetail> _menuRecipes;
        private List<FoodExchangeReferenceItem> _allOfficialFoods;
        private ObservableCollection<FoodExchangeReferenceItem> _filteredOfficialFoods;
        private List<FoodCompositionItem> _allFoodCompositions = new();
        private ObservableCollection<FoodCompositionItem> _filteredFoodCompositions = new();

        private bool _isInitializing = true;
        private bool _isDiabeticDietMode = false;
        private double _lastCalculatedBmi = 23.53;

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
            _dbService = new DatabaseService();
            _currentClient = new ClientProfile();

            _exchanges = new ObservableCollection<FoodExchangeItem>(DatabaseService.GetDefaultFoodExchanges(_isDiabeticDietMode));
            _formulas = new ObservableCollection<CommercialFormula>(_dbService.GetAllFormulas());
            _clients = new ObservableCollection<ClientProfile>(_dbService.GetAllClients());
            _menuRecipes = new ObservableCollection<MenuItemDetail>();

            // 監聽每一項食物代換屬性變更，保證份數調整時即時連動加總
            SubscribeExchangesPropertyChanged();
            SubscribeFormulasPropertyChanged();

            GridExchanges.ItemsSource = _exchanges;
            GridFoodExchangeRef.ItemsSource = _exchanges;
            GridFormulasPlanner.ItemsSource = _formulas;
            GridMenuRecipe.ItemsSource = _menuRecipes;
            LoadFoodCompositionDatabase();

            _allOfficialFoods = FoodExchangeDataService.GetOfficialFoodDatabase();
            _filteredOfficialFoods = new ObservableCollection<FoodExchangeReferenceItem>(_allOfficialFoods);

            _isInitializing = false;
            
            // 初始填入一次健康建議範例，讓介面一打開就生動豐富！
            ApplySampleDiet();
            ApplySampleMenuRecipes();
            ApplyMenuRecipeFilter();
            UpdateCalculations();
            UpdateMealPlannedSummaries();
        }

        // ==========================================
        // 1. 體位與生理評估核心計算 (BMI, IBW, eGFR)
        // ==========================================
        private void OnInputParamChanged(object? sender, EventArgs? e)
        {
            if (_isInitializing) return;
            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            if (InHeight == null || InWeight == null || InAge == null || InCreatinine == null || InGender == null)
                return;

            double.TryParse(InHeight.Text, out double height);
            double.TryParse(InWeight.Text, out double weight);
            double.TryParse(InAge.Text, out double age);
            double.TryParse(InCreatinine.Text, out double cr);
            bool isFemale = InGender.SelectedIndex == 1; // 0=男, 1=女

            // 1. BMI & IBW
            double bmi = NutritionCalculator.CalculateBmi(weight, height);
            double ibw = NutritionCalculator.CalculateIbw(height);
            double ibwPercent = NutritionCalculator.CalculateIbwPercent(weight, height);
            var (bmiCat, bmiColor, bmiAdvice) = NutritionCalculator.GetBmiStatus(bmi);

            LblBmiValue.Text = bmi.ToString("0.00");
            LblBmiStatus.Text = bmiCat;
            try { BmiBadge.Background = new BrushConverter().ConvertFromString(bmiColor) as Brush; } catch { }
            UpdateBmiIndicator(bmi);

            LblIbwValue.Text = $"{ibw:0.00} kg";
            LblIbwPercent.Text = $"{ibwPercent:0.0} %";
            double hM = height / 100.0;
            double lowW = Math.Round(18.5 * hM * hM, 1);
            double highW = Math.Round(24.0 * hM * hM, 1);
            LblIbwRange.Text = $"{lowW} ~ {highW} kg";

            // 2. eGFR
            double egfr = NutritionCalculator.CalculateEgfr(isFemale, age, cr);
            var (ckdStage, ckdDesc, ckdAdvice) = NutritionCalculator.GetCkdStage(egfr);

            LblEgfrValue.Text = egfr.ToString("0.0");
            LblEgfrStage.Text = ckdStage;

            // 3. BEE & TDEE
            double bee = NutritionCalculator.CalculateBee(isFemale, weight, height, age);
            double actFactor = 1.3;
            if (CboActivityFactor?.SelectedItem is ComboBoxItem actItem && double.TryParse(actItem.Tag?.ToString(), out double af))
                actFactor = af;

            double stressFactor = 1.0;
            if (CboStressFactor?.SelectedItem is ComboBoxItem strItem && double.TryParse(strItem.Tag?.ToString(), out double sf))
                stressFactor = sf;

            double tdee = NutritionCalculator.CalculateTdee(bee, actFactor, stressFactor);
            LblBeeValue.Text = $"{Math.Round(bee):0} kcal";
            LblTdeeValue.Text = $"{tdee:0} kcal";

            // 4. 更新臨床速算表按鈕文字
            UpdateQuickCalorieButtons(ibw, weight);

            // 5. 更新當前個案標籤
            string clientName = string.IsNullOrWhiteSpace(InName.Text) ? "未命名個案" : InName.Text;
            string chartNo = string.IsNullOrWhiteSpace(InChart.Text) ? "未設定" : InChart.Text;
            TxtActivePatientDisplay.Text = $"{clientName} ({chartNo}, {age:0}歲 {(isFemale ? "女" : "男")})";

            // 6. 三大營養素計算
            UpdateMacroGrams();
            RecalculateTotals();
        }

        private void UpdateQuickCalorieButtons(double ibw, double cbw)
        {
            if (BtnIbw25 == null) return;
            BtnIbw25.Content = $"IBW x 25 = {Math.Round(ibw * 25):0} kcal";
            BtnIbw25.Tag = Math.Round(ibw * 25).ToString();

            BtnIbw28.Content = $"IBW x 28 = {Math.Round(ibw * 28):0} kcal";
            BtnIbw28.Tag = Math.Round(ibw * 28).ToString();

            BtnIbw30.Content = $"IBW x 30 = {Math.Round(ibw * 30):0} kcal";
            BtnIbw30.Tag = Math.Round(ibw * 30).ToString();

            BtnIbw35.Content = $"IBW x 35 = {Math.Round(ibw * 35):0} kcal";
            BtnIbw35.Tag = Math.Round(ibw * 35).ToString();

            BtnCbw25.Content = $"CBW x 25 = {Math.Round(cbw * 25):0} kcal";
            BtnCbw25.Tag = Math.Round(cbw * 25).ToString();

            BtnCbw28.Content = $"CBW x 28 = {Math.Round(cbw * 28):0} kcal";
            BtnCbw28.Tag = Math.Round(cbw * 28).ToString();

            BtnCbw30.Content = $"CBW x 30 = {Math.Round(cbw * 30):0} kcal";
            BtnCbw30.Tag = Math.Round(cbw * 30).ToString();

            BtnCbw35.Content = $"CBW x 35 = {Math.Round(cbw * 35):0} kcal";
            BtnCbw35.Tag = Math.Round(cbw * 35).ToString();

            BtnCbw40.Content = $"CBW x 40 = {Math.Round(cbw * 40):0} kcal";
            BtnCbw40.Tag = Math.Round(cbw * 40).ToString();

            BtnCbw45.Content = $"CBW x 45 = {Math.Round(cbw * 45):0} kcal";
            BtnCbw45.Tag = Math.Round(cbw * 45).ToString();
        }

        // ==========================================
        // 2. 特殊長照推算工具
        // ==========================================
        private void BtnCalcKnee_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InKneeHeight.Text, out double knee);
            double.TryParse(InAge.Text, out double age);
            bool isFemale = InGender.SelectedIndex == 1;

            double estHeight = NutritionCalculator.EstimateHeightFromKnee(isFemale, knee, age);
            LblKneeHeightResult.Text = $"{estHeight:0.0} cm";
        }

        private void BtnApplyKneeHeight_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InKneeHeight.Text, out double knee);
            double.TryParse(InAge.Text, out double age);
            bool isFemale = InGender.SelectedIndex == 1;

            double estHeight = NutritionCalculator.EstimateHeightFromKnee(isFemale, knee, age);
            if (estHeight > 0)
            {
                InHeight.Text = estHeight.ToString("0.0");
                MessageBox.Show($"已成功將膝長推算身高 {estHeight:0.0} cm 代入！✨", "代入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCalcUlna_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InUlnaLength.Text, out double ulna);
            double.TryParse(InAge.Text, out double age);
            bool isFemale = InGender.SelectedIndex == 1;

            double estHeight = NutritionCalculator.EstimateHeightFromUlna(isFemale, ulna, age);
            LblUlnaHeightResult.Text = $"{estHeight:0.0} cm";
        }

        private void BtnApplyUlnaHeight_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InUlnaLength.Text, out double ulna);
            double.TryParse(InAge.Text, out double age);
            bool isFemale = InGender.SelectedIndex == 1;

            double estHeight = NutritionCalculator.EstimateHeightFromUlna(isFemale, ulna, age);
            if (estHeight > 0)
            {
                InHeight.Text = estHeight.ToString("0.0");
                MessageBox.Show($"已成功將尺骨長推算身高 {estHeight:0.0} cm 代入！✨", "代入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCalcWeight_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InHipCircumference.Text, out double hip);
            double.TryParse(InArmCircumference.Text, out double arm);
            bool isFemale = InGender.SelectedIndex == 1;

            double estWeight = NutritionCalculator.EstimateWeightFromCircumference(isFemale, hip, arm);
            LblEstimatedWeightResult.Text = $"{estWeight:0.0} kg";
        }

        private void BtnApplyEstimatedWeight_Click(object sender, RoutedEventArgs e)
        {
            double.TryParse(InHipCircumference.Text, out double hip);
            double.TryParse(InArmCircumference.Text, out double arm);
            bool isFemale = InGender.SelectedIndex == 1;

            double estWeight = NutritionCalculator.EstimateWeightFromCircumference(isFemale, hip, arm);
            if (estWeight > 0)
            {
                InWeight.Text = estWeight.ToString("0.0");
                MessageBox.Show($"已成功將臂圍臀圍推算體重 {estWeight:0.0} kg 代入！✨", "代入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ==========================================
        // 3. 熱量與三大營養素設定
        // ==========================================
        private void OnFactorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            UpdateCalculations();
        }

        private void BtnApplyTdeeAsTarget_Click(object sender, RoutedEventArgs e)
        {
            string tdeeStr = LblTdeeValue.Text.Replace(" kcal", "").Trim();
            if (double.TryParse(tdeeStr, out double tdee))
            {
                InTargetCalories.Text = tdee.ToString("0");
                MessageBox.Show($"已成功套用 TDEE: {tdee:0} kcal 為目標熱量！🎯", "設定成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnQuickCalorie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && double.TryParse(btn.Tag.ToString(), out double cal))
            {
                InTargetCalories.Text = cal.ToString("0");
                MessageBox.Show($"已套用速算熱量: {cal:0} kcal！🎯", "設定成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnTargetCalorieChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing) return;
            double.TryParse(InTargetCalories.Text, out double targetCal);
            TxtActiveCaloriesDisplay.Text = $"{targetCal:0} kcal";
            UpdateMacroGrams();
            RecalculateTotals();
        }

        private bool _isDiseaseDietMode = false;

        private void ToggleDiseaseDiet_Click(object sender, RoutedEventArgs e)
        {
            _isDiseaseDietMode = ToggleDiseaseDiet.IsChecked == true;
            if (_isDiseaseDietMode)
            {
                ToggleDiseaseDiet.Background = new BrushConverter().ConvertFromString("#7C3AED") as Brush;
                ToggleDiseaseDiet.Foreground = Brushes.White;
                LblDiseaseDietStatus.Text = "已啟用疾病飲食模式：解除 50-60% / 20-30% / 10-20% 限制";
                TxtStatus.Text = "已切換至【疾病飲食模式】，熱量百分比建議已自動隱藏，可自由自訂比例！";

                // 自動隱藏「熱量百分比建議」欄位
                if (ColRatioSuggest != null) ColRatioSuggest.Width = new GridLength(0);
                if (HdrRatioSuggest != null) HdrRatioSuggest.Visibility = Visibility.Collapsed;
                if (CellRatioSuggestCarb != null) CellRatioSuggestCarb.Visibility = Visibility.Collapsed;
                if (CellRatioSuggestFat != null) CellRatioSuggestFat.Visibility = Visibility.Collapsed;
                if (CellRatioSuggestProt != null) CellRatioSuggestProt.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToggleDiseaseDiet.Background = new BrushConverter().ConvertFromString("#F1F5F9") as Brush;
                ToggleDiseaseDiet.Foreground = new BrushConverter().ConvertFromString("#475569") as Brush;
                LblDiseaseDietStatus.Text = "";
                TxtStatus.Text = "✨ 已切換至【一般標準飲食模式】，顯示建議百分比並啟用輸入限制規範。";

                // 顯示「熱量百分比建議」欄位
                if (ColRatioSuggest != null) ColRatioSuggest.Width = new GridLength(115);
                if (HdrRatioSuggest != null) HdrRatioSuggest.Visibility = Visibility.Visible;
                if (CellRatioSuggestCarb != null) CellRatioSuggestCarb.Visibility = Visibility.Visible;
                if (CellRatioSuggestFat != null) CellRatioSuggestFat.Visibility = Visibility.Visible;
                if (CellRatioSuggestProt != null) CellRatioSuggestProt.Visibility = Visibility.Visible;
            }
            UpdateMacroGrams();
        }

        private string _lastValidCarb = "50";
        private string _lastValidFat = "30";
        private string _lastValidProt = "20";
        private bool _isRevertingRatio = false;

        private void OnRatioChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing || _isRevertingRatio) return;

            // 正在輸入中：即時更新樣式與下方計算，不立即跳彈窗阻擋，讓使用者能順暢刪除或修改（如 50 -> 5 -> 55）
            if (sender is TextBox tb)
            {
                if (double.TryParse(tb.Text, out double val))
                {
                    bool isInvalid = false;
                    if (!_isDiseaseDietMode)
                    {
                        if (tb == InCarbPercent && (val < 50 || val > 60)) isInvalid = true;
                        else if (tb == InFatPercent && (val < 20 || val > 30)) isInvalid = true;
                        else if (tb == InProteinPercent && (val < 10 || val > 20)) isInvalid = true;
                    }

                    if (isInvalid)
                    {
                        // 標記紅色提醒
                        tb.Foreground = new BrushConverter().ConvertFromString("#E63946") as Brush;
                        tb.BorderBrush = new BrushConverter().ConvertFromString("#E63946") as Brush;
                    }
                    else
                    {
                        // 落在合法區間內，恢復正常樣式並更新有效值
                        tb.Foreground = new BrushConverter().ConvertFromString("#2D3748") as Brush;
                        tb.BorderBrush = new BrushConverter().ConvertFromString("#E2E8F0") as Brush;
                        if (tb == InCarbPercent) _lastValidCarb = tb.Text;
                        else if (tb == InFatPercent) _lastValidFat = tb.Text;
                        else if (tb == InProteinPercent) _lastValidProt = tb.Text;
                    }
                }
            }

            UpdateMacroGrams();
            RecalculateTotals();
        }

        private void OnRatioKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb)
            {
                ValidateAndEnforceRatio(tb);
            }
        }

        private void OnRatioLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                ValidateAndEnforceRatio(tb);
            }
        }

        private void ValidateAndEnforceRatio(TextBox tb)
        {
            if (_isInitializing || _isRevertingRatio) return;

            if (_isDiseaseDietMode)
            {
                tb.Foreground = new BrushConverter().ConvertFromString("#2D3748") as Brush;
                tb.BorderBrush = new BrushConverter().ConvertFromString("#E2E8F0") as Brush;
                return;
            }

            bool isInvalid = false;
            string errMsg = "";

            if (double.TryParse(tb.Text, out double val))
            {
                if (tb == InCarbPercent)
                {
                    if (val < 50 || val > 60)
                    {
                        isInvalid = true;
                        errMsg = "醣類標準建議為 50~60%，無法設定此比例！疾病飲食請開啟「疾病飲食模式」解除限制。";
                    }
                }
                else if (tb == InFatPercent)
                {
                    if (val < 20 || val > 30)
                    {
                        isInvalid = true;
                        errMsg = "脂質標準建議為 20~30%，無法設定此比例！疾病飲食請開啟「疾病飲食模式」解除限制。";
                    }
                }
                else if (tb == InProteinPercent)
                {
                    if (val < 10 || val > 20)
                    {
                        isInvalid = true;
                        errMsg = "蛋白質標準建議為 10~20%，無法設定此比例！疾病飲食請開啟「疾病飲食模式」解除限制。";
                    }
                }
            }
            else
            {
                isInvalid = true;
                errMsg = "請輸入有效的數字百分比！";
            }

            if (isInvalid)
            {
                tb.Foreground = new BrushConverter().ConvertFromString("#E63946") as Brush;
                tb.BorderBrush = new BrushConverter().ConvertFromString("#E63946") as Brush;
                if (LblRatioSumWarning != null)
                {
                    LblRatioSumWarning.Text = errMsg;
                    LblRatioSumWarning.Foreground = new BrushConverter().ConvertFromString("#E63946") as Brush;
                }

                MessageBox.Show(errMsg, "設定比例超出建議範圍", MessageBoxButton.OK, MessageBoxImage.Warning);

                _isRevertingRatio = true;
                if (tb == InCarbPercent) tb.Text = _lastValidCarb;
                else if (tb == InFatPercent) tb.Text = _lastValidFat;
                else if (tb == InProteinPercent) tb.Text = _lastValidProt;
                tb.Foreground = new BrushConverter().ConvertFromString("#2D3748") as Brush;
                tb.BorderBrush = new BrushConverter().ConvertFromString("#E2E8F0") as Brush;
                _isRevertingRatio = false;

                UpdateMacroGrams();
                RecalculateTotals();
            }
            else
            {
                tb.Foreground = new BrushConverter().ConvertFromString("#2D3748") as Brush;
                tb.BorderBrush = new BrushConverter().ConvertFromString("#E2E8F0") as Brush;
                if (tb == InCarbPercent) _lastValidCarb = tb.Text;
                else if (tb == InFatPercent) _lastValidFat = tb.Text;
                else if (tb == InProteinPercent) _lastValidProt = tb.Text;
            }
        }

        private void UpdateMacroGrams()
        {
            if (InTargetCalories == null || InCarbPercent == null || InProteinPercent == null || InFatPercent == null)
                return;

            double.TryParse(InTargetCalories.Text, out double targetCal);
            double.TryParse(InCarbPercent.Text, out double carbPct);
            double.TryParse(InProteinPercent.Text, out double protPct);
            double.TryParse(InFatPercent.Text, out double fatPct);

            // 動態更新三大營養素表格中的目標總熱量數字
            string targetCalStr = $"{targetCal:0} kcal";
            if (LblRowTargetKcalCarb != null) LblRowTargetKcalCarb.Text = targetCalStr;
            if (LblRowTargetKcalFat != null) LblRowTargetKcalFat.Text = targetCalStr;
            if (LblRowTargetKcalProt != null) LblRowTargetKcalProt.Text = targetCalStr;

            // 計算克數與熱量過程
            var (carbG, fatG, protG) = NutritionCalculator.CalculateMacroGrams(targetCal, carbPct, fatPct, protPct);
            double carbKcal = targetCal * (carbPct / 100.0);
            double fatKcal = targetCal * (fatPct / 100.0);
            double protKcal = targetCal * (protPct / 100.0);

            LblTargetCarbGrams.Text = $"{carbG:0.0} g";
            if (LblCalcProcessCarb != null)
                LblCalcProcessCarb.Text = $"{carbKcal:0} ÷ 4 = {carbG:0.0} g";

            LblTargetFatGrams.Text = $"{fatG:0.0} g";
            if (LblCalcProcessFat != null)
                LblCalcProcessFat.Text = $"{fatKcal:0} ÷ 9 = {fatG:0.0} g";

            LblTargetProteinGrams.Text = $"{protG:0.0} g";
            if (LblCalcProcessProt != null)
                LblCalcProcessProt.Text = $"{protKcal:0} ÷ 4 = {protG:0.0} g";

            // 比例總和與規範驗證
            double sum = carbPct + protPct + fatPct;
            if (LblRatioSumWarning != null)
            {
                bool isSum100 = Math.Abs(sum - 100.0) < 0.1;
                bool isCarbValid = carbPct >= 50 && carbPct <= 60;
                bool isFatValid = fatPct >= 20 && fatPct <= 30;
                bool isProtValid = protPct >= 10 && protPct <= 20;

                if (!isSum100)
                {
                    LblRatioSumWarning.Text = $"⚠️ 比例總和：{sum:0.#}% (非 100%，請調整各項比例)";
                    LblRatioSumWarning.Foreground = new BrushConverter().ConvertFromString("#E63946") as Brush;
                }
                else if (!_isDiseaseDietMode && (!isCarbValid || !isFatValid || !isProtValid))
                {
                    LblRatioSumWarning.Text = $"⚠️ 比例總和：100%，但超出一般建議範圍 (醣50~60%、脂20~30%、蛋10~20%)！請微調或點選「疾病飲食」解除限制。";
                    LblRatioSumWarning.Foreground = new BrushConverter().ConvertFromString("#E63946") as Brush;
                }
                else
                {
                    LblRatioSumWarning.Text = _isDiseaseDietMode 
                        ? $"比例總和：100% (特殊疾病飲食模式自訂)" 
                        : $"✨ 比例總和：100% (正常平衡 ｜ 完全符合標準百分比建議)";
                    LblRatioSumWarning.Foreground = new BrushConverter().ConvertFromString("#2A9D8F") as Brush;
                }
            }

            // Dashboard
            LblDashTargetKcal.Text = $"{targetCal:0} kcal";
            LblDashTargetMacros.Text = $"醣 {carbG:0.#}g ({carbPct:0}%) ｜ 蛋 {protG:0.#}g ({protPct:0}%) ｜ 脂 {fatG:0.#}g ({fatPct:0}%)";
            LblRatioTargetText.Text = $"{carbPct:0}% / {protPct:0}% / {fatPct:0}%";
        }

        private void BtnPresetBalanced_Click(object sender, RoutedEventArgs e)
        {
            InCarbPercent.Text = "55";
            InProteinPercent.Text = "15";
            InFatPercent.Text = "30";
        }

        private void BtnPresetHighProtein_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDiseaseDietMode)
            {
                ToggleDiseaseDiet.IsChecked = true;
                ToggleDiseaseDiet_Click(ToggleDiseaseDiet, new RoutedEventArgs());
            }
            InCarbPercent.Text = "40";
            InProteinPercent.Text = "30";
            InFatPercent.Text = "30";
        }

        private void BtnPresetLowProtein_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDiseaseDietMode)
            {
                ToggleDiseaseDiet.IsChecked = true;
                ToggleDiseaseDiet_Click(ToggleDiseaseDiet, new RoutedEventArgs());
            }
            InCarbPercent.Text = "65";
            InProteinPercent.Text = "8";
            InFatPercent.Text = "27";
        }

        // ==========================================
        // 4. 六大類食物與配方份數計算 (CellEdit & Totals)
        // ==========================================
        private void SubscribeExchangesPropertyChanged()
        {
            foreach (var item in _exchanges)
            {
                item.PropertyChanged -= OnExchangeItemPropertyChanged;
                item.PropertyChanged += OnExchangeItemPropertyChanged;
            }
        }

        private void SubscribeFormulasPropertyChanged()
        {
            foreach (var item in _formulas)
            {
                item.PropertyChanged -= OnFormulaItemPropertyChanged;
                item.PropertyChanged += OnFormulaItemPropertyChanged;
            }
        }

        private void OnExchangeItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isInitializing) return;
            // 當餐次份數變更時，立即更新即時小計與儀表板
            RecalculateTotals();
        }

        private void OnFormulaItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isInitializing) return;
            RecalculateTotals();
        }

        private void GridExchanges_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs? e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in _exchanges)
                {
                    item.Recalculate();
                }
                RecalculateTotals();
            }));
        }

        private void GridFormulasPlanner_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs? e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in _formulas)
                {
                    item.Recalculate();
                }
                RecalculateTotals();
            }));
        }

        private void BtnClearExchanges_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _exchanges)
            {
                item.ActualPortions = 0;
                item.Breakfast = 0;
                item.MorningSnack = 0;
                item.Lunch = 0;
                item.AfternoonSnack = 0;
                item.Dinner = 0;
                item.NightSnack = 0;
                item.Recalculate();
            }
            RecalculateTotals();
        }

        private void BtnPresetSampleDiet_Click(object sender, RoutedEventArgs e)
        {
            ApplySampleDiet();
            RecalculateTotals();
            MessageBox.Show("已成功填入 2000 kcal 經典均衡建議菜單！🍱✨", "填入範例", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplySampleDiet()
        {
            // 填入常見健康的 2000 kcal 6 餐份數範例
            foreach (var item in _exchanges)
            {
                item.ActualPortions = 0;
                item.Breakfast = 0;
                item.MorningSnack = 0;
                item.Lunch = 0;
                item.AfternoonSnack = 0;
                item.Dinner = 0;
                item.NightSnack = 0;

                switch (item.Category)
                {
                    case "低脂奶":
                        item.Breakfast = 1;
                        item.NightSnack = 1;
                        item.ActualPortions = 1;
                        break;
                    case "穀物雜糧類":
                    case "米食類":
                        item.Breakfast = 2; // 飯糰或麵包等量
                        item.Lunch = 4;     // 1碗飯
                        item.Dinner = 4;    // 1碗飯
                        item.ActualPortions = 10;
                        break;
                    case "豆魚蛋肉類(低脂)":
                        item.Breakfast = 1;
                        item.Lunch = 2;
                        item.Dinner = 2;
                        item.ActualPortions = 3;
                        break;
                    case "豆魚蛋肉類(中脂)":
                        item.Lunch = 1;
                        item.Dinner = 1;
                        item.ActualPortions = 2;
                        break;
                    case "蔬菜類":
                        item.Lunch = 2;
                        item.Dinner = 2;
                        item.ActualPortions = 2;
                        break;
                    case "水果類":
                        item.MorningSnack = 1;
                        item.AfternoonSnack = 1;
                        item.ActualPortions = 1;
                        break;
                    case "油脂類":
                        item.Breakfast = 1;
                        item.Lunch = 2;
                        item.Dinner = 2;
                        item.ActualPortions = 4;
                        break;
                    case "堅果種子類":
                        item.AfternoonSnack = 1;
                        item.ActualPortions = 0;
                        break;
                }
                item.Recalculate();
            }
        }

        private NutritionSummary BuildCurrentSummary()
        {
            double.TryParse(InTargetCalories.Text, out double targetCal);
            double.TryParse(InCarbPercent.Text, out double carbPct);
            double.TryParse(InProteinPercent.Text, out double protPct);
            double.TryParse(InFatPercent.Text, out double fatPct);

            var (tCarbG, tFatG, tProtG) = NutritionCalculator.CalculateMacroGrams(targetCal, carbPct, fatPct, protPct);

            var summary = new NutritionSummary
            {
                TargetCalories = targetCal,
                TargetCarbGrams = tCarbG,
                TargetFatGrams = tFatG,
                TargetProteinGrams = tProtG
            };

            // 1. 食物份數總計
            double exSuggProt = 0, exSuggFat = 0, exSuggCarb = 0, exSuggKcal = 0;
            double exActProt = 0, exActFat = 0, exActCarb = 0, exActKcal = 0;

            foreach (var item in _exchanges)
            {
                exSuggProt += item.SuggestedProtein;
                exSuggFat += item.SuggestedFat;
                exSuggCarb += item.SuggestedCarbs;
                exSuggKcal += item.SuggestedCalories;

                exActProt += item.ActualProtein;
                exActFat += item.ActualFat;
                exActCarb += item.ActualCarbs;
                exActKcal += item.ActualCalories;
            }

            // 2. 商業配方總計
            double formSuggProt = 0, formSuggFat = 0, formSuggCarb = 0, formSuggKcal = 0;
            double formActProt = 0, formActFat = 0, formActCarb = 0, formActKcal = 0;

            foreach (var f in _formulas)
            {
                formSuggProt += f.SuggestedTotalProtein;
                formSuggFat += f.SuggestedTotalFat;
                formSuggCarb += f.SuggestedTotalCarbs;
                formSuggKcal += f.SuggestedTotalCalories;

                formActProt += f.ActualTotalProtein;
                formActFat += f.ActualTotalFat;
                formActCarb += f.ActualTotalCarbs;
                formActKcal += f.ActualTotalCalories;
            }

            // 合併總計
            summary.SuggestedProtein = Math.Round(exSuggProt + formSuggProt, 1);
            summary.SuggestedFat = Math.Round(exSuggFat + formSuggFat, 1);
            summary.SuggestedCarbs = Math.Round(exSuggCarb + formSuggCarb, 1);
            summary.SuggestedCalories = Math.Round(exSuggKcal + formSuggKcal, 1);

            summary.ActualProtein = Math.Round(exActProt + formActProt, 1);
            summary.ActualFat = Math.Round(exActFat + formActFat, 1);
            summary.ActualCarbs = Math.Round(exActCarb + formActCarb, 1);
            summary.ActualCalories = Math.Round(exActKcal + formActKcal, 1);

            // 計算熱量佔比
            if (summary.SuggestedCalories > 0)
            {
                summary.SuggestedProteinRatio = Math.Round((summary.SuggestedProtein * 4 / summary.SuggestedCalories) * 100, 1);
                summary.SuggestedFatRatio = Math.Round((summary.SuggestedFat * 9 / summary.SuggestedCalories) * 100, 1);
                summary.SuggestedCarbRatio = Math.Round((summary.SuggestedCarbs * 4 / summary.SuggestedCalories) * 100, 1);
            }

            if (summary.ActualCalories > 0)
            {
                summary.ActualProteinRatio = Math.Round((summary.ActualProtein * 4 / summary.ActualCalories) * 100, 1);
                summary.ActualFatRatio = Math.Round((summary.ActualFat * 9 / summary.ActualCalories) * 100, 1);
                summary.ActualCarbRatio = Math.Round((summary.ActualCarbs * 4 / summary.ActualCalories) * 100, 1);
            }

            return summary;
        }

        private void RecalculateTotals()
        {
            if (LblExchangeActualProt == null) return;

            var summary = BuildCurrentSummary();

            // 食物份數小計與堅果種子下方之統計列 (三大營養素加總克數與百分比)
            double exSuggProt = 0, exSuggFat = 0, exSuggCarb = 0, exSuggKcal = 0;
            double exActProt = 0, exActFat = 0, exActCarb = 0, exActKcal = 0;
            double exActPortions = 0;
            double exBf = 0, exMs = 0, exLu = 0, exAs = 0, exDi = 0, exNs = 0, exSuggTotal = 0;

            foreach (var item in _exchanges)
            {
                exSuggProt += item.SuggestedProtein;
                exSuggFat += item.SuggestedFat;
                exSuggCarb += item.SuggestedCarbs;
                exSuggKcal += item.SuggestedCalories;

                exActProt += item.ActualProtein;
                exActFat += item.ActualFat;
                exActCarb += item.ActualCarbs;
                exActKcal += item.ActualCalories;
                exActPortions += item.ActualPortions;

                exBf += item.Breakfast;
                exMs += item.MorningSnack;
                exLu += item.Lunch;
                exAs += item.AfternoonSnack;
                exDi += item.Dinner;
                exNs += item.NightSnack;
                exSuggTotal += item.SuggestedTotalPortions;
            }

            if (LblExchangeActualPortions != null) LblExchangeActualPortions.Text = $"{exActPortions:0.#}";
            if (LblExchangeActualProt != null) LblExchangeActualProt.Text = $"{exActProt:0.0} g";
            if (LblExchangeActualFat != null) LblExchangeActualFat.Text = $"{exActFat:0.0} g";
            if (LblExchangeActualCarb != null) LblExchangeActualCarb.Text = $"{exActCarb:0.0} g";
            if (LblExchangeActualKcal != null) LblExchangeActualKcal.Text = $"{exActKcal:0} kcal";

            if (LblMealBfSum != null) LblMealBfSum.Text = $"{exBf:0.#}";
            if (LblMealMsSum != null) LblMealMsSum.Text = $"{exMs:0.#}";
            if (LblMealLuSum != null) LblMealLuSum.Text = $"{exLu:0.#}";
            if (LblMealAsSum != null) LblMealAsSum.Text = $"{exAs:0.#}";
            if (LblMealDiSum != null) LblMealDiSum.Text = $"{exDi:0.#}";
            if (LblMealNsSum != null) LblMealNsSum.Text = $"{exNs:0.#}";
            if (LblMealTotalSum != null) LblMealTotalSum.Text = $"{exSuggTotal:0.#}";

            // 計算 4-9-4 營養素計算熱量：醣類*4 + 蛋白質*4 + 脂質*9
            double calcKcal = (exActCarb * 4.0) + (exActProt * 4.0) + (exActFat * 9.0);
            if (LblExchangeCalcKcalFormula != null)
            {
                LblExchangeCalcKcalFormula.Text = $"{calcKcal:0} kcal";
            }

            // 計算三大營養素百分比 (以計算總熱量為分母)
            double carbPct = calcKcal > 0 ? (exActCarb * 4.0 / calcKcal * 100.0) : 0;
            double fatPct = calcKcal > 0 ? (exActFat * 9.0 / calcKcal * 100.0) : 0;
            double protPct = calcKcal > 0 ? (exActProt * 4.0 / calcKcal * 100.0) : 0;

            if (LblExchangeCarbRatio != null) LblExchangeCarbRatio.Text = $"{carbPct:0.0} %";
            if (LblExchangeFatRatio != null) LblExchangeFatRatio.Text = $"{fatPct:0.0} %";
            if (LblExchangeProtRatio != null) LblExchangeProtRatio.Text = $"{protPct:0.0} %";
            if (LblExchangeRatioTotal != null) LblExchangeRatioTotal.Text = calcKcal > 0 ? "100.0 %" : "0.0 %";

            if (LblExchangeMacroFormulaBreakdown != null)
            {
                LblExchangeMacroFormulaBreakdown.Text = $"4-9-4熱量：醣 {exActCarb:0.#}g×4 ({exActCarb*4:0} kcal) ＋ 蛋 {exActProt:0.#}g×4 ({exActProt*4:0} kcal) ＋ 脂 {exActFat:0.#}g×9 ({exActFat*9:0} kcal) ＝ {calcKcal:0} kcal";
            }

            // 商業配方小計 Bar
            double formSuggProt = 0, formSuggFat = 0, formSuggCarb = 0, formSuggKcal = 0;
            foreach (var f in _formulas)
            {
                formSuggProt += f.SuggestedTotalProtein;
                formSuggFat += f.SuggestedTotalFat;
                formSuggCarb += f.SuggestedTotalCarbs;
                formSuggKcal += f.SuggestedTotalCalories;
            }
            LblFormulaTotalProt.Text = $"{formSuggProt:0.0} g";
            LblFormulaTotalFat.Text = $"{formSuggFat:0.0} g";
            LblFormulaTotalCarb.Text = $"{formSuggCarb:0.0} g";
            LblFormulaTotalKcal.Text = $"{formSuggKcal:0} kcal";

            // Dashboard 總結
            LblDashSuggestedKcal.Text = $"{summary.SuggestedCalories:0} kcal";
            LblDashSuggestedMacros.Text = $"醣 {summary.SuggestedCarbs:0.#}g ({summary.SuggestedCarbRatio:0}%) ｜ 蛋 {summary.SuggestedProtein:0.#}g ({summary.SuggestedProteinRatio:0}%) ｜ 脂 {summary.SuggestedFat:0.#}g ({summary.SuggestedFatRatio:0}%)";
            LblDashAchievementRate.Text = $"熱量達成率：{summary.CalorieAchievementRate:0.0} %";

            LblDashActualKcal.Text = $"{summary.ActualCalories:0} kcal";
            LblDashActualMacros.Text = $"醣 {summary.ActualCarbs:0.#}g ({summary.ActualCarbRatio:0}%) ｜ 蛋 {summary.ActualProtein:0.#}g ({summary.ActualProteinRatio:0}%) ｜ 脂 {summary.ActualFat:0.#}g ({summary.ActualFatRatio:0}%)";
            double diff = summary.ActualCalories - summary.TargetCalories;
            LblDashActualDiff.Text = $"與目標差距：{(diff >= 0 ? "+" : "")}{diff:0} kcal";

            // 進度條比例
            LblRatioSuggestedText.Text = $"{summary.SuggestedCarbRatio:0}% / {summary.SuggestedProteinRatio:0}% / {summary.SuggestedFatRatio:0}%";
            LblRatioActualText.Text = $"{summary.ActualCarbRatio:0}% / {summary.ActualProteinRatio:0}% / {summary.ActualFatRatio:0}%";
            UpdateMealPlannedSummaries();
        }

        // ==========================================
        // 5. 商業配方視窗管理
        // ==========================================
        private void BtnOpenFormulas_Click(object sender, RoutedEventArgs e)
        {
            var win = new FormulaWindow(_dbService) { Owner = this };
            if (win.ShowDialog() == true || win.DataChanged)
            {
                // 重新載入最新配方庫
                var updatedList = _dbService.GetAllFormulas();
                // 保留當前已經填入的 SuggestedPortions / ActualPortions
                var map = new Dictionary<int, (double Sugg, double Act)>();
                foreach (var f in _formulas)
                {
                    map[f.Id] = (f.SuggestedPortions, f.ActualPortions);
                }

                _formulas.Clear();
                foreach (var item in updatedList)
                {
                    if (map.TryGetValue(item.Id, out var p))
                    {
                        item.SuggestedPortions = p.Sugg;
                        item.ActualPortions = p.Act;
                        item.Recalculate();
                    }
                    _formulas.Add(item);
                }
                RecalculateTotals();
            }
        }

        // ==========================================
        // 6. 個案資料庫儲存與載入 (SQLite)
        // ==========================================
        private void BtnSaveClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double.TryParse(InHeight.Text, out double height);
                double.TryParse(InWeight.Text, out double weight);
                double.TryParse(InAge.Text, out double age);
                double.TryParse(InCreatinine.Text, out double cr);
                double.TryParse(InTargetCalories.Text, out double targetCal);
                double.TryParse(InCarbPercent.Text, out double carbPct);
                double.TryParse(InProteinPercent.Text, out double protPct);
                double.TryParse(InFatPercent.Text, out double fatPct);

                double.TryParse(InKneeHeight.Text, out double knee);
                double.TryParse(InUlnaLength.Text, out double ulna);
                double.TryParse(InHipCircumference.Text, out double hip);
                double.TryParse(InArmCircumference.Text, out double arm);

                double actFactor = 1.3;
                if (CboActivityFactor.SelectedItem is ComboBoxItem actItem && double.TryParse(actItem.Tag?.ToString(), out double af))
                    actFactor = af;

                double stressFactor = 1.0;
                if (CboStressFactor.SelectedItem is ComboBoxItem strItem && double.TryParse(strItem.Tag?.ToString(), out double sf))
                    stressFactor = sf;

                _currentClient.Name = string.IsNullOrWhiteSpace(InName.Text) ? "未命名個案" : InName.Text.Trim();
                _currentClient.ChartNumber = InChart.Text.Trim();
                _currentClient.Gender = InGender.SelectedIndex == 1 ? "女" : "男";
                _currentClient.Age = age;
                _currentClient.Height = height;
                _currentClient.Weight = weight;
                _currentClient.Creatinine = cr;
                _currentClient.ActivityFactor = actFactor;
                _currentClient.StressFactor = stressFactor;
                _currentClient.KneeHeight = knee;
                _currentClient.UlnaLength = ulna;
                _currentClient.HipCircumference = hip;
                _currentClient.ArmCircumference = arm;
                _currentClient.TargetCalories = targetCal;
                _currentClient.CarbRatio = carbPct;
                _currentClient.ProteinRatio = protPct;
                _currentClient.FatRatio = fatPct;
                _currentClient.Notes = TxtNotes.Text;
                _currentClient.UpdatedAt = DateTime.Now;

                int clientId = _dbService.SaveClient(_currentClient);
                _currentClient.Id = clientId;

                // 儲存菜單歷史記錄
                string exJson = JsonSerializer.Serialize(_exchanges);
                string formJson = JsonSerializer.Serialize(_formulas);

                var plan = new DietPlanRecord
                {
                    ClientId = clientId,
                    ClientName = _currentClient.Name,
                    Title = $"{_currentClient.Name} 的飲食計劃 ({DateTime.Now:yyyy-MM-dd})",
                    PlanDate = DateTime.Now,
                    TargetCalories = targetCal,
                    ExchangesJson = exJson,
                    FormulasJson = formJson,
                    SummaryNotes = TxtNotes.Text
                };
                _dbService.SaveDietPlan(plan);

                RefreshClientsList();
                TxtStatus.Text = $"✅ 已成功儲存個案【{_currentClient.Name}】與菜單計劃至 SQLite！({DateTime.Now:HH:mm:ss})";
                MessageBox.Show($"個案【{_currentClient.Name}】及菜單份數已成功儲存至 SQLite 本機資料庫！💾✨", "儲存成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNewClient_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("確定要開立新個案嗎？未儲存的異動將被重設。", "開立新個案", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _currentClient = new ClientProfile();
                InName.Text = "新個案";
                InChart.Text = $"P{DateTime.Now:MMddHH}";
                InGender.SelectedIndex = 0;
                InAge.Text = "30";
                InHeight.Text = "170";
                InWeight.Text = "65";
                InCreatinine.Text = "0.9";
                InTargetCalories.Text = "2000";
                InCarbPercent.Text = "50";
                InProteinPercent.Text = "20";
                InFatPercent.Text = "30";
                TxtNotes.Text = "";

                BtnClearExchanges_Click(this, new RoutedEventArgs());
                TxtStatus.Text = "✨ 已建立空白新個案，請開始評估！";
            }
        }

        private void RefreshClientsList()
        {
            _clients = new ObservableCollection<ClientProfile>(_dbService.GetAllClients());
        }

        private void BtnRefreshClients_Click(object sender, RoutedEventArgs e)
        {
            RefreshClientsList();
        }

        private void BtnLoadClientRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ClientProfile client)
            {
                _currentClient = client;
                _isInitializing = true;

                InName.Text = client.Name;
                InChart.Text = client.ChartNumber;
                InGender.SelectedIndex = client.Gender == "女" ? 1 : 0;
                InAge.Text = client.Age.ToString("0.#");
                InHeight.Text = client.Height.ToString("0.#");
                InWeight.Text = client.Weight.ToString("0.#");
                InCreatinine.Text = client.Creatinine.ToString("0.##");
                InKneeHeight.Text = client.KneeHeight.ToString("0.#");
                InUlnaLength.Text = client.UlnaLength.ToString("0.#");
                InHipCircumference.Text = client.HipCircumference.ToString("0.#");
                InArmCircumference.Text = client.ArmCircumference.ToString("0.#");
                InTargetCalories.Text = client.TargetCalories.ToString("0");
                InCarbPercent.Text = client.CarbRatio.ToString("0.#");
                InProteinPercent.Text = client.ProteinRatio.ToString("0.#");
                InFatPercent.Text = client.FatRatio.ToString("0.#");
                TxtNotes.Text = client.Notes;

                // 嘗試載入最新的菜單紀錄
                var plans = _dbService.GetDietPlansByClientId(client.Id);
                if (plans.Count > 0)
                {
                    var latestPlan = plans[0];
                    if (!string.IsNullOrWhiteSpace(latestPlan.ExchangesJson))
                    {
                        try
                        {
                            var loadedExchanges = JsonSerializer.Deserialize<List<FoodExchangeItem>>(latestPlan.ExchangesJson);
                            if (loadedExchanges != null && loadedExchanges.Count > 0)
                            {
                                _exchanges.Clear();
                                foreach (var item in loadedExchanges) _exchanges.Add(item);
                                SubscribeExchangesPropertyChanged();
                            }
                        }
                        catch { }
                    }

                    if (!string.IsNullOrWhiteSpace(latestPlan.FormulasJson))
                    {
                        try
                        {
                            var loadedFormulas = JsonSerializer.Deserialize<List<CommercialFormula>>(latestPlan.FormulasJson);
                            if (loadedFormulas != null && loadedFormulas.Count > 0)
                            {
                                _formulas.Clear();
                                foreach (var item in loadedFormulas) _formulas.Add(item);
                                SubscribeFormulasPropertyChanged();
                            }
                        }
                        catch { }
                    }
                }

                _isInitializing = false;
                UpdateCalculations();
                MainTabs.SelectedIndex = 0; // 切換回第一頁
                TxtStatus.Text = $"📂 已載入個案【{client.Name}】完整檔案！";
                MessageBox.Show($"已載入個案【{client.Name}】及對應菜單份數！✨", "載入成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteClientRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ClientProfile client)
            {
                var result = MessageBox.Show($"確定要刪除個案【{client.Name}】與所有相關歷史紀錄嗎？", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _dbService.DeleteClient(client.Id);
                    RefreshClientsList();
                    TxtStatus.Text = $"🗑️ 已刪除個案【{client.Name}】！";
                }
            }
        }

        // ==========================================
        // 7. 匯出與列印衛教評估單
        // ==========================================
        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 先同步當前個案基本資料
                double.TryParse(InHeight.Text, out double height);
                double.TryParse(InWeight.Text, out double weight);
                double.TryParse(InAge.Text, out double age);
                double.TryParse(InCreatinine.Text, out double cr);
                double.TryParse(InTargetCalories.Text, out double targetCal);
                double.TryParse(InCarbPercent.Text, out double carbPct);
                double.TryParse(InProteinPercent.Text, out double protPct);
                double.TryParse(InFatPercent.Text, out double fatPct);

                _currentClient.Name = string.IsNullOrWhiteSpace(InName.Text) ? "未命名個案" : InName.Text.Trim();
                _currentClient.ChartNumber = InChart.Text.Trim();
                _currentClient.Gender = InGender.SelectedIndex == 1 ? "女" : "男";
                _currentClient.Age = age;
                _currentClient.Height = height;
                _currentClient.Weight = weight;
                _currentClient.Creatinine = cr;
                _currentClient.TargetCalories = targetCal;
                _currentClient.CarbRatio = carbPct;
                _currentClient.ProteinRatio = protPct;
                _currentClient.FatRatio = fatPct;
                _currentClient.Notes = TxtNotes.Text;

                var summary = BuildCurrentSummary();
                string html = ExportService.GenerateHtmlReport(_currentClient, new List<FoodExchangeItem>(_exchanges), new List<CommercialFormula>(_formulas), summary);
                ExportService.OpenReportInBrowser(html);

                TxtStatus.Text = $"🖨️ 已產生衛教處方單並以瀏覽器開啟，可一鍵列印或另存為 PDF！({DateTime.Now:HH:mm:ss})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯出報表失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // 8. 側邊選單切換分頁導覽
        // ==========================================
        private void NavBtnCalories_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 0;
            TxtPageHeaderTitle.Text = "⚡ 基本資料與熱量計算";
            TxtPageHeaderSubtitle.Text = "包含個案資料、體位指標 (BMI/IBW/eGFR)、BEE/TDEE 與臨床速算對照表";
        }

        private void NavBtnFoodExchange_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 1;
            TxtPageHeaderTitle.Text = "📖 食物代換表 ＆ 三大營養素計算";
            TxtPageHeaderSubtitle.Text = "三大營養素百分比試算表、衛福部六大類食物代換標準表與每日飲食指南扇形圖";
        }

        private void NavBtnMenuDesign_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 2;
            TxtPageHeaderTitle.Text = "🍱 菜單設計";
            TxtPageHeaderSubtitle.Text = "天然食物各餐分配（早餐、早點、午餐、午點、晚餐、晚點）與實際飲食對照，即時加總小計";
        }

        private void NavBtnMenuRecipe_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 3;
            TxtPageHeaderTitle.Text = "📝 菜單開立 ＆ AI 食材顏色智慧辨識";
            TxtPageHeaderSubtitle.Text = "菜名、食材、份數(Ex)、生重克數(g)與 AI 自動辨識食材顏色（橙、黃、紅、綠、紫/黑、白、棕）";
            UpdateColorStatistics();
        }

        private void NavBtnFormulas_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 4;
            TxtPageHeaderTitle.Text = "🍼 管灌商業營養配方計算專區";
            TxtPageHeaderSubtitle.Text = "專屬管灌與商業配方規劃，填寫建議份數與實際份數，自動併入全日總熱量與三大營養素";
        }

        private void NavBtnNutritionSummary_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 5;
            TxtPageHeaderTitle.Text = "📊 營養成分總覽與個案存檔";
            TxtPageHeaderSubtitle.Text = "全方位營養成分對照表（目標 vs 建議菜單 vs 實際飲食）、營養師衛教建議與歷史個案庫";
        }

        private void BtnAddFoodExchange_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new FoodExchangeItem
            {
                Category = "自訂新食材",
                UnitName = "份 (100g)",
                Icon = "🥗",
                ProteinPerUnit = 0,
                FatPerUnit = 0,
                CarbPerUnit = 0,
                CaloriesPerUnit = 0,
                ActualPortions = 1
            };
            newItem.PropertyChanged += OnExchangeItemPropertyChanged;
            _exchanges.Add(newItem);

            // 選取該新項目
            GridFoodExchangeRef.SelectedItem = newItem;
            GridFoodExchangeRef.ScrollIntoView(newItem);
            RecalculateTotals();
            TxtStatus.Text = "➕ 已新增空白食物代換項目，可直接點選儲存格自訂名稱、份量說明與營養數值！";
        }

        private void BtnResetExchanges_Click(object sender, RoutedEventArgs e)
        {
            var def = DatabaseService.GetDefaultFoodExchanges(_isDiabeticDietMode);
            _exchanges.Clear();
            foreach (var item in def) _exchanges.Add(item);
            SubscribeExchangesPropertyChanged();
            ApplySampleDiet();
            UpdateCalculations();
            TxtStatus.Text = "🔄 已恢復食物代換表標準規格與數值！";
        }

        private void BtnToggleDiabeticDiet_Click(object sender, RoutedEventArgs e)
        {
            _isDiabeticDietMode = !_isDiabeticDietMode;
            UpdateDiabeticDietUI();
        }

        private void UpdateDiabeticDietUI()
        {
            string normalText = "糖尿病、低蛋白飲食";
            string activeText = "糖尿病、低蛋白飲食 (啟用中)";
            var activeBg = new BrushConverter().ConvertFromString("#0D9488") as Brush;
            var normalBg = new BrushConverter().ConvertFromString("#3B82F6") as Brush;

            if (BtnDiabeticDietTab1 != null)
            {
                BtnDiabeticDietTab1.Content = _isDiabeticDietMode ? activeText : normalText;
                BtnDiabeticDietTab1.Background = _isDiabeticDietMode ? activeBg : normalBg;
            }
            if (BtnDiabeticDietTab2 != null)
            {
                BtnDiabeticDietTab2.Content = _isDiabeticDietMode ? activeText : normalText;
                BtnDiabeticDietTab2.Background = _isDiabeticDietMode ? activeBg : normalBg;
            }

            if (_isDiabeticDietMode)
            {
                // 啟用：米食類蛋白質 1.5g、麵食類 2.5g，不顯示穀物雜糧類
                var grain = _exchanges.FirstOrDefault(x => x.Category == "穀物雜糧類");
                int idx = 3;
                double prevAct = 0, prevBf = 0, prevMs = 0, prevLu = 0, prevAs = 0, prevDi = 0, prevNs = 0;
                if (grain != null)
                {
                    idx = _exchanges.IndexOf(grain);
                    prevAct = grain.ActualPortions;
                    prevBf = grain.Breakfast;
                    prevMs = grain.MorningSnack;
                    prevLu = grain.Lunch;
                    prevAs = grain.AfternoonSnack;
                    prevDi = grain.Dinner;
                    prevNs = grain.NightSnack;
                    _exchanges.Remove(grain);
                }

                if (!_exchanges.Any(x => x.Category == "米食類"))
                {
                    var rice = new FoodExchangeItem
                    {
                        Category = "米食類",
                        Icon = "🍚",
                        UnitName = "份 (1/4碗飯)",
                        ProteinPerUnit = 1.5,
                        FatPerUnit = 0,
                        CarbPerUnit = 15,
                        CaloriesPerUnit = 70,
                        ActualPortions = prevAct,
                        Breakfast = prevBf,
                        MorningSnack = prevMs,
                        Lunch = prevLu,
                        AfternoonSnack = prevAs,
                        Dinner = prevDi,
                        NightSnack = prevNs
                    };
                    rice.Recalculate();
                    _exchanges.Insert(idx, rice);
                }
                if (!_exchanges.Any(x => x.Category == "麵食類"))
                {
                    var noodle = new FoodExchangeItem
                    {
                        Category = "麵食類",
                        Icon = "🍜",
                        UnitName = "份 (1/2碗麵)",
                        ProteinPerUnit = 2.5,
                        FatPerUnit = 0,
                        CarbPerUnit = 15,
                        CaloriesPerUnit = 70,
                        ActualPortions = 0
                    };
                    noodle.Recalculate();
                    _exchanges.Insert(idx + 1, noodle);
                }

                SubscribeExchangesPropertyChanged();
                RecalculateTotals();
                TxtStatus.Text = "已啟用【糖尿病、低蛋白飲食】：米食類蛋白質以 1.5g、麵食類以 2.5g 計算。";
            }
            else
            {
                // 停用：以穀物雜糧類 (2g) 計算，不顯示米食類、麵食類
                var rice = _exchanges.FirstOrDefault(x => x.Category == "米食類");
                var noodle = _exchanges.FirstOrDefault(x => x.Category == "麵食類");
                int idx = 3;
                double totAct = 0, totBf = 0, totMs = 0, totLu = 0, totAs = 0, totDi = 0, totNs = 0;

                if (rice != null)
                {
                    idx = _exchanges.IndexOf(rice);
                    totAct += rice.ActualPortions;
                    totBf += rice.Breakfast;
                    totMs += rice.MorningSnack;
                    totLu += rice.Lunch;
                    totAs += rice.AfternoonSnack;
                    totDi += rice.Dinner;
                    totNs += rice.NightSnack;
                    _exchanges.Remove(rice);
                }
                if (noodle != null)
                {
                    totAct += noodle.ActualPortions;
                    totBf += noodle.Breakfast;
                    totMs += noodle.MorningSnack;
                    totLu += noodle.Lunch;
                    totAs += noodle.AfternoonSnack;
                    totDi += noodle.Dinner;
                    totNs += noodle.NightSnack;
                    _exchanges.Remove(noodle);
                }

                if (!_exchanges.Any(x => x.Category == "穀物雜糧類"))
                {
                    var grain = new FoodExchangeItem
                    {
                        Category = "穀物雜糧類",
                        Icon = "🍚",
                        UnitName = "份 (1/4碗飯或半碗麵)",
                        ProteinPerUnit = 2,
                        FatPerUnit = 0,
                        CarbPerUnit = 15,
                        CaloriesPerUnit = 70,
                        ActualPortions = totAct,
                        Breakfast = totBf,
                        MorningSnack = totMs,
                        Lunch = totLu,
                        AfternoonSnack = totAs,
                        Dinner = totDi,
                        NightSnack = totNs
                    };
                    grain.Recalculate();
                    _exchanges.Insert(idx, grain);
                }

                SubscribeExchangesPropertyChanged();
                RecalculateTotals();
                TxtStatus.Text = "🌾 已切換至一般標準模式：顯示【穀物雜糧類】(蛋白質 2.0g)，不顯示米食類、麵食類。";
            }
        }

        // ==========================================
        // 9. 視窗控制（原生 Win32 邊緣拖曳縮放、拖曳移動、最小化、最大化、關閉）
        // ==========================================
        private const int WM_NCHITTEST = 0x0084;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCAPTION = 2;
        private const int HTCLIENT = 1;

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                int x = (short)(lParam.ToInt64() & 0xFFFF);
                int y = (short)((lParam.ToInt64() >> 16) & 0xFFFF);
                var windowPoint = PointFromScreen(new Point(x, y));

                const double resizeBorder = 8.0;

                bool isLeft = windowPoint.X <= resizeBorder;
                bool isRight = windowPoint.X >= ActualWidth - resizeBorder;
                bool isTop = windowPoint.Y <= resizeBorder;
                bool isBottom = windowPoint.Y >= ActualHeight - resizeBorder;

                if (isTop && isLeft) { handled = true; return new IntPtr(HTTOPLEFT); }
                if (isTop && isRight) { handled = true; return new IntPtr(HTTOPRIGHT); }
                if (isBottom && isLeft) { handled = true; return new IntPtr(HTBOTTOMLEFT); }
                if (isBottom && isRight) { handled = true; return new IntPtr(HTBOTTOMRIGHT); }
                if (isLeft) { handled = true; return new IntPtr(HTLEFT); }
                if (isRight) { handled = true; return new IntPtr(HTRIGHT); }
                if (isTop) { handled = true; return new IntPtr(HTTOP); }
                if (isBottom) { handled = true; return new IntPtr(HTBOTTOM); }
            }
            return IntPtr.Zero;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                BtnMaximize_Click(sender, e);
            }
            else
            {
                if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ==========================================
        // 10. 菜單開立 (AI 食材顏色辨識)
        // ==========================================
        private void RefreshDishGrouping()
        {
            int n = _menuRecipes.Count;
            for (int i = 0; i < n; i++)
            {
                var cur = _menuRecipes[i];
                string curDish = (cur.DishName ?? "").Trim();

                if (string.IsNullOrEmpty(curDish))
                {
                    cur.IsFirstInDishGroup = true;
                    cur.IsLastInDishGroup = true;
                    continue;
                }

                // 是否為該群組第一列
                if (i == 0 || (curDish != (_menuRecipes[i - 1].DishName ?? "").Trim()))
                {
                    cur.IsFirstInDishGroup = true;
                }
                else
                {
                    cur.IsFirstInDishGroup = false;
                }

                // 是否為該群組最後一列
                if (i == n - 1 || (curDish != (_menuRecipes[i + 1].DishName ?? "").Trim()))
                {
                    cur.IsLastInDishGroup = true;
                }
                else
                {
                    cur.IsLastInDishGroup = false;
                }
            }
        }

        private void BtnAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string selectedMeal = "早餐";
            if (CboDefaultMealTime != null && CboDefaultMealTime.SelectedItem is ComboBoxItem cboItem)
            {
                string cboVal = cboItem.Content?.ToString() ?? "早餐";
                if (cboVal != "全部餐次") selectedMeal = cboVal;
            }

            var item = new MenuItemDetail
            {
                MealTime = selectedMeal,
                DishName = "清炒時蔬",
                IngredientName = "高麗菜",
                Category = "蔬菜類",
                ExchangePortion = 1.0,
                RawWeightGrams = 100.0,
                Notes = "",
                MealPlannedSummary = GetMealPlannedPortionsSummary(selectedMeal)
            };
            item.AutoRecognizeColor();
            _menuRecipes.Add(item);
            ApplyMenuRecipeFilter();
            RefreshDishGrouping();
            UpdateColorStatistics();
            TxtStatus.Text = "➕ 已新增一道菜餚食材！請輸入食材名稱，系統將自動 AI 辨識顏色。";
        }

        private void BtnDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MenuItemDetail item)
            {
                _menuRecipes.Remove(item);
                RefreshDishGrouping();
                UpdateColorStatistics();
                TxtStatus.Text = $"🗑️ 已刪除菜色食材【{item.DishName} - {item.IngredientName}】";
            }
        }

        private void BtnClearMenuRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("確定要清空目前開立的所有菜色食材嗎？", "確認清空", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _menuRecipes.Clear();
                RefreshDishGrouping();
                UpdateColorStatistics();
                TxtStatus.Text = "🗑️ 已清空菜單開立內容。";
            }
        }

        private void BtnLoadSampleMenuRecipe_Click(object sender, RoutedEventArgs e)
        {
            ApplySampleMenuRecipes();
            ApplyMenuRecipeFilter();
            ApplyMenuRecipeFilter();
            UpdateColorStatistics();
            TxtStatus.Text = "🌟 已成功載入經典五色均衡菜單！";
        }

        private void ApplySampleMenuRecipes()
        {
            _menuRecipes.Clear();
            var samples = new List<MenuItemDetail>
            {
                new MenuItemDetail { MealTime = "早餐", DishName = "香蕉堅果燕麥粥", IngredientName = "香蕉", Category = "水果類", ExchangePortion = 1.0, RawWeightGrams = 95, Notes = "" },
                new MenuItemDetail { MealTime = "早餐", DishName = "香蕉堅果燕麥粥", IngredientName = "牛奶", Category = "低脂奶", ExchangePortion = 1.0, RawWeightGrams = 240, Notes = "" },
                new MenuItemDetail { MealTime = "早餐", DishName = "香蕉堅果燕麥粥", IngredientName = "燕麥", Category = "全穀雜糧", ExchangePortion = 2.0, RawWeightGrams = 40, Notes = "" },
                new MenuItemDetail { MealTime = "午餐", DishName = "胡蘿蔔炒雞胸肉", IngredientName = "胡蘿蔔", Category = "蔬菜類", ExchangePortion = 1.0, RawWeightGrams = 100, Notes = "" },
                new MenuItemDetail { MealTime = "午餐", DishName = "胡蘿蔔炒雞胸肉", IngredientName = "雞胸肉", Category = "豆魚蛋肉", ExchangePortion = 2.0, RawWeightGrams = 60, Notes = "" },
                new MenuItemDetail { MealTime = "午餐", DishName = "蒜炒菠菜", IngredientName = "菠菜", Category = "蔬菜類", ExchangePortion = 1.0, RawWeightGrams = 100, Notes = "" },
                new MenuItemDetail { MealTime = "晚餐", DishName = "番茄豆腐湯", IngredientName = "番茄", Category = "蔬菜類", ExchangePortion = 1.0, RawWeightGrams = 100, Notes = "" },
                new MenuItemDetail { MealTime = "晚餐", DishName = "番茄豆腐湯", IngredientName = "豆腐", Category = "豆魚蛋肉", ExchangePortion = 1.0, RawWeightGrams = 110, Notes = "" },
                new MenuItemDetail { MealTime = "晚餐", DishName = "清炒高麗菜", IngredientName = "高麗菜", Category = "蔬菜類", ExchangePortion = 1.0, RawWeightGrams = 100, Notes = "" },
                new MenuItemDetail { MealTime = "晚餐", DishName = "紫米飯", IngredientName = "紫米", Category = "全穀雜糧", ExchangePortion = 3.0, RawWeightGrams = 60, Notes = "" }
            };

            foreach (var s in samples)
            {
                s.AutoRecognizeColor();
                s.MealPlannedSummary = GetMealPlannedPortionsSummary(s.MealTime);
                _menuRecipes.Add(s);
            }
            RefreshDishGrouping();
        }

        private void GridMenuRecipe_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Row.Item is MenuItemDetail item)
                {
                    item.AutoRecognizeColor();
                    item.MealPlannedSummary = GetMealPlannedPortionsSummary(item.MealTime);
                }
                RefreshDishGrouping();
                UpdateColorStatistics();
                UpdateMealPlannedSummaries();
            }));
        }

        private void UpdateColorStatistics()
        {
            if (LblColorCountStats == null) return;
            var colorSet = new HashSet<string>();
            foreach (var item in _menuRecipes)
            {
                if (!string.IsNullOrWhiteSpace(item.FoodColor) && item.FoodColor != "天然原色")
                {
                    colorSet.Add(item.FoodColor);
                }
            }
            if (colorSet.Count == 0)
            {
                LblColorCountStats.Text = "目前尚未開立食材色彩";
            }
            else
            {
                LblColorCountStats.Text = $"已開立 {colorSet.Count} 種彩虹植化素顏色：【{string.Join("、", colorSet)}】（共 {_menuRecipes.Count} 項食材）";
            }
        }
        // ==========================================
        // 10.5 各餐次設計份數即時串接 (顯示於菜單開立餐次旁邊)
        // ==========================================
        public string GetMealPlannedPortionsSummary(string mealTime)
        {
            if (_exchanges == null || _exchanges.Count == 0) return "無規劃份數";
            var list = new List<string>();
            foreach (var ex in _exchanges)
            {
                double portion = mealTime switch
                {
                    "早餐" => ex.Breakfast,
                    "早點" => ex.MorningSnack,
                    "午餐" => ex.Lunch,
                    "午點" => ex.AfternoonSnack,
                    "晚餐" => ex.Dinner,
                    "晚點" => ex.NightSnack,
                    _ => 0
                };

                if (portion > 0)
                {
                    string pStr = (portion % 1 == 0) ? portion.ToString("0") : portion.ToString("0.#");
                    list.Add($"{ex.Category} {pStr}份");
                }
            }

            if (list.Count == 0) return "無規劃份數";
            return string.Join("、", list);
        }

        private void UpdateMealPlannedSummaries()
        {
            if (_exchanges == null) return;

            string b = GetMealPlannedPortionsSummary("早餐");
            string ms = GetMealPlannedPortionsSummary("早點");
            string l = GetMealPlannedPortionsSummary("午餐");
            string as_ = GetMealPlannedPortionsSummary("午點");
            string d = GetMealPlannedPortionsSummary("晚餐");
            string ns = GetMealPlannedPortionsSummary("晚點");

            if (TxtBreakfastTarget != null) TxtBreakfastTarget.Text = b;
            if (TxtMorningSnackTarget != null) TxtMorningSnackTarget.Text = ms;
            if (TxtLunchTarget != null) TxtLunchTarget.Text = l;
            if (TxtAfternoonSnackTarget != null) TxtAfternoonSnackTarget.Text = as_;
            if (TxtDinnerTarget != null) TxtDinnerTarget.Text = d;
            if (TxtNightSnackTarget != null) TxtNightSnackTarget.Text = ns;

            if (CboDefaultMealTime != null && CboDefaultMealTime.SelectedItem is ComboBoxItem cboItem)
            {
                string curMeal = cboItem.Content?.ToString() ?? "早餐";
                if (TxtCurrentMealTargetPortions != null)
                {
                    TxtCurrentMealTargetPortions.Text = GetMealPlannedPortionsSummary(curMeal);
                }
            }

            if (_menuRecipes != null)
            {
                foreach (var item in _menuRecipes)
                {
                    item.MealPlannedSummary = GetMealPlannedPortionsSummary(item.MealTime);
                }
            }
        }

        private void CboDefaultMealTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboDefaultMealTime?.SelectedItem is ComboBoxItem cboItem && TxtCurrentMealTargetPortions != null)
            {
                string meal = cboItem.Content?.ToString() ?? "全部餐次";
                if (meal == "全部餐次")
                {
                    TxtCurrentMealTargetPortions.Text = "全天各餐規劃中";
                }
                else
                {
                    TxtCurrentMealTargetPortions.Text = GetMealPlannedPortionsSummary(meal);
                }
            }
            ApplyMenuRecipeFilter();
        }

        private void ApplyMenuRecipeFilter()
        {
            if (GridMenuRecipe == null || CboDefaultMealTime == null) return;
            string selectedMeal = (CboDefaultMealTime.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "全部餐次";
            var view = CollectionViewSource.GetDefaultView(GridMenuRecipe.ItemsSource);
            if (view == null) return;

            if (selectedMeal == "全部餐次" || string.IsNullOrWhiteSpace(selectedMeal))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = obj =>
                {
                    if (obj is MenuItemDetail item)
                    {
                        return item.MealTime == selectedMeal;
                    }
                    return false;
                };
            }
            view.Refresh();
            RefreshDishGrouping();
        }

        private void BtnOpenFoodExchangeQuery_Click(object sender, RoutedEventArgs e)
        {
            var win = new FoodExchangeQueryWindow(_allOfficialFoods);
            win.Owner = this;
            win.OnItemSelected += (item) =>
            {
                AddOfficialFoodToMenu(item);
            };
            win.ShowDialog();
        }

        private void AddOfficialFoodToMenu(FoodExchangeReferenceItem item)
        {
            string defaultMeal = "早餐";
            if (CboDefaultMealTime?.SelectedItem is ComboBoxItem cboItem)
            {
                string cboVal = cboItem.Content?.ToString() ?? "早餐";
                if (cboVal != "全部餐次") defaultMeal = cboVal;
            }

            var detail = new MenuItemDetail
            {
                MealTime = defaultMeal,
                DishName = item.FoodName,
                IngredientName = item.FoodName,
                Category = item.Category,
                ExchangePortion = 1.0,
                RawWeightGrams = item.RawWeightGrams,
                Notes = item.Notes,
                MealPlannedSummary = GetMealPlannedPortionsSummary(defaultMeal)
            };
            detail.AutoRecognizeColor();
            _menuRecipes.Add(detail);
            ApplyMenuRecipeFilter();
            RefreshDishGrouping();
            UpdateColorStatistics();
            TxtStatus.Text = $"➕ 已從衛福部食物代換表帶入【{item.FoodName}】至 {defaultMeal}！";
        }

        private void MealCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string mealName && CboDefaultMealTime != null)
            {
                foreach (ComboBoxItem item in CboDefaultMealTime.Items)
                {
                    if (item.Content?.ToString() == mealName)
                    {
                        CboDefaultMealTime.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // ==========================================
        // ==========================================
        // 10.7 BMI 指標動態位置切換 (五色指標無數字)
        // ==========================================
        private void UpdateBmiIndicator(double bmi)
        {
            _lastCalculatedBmi = bmi;

            if (CanvasBmiPointer == null || BmiDynamicPointer == null || TxtBmiArrow == null || TxtBmiLabel == null)
                return;

            // Reset segment opacities
            if (BmiSeg0 != null) BmiSeg0.Opacity = 0.7;
            if (BmiSeg1 != null) BmiSeg1.Opacity = 0.7;
            if (BmiSeg2 != null) BmiSeg2.Opacity = 0.7;
            if (BmiSeg3 != null) BmiSeg3.Opacity = 0.7;
            if (BmiSeg4 != null) BmiSeg4.Opacity = 0.7;

            // 依據 BMI 精準計算於 5 大區間尺標之水平百分比 (0.0 ~ 1.0)
            // 區間0 (0.00 ~ 0.20): 體重過輕 (< 18.5, 範圍 15.0 ~ 18.5)
            // 區間1 (0.20 ~ 0.40): 正常體重 (18.5 ~ 24.0, 跨度 5.5)
            // 區間2 (0.40 ~ 0.60): 體重過重 (24.0 ~ 27.0, 跨度 3.0)
            // 區間3 (0.60 ~ 0.80): 輕度肥胖 (27.0 ~ 30.0, 跨度 3.0)
            // 區間4 (0.80 ~ 1.00): 中重度肥胖 (30.0 ~ 35.0, 跨度 5.0)
            double ratio;
            string colorHex;

            if (bmi <= 15.0)
            {
                ratio = 0.02;
                colorHex = "#0284C7";
                if (BmiSeg0 != null) BmiSeg0.Opacity = 1.0;
            }
            else if (bmi < 18.5)
            {
                double t = (bmi - 15.0) / 3.5;
                ratio = 0.02 + t * 0.18;
                colorHex = "#0284C7";
                if (BmiSeg0 != null) BmiSeg0.Opacity = 1.0;
            }
            else if (bmi < 24.0)
            {
                double t = (bmi - 18.5) / 5.5;
                ratio = 0.20 + t * 0.20;
                colorHex = "#16A34A";
                if (BmiSeg1 != null) BmiSeg1.Opacity = 1.0;
            }
            else if (bmi < 27.0)
            {
                double t = (bmi - 24.0) / 3.0;
                ratio = 0.40 + t * 0.20;
                colorHex = "#EAB308";
                if (BmiSeg2 != null) BmiSeg2.Opacity = 1.0;
            }
            else if (bmi < 30.0)
            {
                double t = (bmi - 27.0) / 3.0;
                ratio = 0.60 + t * 0.20;
                colorHex = "#F97316";
                if (BmiSeg3 != null) BmiSeg3.Opacity = 1.0;
            }
            else
            {
                double t = Math.Min(1.0, (bmi - 30.0) / 5.0);
                ratio = 0.80 + t * 0.18;
                colorHex = "#EF4444";
                if (BmiSeg4 != null) BmiSeg4.Opacity = 1.0;
            }

            var brush = new BrushConverter().ConvertFromString(colorHex) as Brush ?? Brushes.Green;
            TxtBmiArrow.Foreground = brush;
            TxtBmiLabel.Foreground = brush;

            // 動態定位指針至刻度精確位置
            double rulerWidth = CanvasBmiPointer.ActualWidth;
            if (rulerWidth <= 0) rulerWidth = 450;

            double targetX = ratio * rulerWidth;
            double pointerWidth = BmiDynamicPointer.Width > 0 ? BmiDynamicPointer.Width : 70;
            double left = targetX - (pointerWidth / 2.0);
            left = Math.Clamp(left, 0, Math.Max(0, rulerWidth - pointerWidth));
            Canvas.SetLeft(BmiDynamicPointer, left);
        }

        private void CanvasBmiPointer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBmiIndicator(_lastCalculatedBmi);
        }

        // 10.8 台灣食品營養成分資料庫 (2025版 UPDATE1) 檢索服務
        // ==========================================
        private void LoadFoodCompositionDatabase()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "food_composition_2025.json");
                if (!File.Exists(jsonPath))
                {
                    jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "food_composition_2025.json");
                }

                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    var items = JsonSerializer.Deserialize<List<FoodCompositionItem>>(json);
                    if (items != null)
                    {
                        _allFoodCompositions = items;
                    }
                }
            }
            catch { }

            _filteredFoodCompositions = new ObservableCollection<FoodCompositionItem>(_allFoodCompositions);
            if (GridFoodComposition != null)
            {
                GridFoodComposition.ItemsSource = _filteredFoodCompositions;
            }
            UpdateCompositionFilter();
        }

        private void TxtCompositionSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCompositionFilter();
        }

        private void CboCompositionCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCompositionFilter();
        }

        private void BtnClearCompositionSearch_Click(object sender, RoutedEventArgs e)
        {
            if (TxtCompositionSearch != null) TxtCompositionSearch.Text = "";
            if (CboCompositionCategoryFilter != null) CboCompositionCategoryFilter.SelectedIndex = 0;
            UpdateCompositionFilter();
        }

        private void UpdateCompositionFilter()
        {
            if (_allFoodCompositions == null || _filteredFoodCompositions == null) return;

            string query = (TxtCompositionSearch?.Text ?? "").Trim().ToLower();
            string selectedCat = "全部類別";
            if (CboCompositionCategoryFilter?.SelectedItem is ComboBoxItem cboItem)
            {
                selectedCat = cboItem.Content?.ToString() ?? "全部類別";
            }

            var results = _allFoodCompositions.Where(f =>
            {
                bool matchesCat = selectedCat == "全部類別" || f.Category == selectedCat;
                if (!matchesCat) return false;

                if (string.IsNullOrEmpty(query)) return true;

                return (!string.IsNullOrEmpty(f.Name) && f.Name.ToLower().Contains(query))
                    || (!string.IsNullOrEmpty(f.CommonName) && f.CommonName.ToLower().Contains(query))
                    || (!string.IsNullOrEmpty(f.Code) && f.Code.ToLower().Contains(query))
                    || (!string.IsNullOrEmpty(f.Category) && f.Category.ToLower().Contains(query))
                    || (!string.IsNullOrEmpty(f.Description) && f.Description.ToLower().Contains(query));
            }).ToList();

            _filteredFoodCompositions.Clear();
            foreach (var item in results)
            {
                _filteredFoodCompositions.Add(item);
            }

            if (LblCompositionStats != null)
            {
                LblCompositionStats.Text = $"📋 檢索結果：共 {results.Count:N0} 項食品（總收錄 {_allFoodCompositions.Count:N0} 項官方食品成分）";
            }
        }

        // ==========================================
        // 11. 衛福部食物代換表查詢系統
        // ==========================================
    }
}
