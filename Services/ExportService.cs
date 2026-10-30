using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NutritionPlannerApp.Models;

namespace NutritionPlannerApp.Services
{
    public static class ExportService
    {
        public static string GenerateHtmlReport(
            ClientProfile client,
            List<FoodExchangeItem> exchanges,
            List<CommercialFormula> formulas,
            NutritionSummary summary)
        {
            var sb = new StringBuilder();
            double bmi = NutritionCalculator.CalculateBmi(client.Weight, client.Height);
            double ibw = NutritionCalculator.CalculateIbw(client.Height);
            double ibwPercent = NutritionCalculator.CalculateIbwPercent(client.Weight, client.Height);
            var bmiStatus = NutritionCalculator.GetBmiStatus(bmi);
            bool isFemale = client.Gender == "女";
            double egfr = NutritionCalculator.CalculateEgfr(isFemale, client.Age, client.Creatinine);
            var ckd = NutritionCalculator.GetCkdStage(egfr);
            double bee = NutritionCalculator.CalculateBee(isFemale, client.Weight, client.Height, client.Age);
            double tdee = NutritionCalculator.CalculateTdee(bee, client.ActivityFactor, client.StressFactor);

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='zh-TW'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'/>");
            sb.AppendLine("<title>營養諮詢評估與菜單處方單</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                body {
                    font-family: 'Microsoft JhengHei', 'PingFang TC', sans-serif;
                    background-color: #F8F9FA;
                    color: #2D3748;
                    margin: 0;
                    padding: 24px;
                }
                .container {
                    max-width: 900px;
                    margin: 0 auto;
                    background: #FFFFFF;
                    border-radius: 16px;
                    padding: 32px;
                    box-shadow: 0 10px 25px rgba(0,0,0,0.06);
                    border-top: 8px solid #FF6B8B;
                }
                .header {
                    text-align: center;
                    border-bottom: 2px dashed #E2E8F0;
                    padding-bottom: 20px;
                    margin-bottom: 24px;
                }
                .header h1 {
                    color: #E63946;
                    margin: 0;
                    font-size: 26px;
                }
                .header p {
                    color: #718096;
                    margin: 6px 0 0 0;
                    font-size: 14px;
                }
                .section-title {
                    font-size: 18px;
                    font-weight: bold;
                    color: #1D3557;
                    border-left: 5px solid #FF6B8B;
                    padding-left: 10px;
                    margin: 24px 0 12px 0;
                }
                .grid-2 {
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 16px;
                }
                .card {
                    background: #FDF6F0;
                    border-radius: 12px;
                    padding: 16px;
                    border: 1px solid #FEE5D9;
                }
                .card-blue {
                    background: #EBF8FF;
                    border: 1px solid #BEE3F8;
                }
                .card-green {
                    background: #F0FFF4;
                    border: 1px solid #C6F6D5;
                }
                .badge {
                    display: inline-block;
                    padding: 4px 10px;
                    border-radius: 20px;
                    font-size: 12px;
                    font-weight: bold;
                    color: white;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 10px;
                    font-size: 13px;
                }
                th, td {
                    border: 1px solid #E2E8F0;
                    padding: 8px 10px;
                    text-align: center;
                }
                th {
                    background-color: #FFF0F3;
                    color: #9B2C2C;
                    font-weight: bold;
                }
                tr:nth-child(even) {
                    background-color: #FAFAFA;
                }
                .highlight-row {
                    background-color: #FFF5F5 !important;
                    font-weight: bold;
                }
                .print-btn {
                    display: inline-block;
                    background: linear-gradient(135deg, #FF6B8B, #FF8E72);
                    color: white;
                    padding: 10px 24px;
                    font-size: 14px;
                    font-weight: bold;
                    border-radius: 25px;
                    text-decoration: none;
                    cursor: pointer;
                    margin-bottom: 20px;
                    border: none;
                }
                @media print {
                    body { background: white; padding: 0; }
                    .container { box-shadow: none; border: none; padding: 0; }
                    .print-btn { display: none; }
                }
            ");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<button class='print-btn' onclick='window.print()'>🖨️ 列印此衛教單 / 儲存為 PDF</button>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🥗 營養諮詢評估與飲食菜單處方單</h1>");
            sb.AppendLine($"<p>評估日期：{DateTime.Now:yyyy年MM月dd日 HH:mm} ｜ 系統：可愛風熱量與份數計算系統</p>");
            sb.AppendLine("</div>");

            // Basic Info & Anthropometrics
            sb.AppendLine("<div class='section-title'>👤 個案基本資料與體位生理指標</div>");
            sb.AppendLine("<div class='grid-2'>");
            sb.AppendLine("<div class='card'>");
            sb.AppendLine($"<p><strong>個案姓名：</strong>{client.Name} ｜ <strong>病歷/編號：</strong>{client.ChartNumber}</p>");
            sb.AppendLine($"<p><strong>性別年齡：</strong>{client.Gender}，{client.Age} 歲</p>");
            sb.AppendLine($"<p><strong>身高體重：</strong>{client.Height} cm ｜ {client.Weight} kg</p>");
            sb.AppendLine($"<p><strong>BMI 數值：</strong><span style='font-size:16px; font-weight:bold; color:#E63946;'>{bmi}</span> ({bmiStatus.Category})</p>");
            sb.AppendLine($"<p><strong>理想體重 (IBW)：</strong>{ibw} kg ｜ <strong>IBW%：</strong>{ibwPercent}%</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='card card-blue'>");
            sb.AppendLine($"<p><strong>血清肌酸酐 (Cr)：</strong>{client.Creatinine} mg/dL</p>");
            sb.AppendLine($"<p><strong>腎絲球過濾率 (eGFR)：</strong><span style='font-size:16px; font-weight:bold; color:#2B6CB0;'>{egfr} ml/min/1.73m²</span></p>");
            sb.AppendLine($"<p><strong>慢性腎臟病分期：</strong><span style='font-weight:bold;'>{ckd.Stage}</span> - {ckd.Desc}</p>");
            sb.AppendLine($"<p><strong>基礎代謝率 (BEE)：</strong>{bee} kcal</p>");
            sb.AppendLine($"<p><strong>每日熱量消耗 (TDEE)：</strong><span style='font-size:16px; font-weight:bold; color:#D69E2E;'>{tdee} kcal</span> (活動x{client.ActivityFactor}, 壓力x{client.StressFactor})</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Nutrition Targets vs Results
            sb.AppendLine("<div class='section-title'>🎯 熱量與三大營養素目標達成分析</div>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>項目</th><th>目標建議值</th><th>建議菜單總計</th><th>實際飲食攝取</th><th>熱量達成率</th></tr></thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine($"<tr><td><strong>總熱量 (kcal)</strong></td><td>{summary.TargetCalories:F0} kcal</td><td style='color:#38B000; font-weight:bold;'>{summary.SuggestedCalories:F0} kcal</td><td>{summary.ActualCalories:F0} kcal</td><td>{summary.CalorieAchievementRate}%</td></tr>");
            sb.AppendLine($"<tr><td><strong>蛋白質 (g)</strong></td><td>{summary.TargetProteinGrams:F1} g ({client.ProteinRatio}%)</td><td>{summary.SuggestedProtein:F1} g ({summary.SuggestedProteinRatio}%)</td><td>{summary.ActualProtein:F1} g ({summary.ActualProteinRatio}%)</td><td>-</td></tr>");
            sb.AppendLine($"<tr><td><strong>脂質 (g)</strong></td><td>{summary.TargetFatGrams:F1} g ({client.FatRatio}%)</td><td>{summary.SuggestedFat:F1} g ({summary.SuggestedFatRatio}%)</td><td>{summary.ActualFat:F1} g ({summary.ActualFatRatio}%)</td><td>-</td></tr>");
            sb.AppendLine($"<tr><td><strong>醣類 (g)</strong></td><td>{summary.TargetCarbGrams:F1} g ({client.CarbRatio}%)</td><td>{summary.SuggestedCarbs:F1} g ({summary.SuggestedCarbRatio}%)</td><td>{summary.ActualCarbs:F1} g ({summary.ActualCarbRatio}%)</td><td>-</td></tr>");
            sb.AppendLine("</tbody></table>");

            // Food Exchange Menu Table (6 meals)
            sb.AppendLine("<div class='section-title'>🍱 建議飲食各餐六大類份數分配</div>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>食物類別</th><th>單位說明</th><th>早餐</th><th>早點</th><th>午餐</th><th>午點</th><th>晚餐</th><th>晚點</th><th>總份數</th><th>蛋白質(g)</th><th>脂肪(g)</th><th>醣類(g)</th><th>熱量(kcal)</th></tr></thead>");
            sb.AppendLine("<tbody>");

            double totalExProt = 0, totalExFat = 0, totalExCarb = 0, totalExKcal = 0;
            foreach (var item in exchanges)
            {
                if (item.SuggestedTotalPortions > 0 || item.ActualPortions > 0)
                {
                    totalExProt += item.SuggestedProtein;
                    totalExFat += item.SuggestedFat;
                    totalExCarb += item.SuggestedCarbs;
                    totalExKcal += item.SuggestedCalories;

                    sb.AppendLine($"<tr><td>{item.Icon} {item.Category}</td><td>{item.UnitName}</td><td>{item.Breakfast:0.#}</td><td>{item.MorningSnack:0.#}</td><td>{item.Lunch:0.#}</td><td>{item.AfternoonSnack:0.#}</td><td>{item.Dinner:0.#}</td><td>{item.NightSnack:0.#}</td><td style='font-weight:bold; color:#E63946;'>{item.SuggestedTotalPortions:0.#}</td><td>{item.SuggestedProtein:0.#}</td><td>{item.SuggestedFat:0.#}</td><td>{item.SuggestedCarbs:0.#}</td><td>{item.SuggestedCalories:0.#}</td></tr>");
                }
            }
            sb.AppendLine($"<tr class='highlight-row'><td colspan='8'><strong>六大類食物小計</strong></td><td>-</td><td>{totalExProt:F1}</td><td>{totalExFat:F1}</td><td>{totalExCarb:F1}</td><td>{totalExKcal:F0} kcal</td></tr>");
            sb.AppendLine("</tbody></table>");

            // Commercial Formulas Table
            bool hasFormulas = false;
            foreach (var f in formulas)
            {
                if (f.SuggestedPortions > 0 || f.ActualPortions > 0)
                {
                    hasFormulas = true;
                    break;
                }
            }

            if (hasFormulas)
            {
                sb.AppendLine("<div class='section-title'>🍼 搭配商業營養配方</div>");
                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr><th>配方名稱</th><th>包裝規格</th><th>建議份數/罐數</th><th>蛋白質(g)</th><th>脂肪(g)</th><th>醣類(g)</th><th>熱量(kcal)</th><th>備註用途</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var f in formulas)
                {
                    if (f.SuggestedPortions > 0)
                    {
                        sb.AppendLine($"<tr><td><strong>{f.Name}</strong></td><td>{f.Spec}</td><td style='font-weight:bold; color:#E63946;'>{f.SuggestedPortions:0.#} 份</td><td>{f.SuggestedTotalProtein:0.#}</td><td>{f.SuggestedTotalFat:0.#}</td><td>{f.SuggestedTotalCarbs:0.#}</td><td>{f.SuggestedTotalCalories:0.#}</td><td>{f.Notes}</td></tr>");
                    }
                }
                sb.AppendLine("</tbody></table>");
            }

            // Dietitian Notes & Health Advice
            sb.AppendLine("<div class='section-title'>📝 營養師衛教建議與備註</div>");
            sb.AppendLine("<div class='card card-green'>");
            sb.AppendLine($"<p><strong>💡 體態指引：</strong>{bmiStatus.Advice}</p>");
            sb.AppendLine($"<p><strong>🫘 腎功能飲食叮嚀：</strong>{ckd.NutritionAdvice}</p>");
            if (!string.IsNullOrWhiteSpace(client.Notes))
            {
                sb.AppendLine($"<p><strong>📌 個案專屬備註：</strong>{client.Notes.Replace("\n", "<br/>")}</p>");
            }
            sb.AppendLine("</div>");

            sb.AppendLine("<div style='margin-top: 30px; text-align: right; color:#718096; font-size:13px;'>");
            sb.AppendLine("營養師簽章：___________________ ｜ 日期：____年____月____日");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        public static void OpenReportInBrowser(string htmlContent)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"NutritionReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            File.WriteAllText(tempFile, htmlContent, Encoding.UTF8);
            Process.Start(new ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true
            });
        }
    }
}
