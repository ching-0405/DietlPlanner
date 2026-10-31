// 智慧臨床營養評估與菜單設計系統 - 前端核心邏輯

// 全域狀態
let isDiabeticMode = false;
let isDiseaseMode = false;
let exchangesData = [];
let menuRecipes = [
  { meal: "早餐", dish: "香蕉堅果燕麥粥", food: "香蕉", category: "水果類", ex: 1, rawWeight: 95, color: "黃色" },
  { meal: "早餐", dish: "", food: "牛奶", category: "低脂奶", ex: 1, rawWeight: 240, color: "白色" },
  { meal: "早餐", dish: "", food: "燕麥", category: "全穀雜糧", ex: 2, rawWeight: 40, color: "白色" },
  { meal: "午餐", dish: "胡蘿蔔炒雞胸肉", food: "胡蘿蔔", category: "蔬菜類", ex: 1, rawWeight: 100, color: "橙色" },
  { meal: "午餐", dish: "", food: "雞胸肉", category: "豆魚蛋肉", ex: 2, rawWeight: 60, color: "白色" },
  { meal: "午餐", dish: "蒜炒菠菜", food: "菠菜", category: "蔬菜類", ex: 1, rawWeight: 100, color: "綠色" },
  { meal: "晚餐", dish: "番茄豆腐湯", food: "番茄", category: "蔬菜類", ex: 1, rawWeight: 100, color: "紅色" },
  { meal: "晚餐", dish: "", food: "豆腐", category: "豆魚蛋肉", ex: 1, rawWeight: 110, color: "白色" },
  { meal: "晚餐", dish: "清炒高麗菜", food: "高麗菜", category: "蔬菜類", ex: 1, rawWeight: 100, color: "粉綠/嫩綠" },
  { meal: "晚餐", dish: "紫米飯", food: "紫米", category: "全穀雜糧", ex: 3, rawWeight: 60, color: "紫色/黑色" }
];

document.addEventListener("DOMContentLoaded", () => {
  initTabs();
  initAssessment();
  initExchanges();
  initMenu();
  initFormulas();
  initFoodExchangeQuery();
});

// ==========================================
// 1. 頁籤切換
// ==========================================
function initTabs() {
  const tabs = document.querySelectorAll(".nav-tab");
  tabs.forEach(tab => {
    tab.addEventListener("click", () => {
      tabs.forEach(t => t.classList.remove("active"));
      tab.classList.add("active");
      const target = tab.dataset.tab;
      document.querySelectorAll(".tab-content").forEach(content => {
        content.classList.remove("active");
      });
      document.getElementById(target).classList.add("active");
    });
  });
}

// ==========================================
// 2. 第一頁：個案評估與體位計算
// ==========================================
function initAssessment() {
  const inputs = ["inGender", "inAge", "inHeight", "inWeight", "inActivity", "inStress", "inCreatinine", "inKneeHeight", "inUlnaLength", "inCalfCirc", "inArmCirc"];
  inputs.forEach(id => {
    const el = document.getElementById(id);
    if (el) {
      el.addEventListener("input", calcAssessment);
      el.addEventListener("change", calcAssessment);
    }
  });

  document.getElementById("btnApplyTdee").addEventListener("click", () => {
    const tdeeText = document.getElementById("valTdee").innerText;
    const tdee = parseFloat(tdeeText) || 2000;
    document.getElementById("inTargetKcal").value = Math.round(tdee);
    calcExchangeTotals();
    alert(`已將 TDEE (${Math.round(tdee)} kcal) 套用至第 2 頁目標熱量！`);
  });

  calcAssessment();
}

function calcAssessment() {
  const gender = document.getElementById("inGender").value;
  const age = parseFloat(document.getElementById("inAge").value) || 30;
  const height = parseFloat(document.getElementById("inHeight").value) || 170;
  const weight = parseFloat(document.getElementById("inWeight").value) || 68;
  const cr = parseFloat(document.getElementById("inCreatinine").value) || 0.9;
  const kneeH = parseFloat(document.getElementById("inKneeHeight").value) || 50;
  const ulnaL = parseFloat(document.getElementById("inUlnaLength").value) || 25;
  const calfC = parseFloat(document.getElementById("inCalfCirc").value) || 34;
  const armC = parseFloat(document.getElementById("inArmCirc").value) || 28;

  const hM = height / 100.0;
  const bmi = hM > 0 ? (weight / (hM * hM)) : 0;
  const ibw = 22 * (hM * hM);
  const ibwPercent = ibw > 0 ? (weight / ibw * 100) : 0;
  const minNormalWeight = 18.5 * (hM * hM);
  const maxNormalWeight = 24.0 * (hM * hM);

  // 畫面更新
  document.getElementById("valBmi").innerText = bmi.toFixed(2);
  document.getElementById("valIbw").innerText = `${ibw.toFixed(2)} kg`;
  document.getElementById("valIbwPercent").innerText = `${ibwPercent.toFixed(1)} %`;
  document.getElementById("valBmiRange").innerText = `${minNormalWeight.toFixed(1)} ~ ${maxNormalWeight.toFixed(1)} kg`;

  // BMI 分級標籤
  const badge = document.getElementById("badgeBmiStatus");
  if (bmi < 18.5) {
    badge.innerText = "體重過輕";
    badge.className = "badge";
    badge.style.background = "#e0f2fe";
    badge.style.color = "#0284c7";
  } else if (bmi < 24) {
    badge.innerText = "體重正常 (理想)";
    badge.className = "badge badge-normal";
    badge.style.background = "";
    badge.style.color = "";
  } else if (bmi < 27) {
    badge.innerText = "體重過重";
    badge.className = "badge";
    badge.style.background = "#fef3c7";
    badge.style.color = "#d97706";
  } else if (bmi < 30) {
    badge.innerText = "輕度肥胖";
    badge.className = "badge";
    badge.style.background = "#ffedd5";
    badge.style.color = "#ea580c";
  } else {
    badge.innerText = "中重度肥胖";
    badge.className = "badge";
    badge.style.background = "#fee2e2";
    badge.style.color = "#dc2626";
  }

  // BMI 指標尺標滑動位置更新 (5個區塊連續計算)
  updateBmiPointer(bmi);

  // BEE 計算 (Harris-Benedict 公式) 取整數
  let bee = 0;
  if (gender === "男") {
    bee = 66.47 + (13.75 * weight) + (5.0 * height) - (6.76 * age);
  } else {
    bee = 655.1 + (9.56 * weight) + (1.85 * height) - (4.68 * age);
  }
  const beeInt = Math.round(bee);
  document.getElementById("valBee").innerText = `${beeInt} kcal`;

  // TDEE
  const act = parseFloat(document.getElementById("inActivity").value) || 1.3;
  const stress = parseFloat(document.getElementById("inStress").value) || 1.0;
  const tdee = Math.round(bee * act * stress);
  document.getElementById("valTdee").innerText = `${tdee} kcal`;

  // 臨床速算按鈕動態熱量生成
  renderQuickCalcButtons(ibw, weight);

  // 腎功能 eGFR (2021 CKD-EPI)
  let egfr = 0;
  if (gender === "女") {
    const minVal = Math.min(cr / 0.7, 1);
    const maxVal = Math.max(cr / 0.7, 1);
    egfr = 142 * Math.pow(minVal, -0.241) * Math.pow(maxVal, -1.2) * Math.pow(0.9938, age) * 1.012;
  } else {
    const minVal = Math.min(cr / 0.9, 1);
    const maxVal = Math.max(cr / 0.9, 1);
    egfr = 142 * Math.pow(minVal, -0.302) * Math.pow(maxVal, -1.2) * Math.pow(0.9938, age);
  }
  document.getElementById("valEgfr").innerText = egfr.toFixed(1);
  const badgeEgfr = document.getElementById("badgeEgfr");
  if (egfr >= 90) { badgeEgfr.innerText = "正常 (G1)"; badgeEgfr.className = "badge badge-normal"; }
  else if (egfr >= 60) { badgeEgfr.innerText = "輕度受損 (G2)"; badgeEgfr.className = "badge"; badgeEgfr.style.background = "#fef3c7"; badgeEgfr.style.color = "#d97706"; }
  else if (egfr >= 45) { badgeEgfr.innerText = "輕中度受損 (G3a)"; badgeEgfr.className = "badge"; badgeEgfr.style.background = "#ffedd5"; badgeEgfr.style.color = "#ea580c"; }
  else if (egfr >= 30) { badgeEgfr.innerText = "中重度受損 (G3b)"; badgeEgfr.className = "badge"; badgeEgfr.style.background = "#fee2e2"; badgeEgfr.style.color = "#dc2626"; }
  else if (egfr >= 15) { badgeEgfr.innerText = "重度衰竭 (G4)"; badgeEgfr.className = "badge"; badgeEgfr.style.background = "#fee2e2"; badgeEgfr.style.color = "#991b1b"; }
  else { badgeEgfr.innerText = "末期腎病 (G5)"; badgeEgfr.className = "badge"; badgeEgfr.style.background = "#7f1d1d"; badgeEgfr.style.color = "#ffffff"; }

  // 膝長估身高 (Chumlea)
  let estKneeH = gender === "男"
    ? 64.19 - (0.04 * age) + (2.02 * kneeH)
    : 84.88 - (0.24 * age) + (1.83 * kneeH);
  document.getElementById("valEstKneeHeight").innerText = estKneeH.toFixed(1);

  // 尺骨長估身高
  let estUlnaH = 0;
  if (gender === "男") {
    estUlnaH = 79.2 + (3.6 * ulnaL);
  } else {
    estUlnaH = 73.5 + (3.75 * ulnaL);
  }
  document.getElementById("valEstUlnaHeight").innerText = estUlnaH.toFixed(1);

  // 圈度估體重 (Chumlea)
  let estW = gender === "男"
    ? (kneeH * 1.1) + (armC * 3.07) - 75.81
    : (kneeH * 1.01) + (armC * 2.81) - 66.04;
  if (estW < 30) estW = 60.0;
  document.getElementById("valEstCircWeight").innerText = estW.toFixed(1);
}

function updateBmiPointer(bmi) {
  // 5 個等寬區塊 (各佔 20%)：
  // 0: < 18.5 (15.0..18.5)
  // 1: 18.5 ~ 24.0 (5.5 範圍)
  // 2: 24.0 ~ 27.0 (3.0 範圍)
  // 3: 27.0 ~ 30.0 (3.0 範圍)
  // 4: >= 30.0 (30.0..35.0)
  let blockIndex = 1;
  let ratioInBlock = 0.5;

  if (bmi < 18.5) {
    blockIndex = 0;
    const clamped = Math.max(15.0, Math.min(18.5, bmi));
    ratioInBlock = (clamped - 15.0) / (18.5 - 15.0);
  } else if (bmi < 24.0) {
    blockIndex = 1;
    ratioInBlock = (bmi - 18.5) / (24.0 - 18.5);
  } else if (bmi < 27.0) {
    blockIndex = 2;
    ratioInBlock = (bmi - 24.0) / (27.0 - 24.0);
  } else if (bmi < 30.0) {
    blockIndex = 3;
    ratioInBlock = (bmi - 27.0) / (30.0 - 27.0);
  } else {
    blockIndex = 4;
    const clamped = Math.min(35.0, bmi);
    ratioInBlock = (clamped - 30.0) / (35.0 - 30.0);
  }

  // 總百分比 = (blockIndex + ratioInBlock) * 20%
  const totalPercent = (blockIndex + ratioInBlock) * 20.0;
  const clampedPercent = Math.max(2, Math.min(98, totalPercent));
  const pointer = document.getElementById("bmiPointer");
  if (pointer) {
    pointer.style.left = `${clampedPercent}%`;
  }
}

function renderQuickCalcButtons(ibw, cbw) {
  const ibwContainer = document.getElementById("ibwQuickBtns");
  const cbwContainer = document.getElementById("cbwQuickBtns");
  const factors = [25, 28, 30, 35, 40, 45];

  ibwContainer.innerHTML = [25, 28, 30, 35].map(k => {
    const kcal = Math.round(ibw * k);
    return `<button class="btn-quick-ibw" onclick="applyQuickKcal(${kcal})">IBW x ${k} = ${kcal} kcal</button>`;
  }).join("");

  cbwContainer.innerHTML = factors.map(k => {
    const kcal = Math.round(cbw * k);
    return `<button class="btn-quick-cbw" onclick="applyQuickKcal(${kcal})">CBW x ${k} = ${kcal} kcal</button>`;
  }).join("");
}

function applyQuickKcal(kcal) {
  document.getElementById("inTargetKcal").value = kcal;
  calcExchangeTotals();
  alert(`已將速算熱量 ${kcal} kcal 套用至目標熱量！`);
}

function applyEstHeight(type) {
  const val = type === "knee"
    ? document.getElementById("valEstKneeHeight").innerText
    : document.getElementById("valEstUlnaHeight").innerText;
  document.getElementById("inHeight").value = parseFloat(val);
  calcAssessment();
}

function applyEstWeight() {
  const val = document.getElementById("valEstCircWeight").innerText;
  document.getElementById("inWeight").value = parseFloat(val);
  calcAssessment();
}

// ==========================================
// 3. 第二頁：六大類食物代換設計
// ==========================================
function initExchanges() {
  exchangesData = getDefaultExchanges(false);
  renderExchangeTable();

  // 模式切換按鈕
  const btnDiabetic = document.getElementById("btnDiabeticMode");
  btnDiabetic.addEventListener("click", () => {
    isDiabeticMode = !isDiabeticMode;
    btnDiabetic.classList.toggle("btn-primary", !isDiabeticMode);
    btnDiabetic.classList.toggle("btn-warning", isDiabeticMode);
    exchangesData = getDefaultExchanges(isDiabeticMode);
    renderExchangeTable();
  });

  const btnDisease = document.getElementById("btnDiseaseMode");
  btnDisease.addEventListener("click", () => {
    isDiseaseMode = !isDiseaseMode;
    btnDisease.classList.toggle("active", isDiseaseMode);
  });

  // 目標熱量與比例監聽
  ["inTargetKcal", "inRatioC", "inRatioP", "inRatioF"].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.addEventListener("input", () => {
      calcMacroTargets();
      calcExchangeTotals();
    });
  });

  calcMacroTargets();
}

// 快速套用預設比例
function applyRatioPreset(carb, fat, protein) {
  document.getElementById("inRatioC").value = carb;
  document.getElementById("inRatioF").value = fat;
  document.getElementById("inRatioP").value = protein;
  calcMacroTargets();
  calcExchangeTotals();
}

// 計算三大營養素目標克數與檢核 (原系統介面邏輯)
function calcMacroTargets() {
  const targetKcal = parseFloat(document.getElementById("inTargetKcal").value) || 2000;
  const ratioC = parseFloat(document.getElementById("inRatioC").value) || 0;
  const ratioF = parseFloat(document.getElementById("inRatioF").value) || 0;
  const ratioP = parseFloat(document.getElementById("inRatioP").value) || 0;

  // 同步顯示脂質與蛋白質的目標熱量文字
  const lblSyncF = document.getElementById("lblSyncKcalFat");
  const lblSyncP = document.getElementById("lblSyncKcalProt");
  if (lblSyncF) lblSyncF.innerText = `${Math.round(targetKcal)} kcal`;
  if (lblSyncP) lblSyncP.innerText = `${Math.round(targetKcal)} kcal`;

  // 1. 醣類: kcal * (C%) / 4
  const carbKcal = targetKcal * (ratioC / 100.0);
  const carbGrams = carbKcal / 4.0;
  const elProcC = document.getElementById("lblCalcProcessCarb");
  const elTargetC = document.getElementById("lblTargetCarbGrams");
  if (elProcC) elProcC.innerText = `${Math.round(carbKcal)} ÷ 4 = ${carbGrams.toFixed(1)} g`;
  if (elTargetC) elTargetC.innerText = `${carbGrams.toFixed(1)} g`;

  // 2. 脂質: kcal * (F%) / 9
  const fatKcal = targetKcal * (ratioF / 100.0);
  const fatGrams = fatKcal / 9.0;
  const elProcF = document.getElementById("lblCalcProcessFat");
  const elTargetF = document.getElementById("lblTargetFatGrams");
  if (elProcF) elProcF.innerText = `${Math.round(fatKcal)} ÷ 9 = ${fatGrams.toFixed(1)} g`;
  if (elTargetF) elTargetF.innerText = `${fatGrams.toFixed(1)} g`;

  // 3. 蛋白質: kcal * (P%) / 4
  const protKcal = targetKcal * (ratioP / 100.0);
  const protGrams = protKcal / 4.0;
  const elProcP = document.getElementById("lblCalcProcessProt");
  const elTargetP = document.getElementById("lblTargetProteinGrams");
  if (elProcP) elProcP.innerText = `${Math.round(protKcal)} ÷ 4 = ${protGrams.toFixed(1)} g`;
  if (elTargetP) elTargetP.innerText = `${protGrams.toFixed(1)} g`;

  // 4. 比例總和與檢核
  const totalRatio = Math.round(ratioC + ratioF + ratioP);
  const lblWarning = document.getElementById("lblRatioSumWarning");
  if (lblWarning) {
    if (totalRatio === 100) {
      lblWarning.className = "text-green font-bold";
      lblWarning.innerText = `✨ 比例總和：${totalRatio}% (正常平衡 ｜ 符合標準熱量百分比建議)`;
    } else {
      lblWarning.className = "text-red font-bold";
      lblWarning.innerText = `⚠️ 比例總和：${totalRatio}% (未滿或超出 100%，請調整)`;
    }
  }
}

function renderExchangeTable() {
  const tbody = document.getElementById("exchangeTableBody");
  tbody.innerHTML = exchangesData.map((item, idx) => {
    return `
      <tr>
        <td class="cat-cell">${item.icon} ${item.category}</td>
        <td class="unit-cell">${item.unit}</td>
        <td>${item.p}</td>
        <td>${item.f}</td>
        <td>${item.c}</td>
        <td>${item.kcal}</td>
        <td class="val-blue"><strong id="ex_total_${idx}">${item.ex}</strong></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.b}" oninput="onMealExChanged(${idx}, 'b', this.value)"></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.ms}" oninput="onMealExChanged(${idx}, 'ms', this.value)"></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.l}" oninput="onMealExChanged(${idx}, 'l', this.value)"></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.as}" oninput="onMealExChanged(${idx}, 'as', this.value)"></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.d}" oninput="onMealExChanged(${idx}, 'd', this.value)"></td>
        <td><input type="number" class="cell-input" step="0.5" min="0" value="${item.ns}" oninput="onMealExChanged(${idx}, 'ns', this.value)"></td>
        <td class="val-green" id="row_p_${idx}">0</td>
        <td class="val-orange" id="row_f_${idx}">0</td>
        <td class="val-red" id="row_c_${idx}">0</td>
        <td class="val-blue" id="row_kcal_${idx}">0</td>
      </tr>
    `;
  }).join("");

  calcExchangeTotals();
}

function onMealExChanged(idx, mealKey, val) {
  const num = parseFloat(val) || 0;
  exchangesData[idx][mealKey] = num;
  exchangesData[idx].ex = exchangesData[idx].b + exchangesData[idx].ms + exchangesData[idx].l + exchangesData[idx].as + exchangesData[idx].d + exchangesData[idx].ns;
  document.getElementById(`ex_total_${idx}`).innerText = exchangesData[idx].ex;
  calcExchangeTotals();
  updateMealChipsBar();
}

function calcExchangeTotals() {
  let totalEx = 0, sumB = 0, sumMs = 0, sumL = 0, sumAs = 0, sumD = 0, sumNs = 0;
  let totalP = 0, totalF = 0, totalC = 0, totalKcal = 0;

  exchangesData.forEach((item, idx) => {
    const rowP = item.ex * item.p;
    const rowF = item.ex * item.f;
    const rowC = item.ex * item.c;
    const rowKcal = item.ex * item.kcal;

    totalEx += item.ex;
    sumB += item.b;
    sumMs += item.ms;
    sumL += item.l;
    sumAs += item.as;
    sumD += item.d;
    sumNs += item.ns;

    totalP += rowP;
    totalF += rowF;
    totalC += rowC;
    totalKcal += rowKcal;

    const elP = document.getElementById(`row_p_${idx}`);
    const elF = document.getElementById(`row_f_${idx}`);
    const elC = document.getElementById(`row_c_${idx}`);
    const elK = document.getElementById(`row_kcal_${idx}`);
    if (elP) elP.innerText = rowP.toFixed(1);
    if (elF) elF.innerText = rowF.toFixed(1);
    if (elC) elC.innerText = rowC.toFixed(1);
    if (elK) elK.innerText = Math.round(rowKcal);
  });

  // 更新下方加總列
  document.getElementById("sumTotalEx").innerText = totalEx.toFixed(1);
  document.getElementById("sumB").innerText = sumB.toFixed(1);
  document.getElementById("sumMs").innerText = sumMs.toFixed(1);
  document.getElementById("sumL").innerText = sumL.toFixed(1);
  document.getElementById("sumAs").innerText = sumAs.toFixed(1);
  document.getElementById("sumD").innerText = sumD.toFixed(1);
  document.getElementById("sumNs").innerText = sumNs.toFixed(1);

  document.getElementById("sumProteinGrams").innerText = `${totalP.toFixed(1)} g`;
  document.getElementById("sumFatGrams").innerText = `${totalF.toFixed(1)} g`;
  document.getElementById("sumCarbGrams").innerText = `${totalC.toFixed(1)} g`;
  document.getElementById("sumTotalCalories").innerText = `${Math.round(totalKcal)} kcal`;

  // 4-9-4 修正熱量與百分比
  const macroKcal = (totalC * 4) + (totalP * 4) + (totalF * 9);
  const pctC = macroKcal > 0 ? (totalC * 4 / macroKcal * 100) : 0;
  const pctP = macroKcal > 0 ? (totalP * 4 / macroKcal * 100) : 0;
  const pctF = macroKcal > 0 ? (totalF * 9 / macroKcal * 100) : 0;

  document.getElementById("pctCarb").innerText = `${pctC.toFixed(1)} %`;
  document.getElementById("pctProtein").innerText = `${pctP.toFixed(1)} %`;
  document.getElementById("pctFat").innerText = `${pctF.toFixed(1)} %`;
  document.getElementById("pctTotal").innerText = `${(pctC + pctP + pctF).toFixed(1)} %`;

  document.getElementById("caloricFormulaDesc").innerText = 
    `4-9-4熱量：醣 ${Math.round(totalC)}g×4 (${Math.round(totalC*4)} kcal) + 蛋 ${Math.round(totalP)}g×4 (${Math.round(totalP*4)} kcal) + 脂 ${Math.round(totalF)}g×9 (${Math.round(totalF*9)} kcal) = ${Math.round(macroKcal)} kcal`;

  updateMealChipsBar();
}

// ==========================================
// 4. 第三頁：菜單開立與食材辨識
// ==========================================
function initMenu() {
  document.getElementById("cboMealFilter").addEventListener("change", renderMenuRecipes);
  document.getElementById("btnOpenExchangeQuery").addEventListener("click", () => {
    openModal("modalExchangeQuery");
  });
  document.getElementById("btnAddCustomItem").addEventListener("click", addCustomRecipeItem);
  renderMenuRecipes();
  updateMealChipsBar();
}

function updateMealChipsBar() {
  const container = document.getElementById("mealChipsBar");
  if (!container) return;

  const meals = [
    { key: "b", name: "早餐", cls: "b" },
    { key: "ms", name: "早點", cls: "ms" },
    { key: "l", name: "午餐", cls: "l" },
    { key: "as", name: "午點", cls: "as" },
    { key: "d", name: "晚餐", cls: "d" },
    { key: "ns", name: "晚點", cls: "ns" }
  ];

  container.innerHTML = meals.map(m => {
    const details = exchangesData
      .filter(it => it[m.key] > 0)
      .map(it => `${it.category} ${it[m.key]}份`)
      .join("、");
    const summary = details ? `：${details}` : "：未設定";

    return `
      <div class="meal-chip" onclick="filterByMeal('${m.name}')">
        <span class="meal-badge ${m.cls}">${m.name}</span>
        <span>${summary}</span>
      </div>
    `;
  }).join("");
}

function filterByMeal(mealName) {
  document.getElementById("cboMealFilter").value = mealName;
  renderMenuRecipes();
}

function renderMenuRecipes() {
  const filter = document.getElementById("cboMealFilter").value;
  const tbody = document.getElementById("menuRecipeTableBody");
  
  const filtered = filter === "全部" 
    ? menuRecipes 
    : menuRecipes.filter(r => r.meal === filter);

  tbody.innerHTML = filtered.map((r, i) => {
    const realIndex = menuRecipes.indexOf(r);
    return `
      <tr>
        <td><strong>${r.meal}</strong></td>
        <td><input type="text" class="form-input" value="${r.dish}" placeholder="輸入菜名" onchange="updateDishName(${realIndex}, this.value)"></td>
        <td><strong>${r.food}</strong></td>
        <td>${r.category}</td>
        <td class="val-red"><strong>${r.ex} Ex</strong></td>
        <td>${r.rawWeight} g</td>
        <td><span class="color-tag ${r.color}">${r.color}</span></td>
        <td><button class="btn btn-danger" onclick="deleteRecipeItem(${realIndex})">✖ 刪除</button></td>
      </tr>
    `;
  }).join("");
}

function updateDishName(index, val) {
  if (menuRecipes[index]) menuRecipes[index].dish = val;
}

function deleteRecipeItem(index) {
  menuRecipes.splice(index, 1);
  renderMenuRecipes();
}

function addCustomRecipeItem() {
  const currentMeal = document.getElementById("cboMealFilter").value;
  const meal = currentMeal === "全部" ? "早餐" : currentMeal;
  const food = prompt("請輸入食材名稱：", "蘋果");
  if (!food) return;
  const ex = parseFloat(prompt("請輸入份數 (Ex)：", "1")) || 1;
  const rawW = parseFloat(prompt("請輸入生重 (g)：", "100")) || 100;
  const color = prompt("請輸入食材顏色（黃色/白色/橙色/綠色/紅色/紫色/黑色/粉綠/嫩綠/其他）：", "紅色") || "其他";

  menuRecipes.push({
    meal: meal,
    dish: "",
    food: food,
    category: "自訂食材",
    ex: ex,
    rawWeight: rawW,
    color: color
  });

  renderMenuRecipes();
}

// ==========================================
// 5. 衛福部 261 項食物代換表彈窗查詢
// ==========================================
function initFoodExchangeQuery() {
  const inSearch = document.getElementById("inFoodSearch");
  const cboCat = document.getElementById("cboCategoryFilter");

  inSearch.addEventListener("input", filterFoodQuery);
  cboCat.addEventListener("change", filterFoodQuery);

  filterFoodQuery();
}

function filterFoodQuery() {
  if (typeof FOOD_EXCHANGE_DB === "undefined") return;

  const keyword = document.getElementById("inFoodSearch").value.trim().toLowerCase();
  const cat = document.getElementById("cboCategoryFilter").value;

  const filtered = FOOD_EXCHANGE_DB.filter(item => {
    const matchCat = cat === "全部" || item.category === cat || item.subCategory.includes(cat);
    const matchText = !keyword || item.foodName.toLowerCase().includes(keyword) || item.subCategory.toLowerCase().includes(keyword) || item.notes.toLowerCase().includes(keyword);
    return matchCat && matchText;
  });

  document.getElementById("lblQueryCount").innerText = filtered.length;
  const tbody = document.getElementById("queryResultBody");

  tbody.innerHTML = filtered.map(item => {
    const cooked = item.cookedWeight ? ` (熟 ${item.cookedWeight}g)` : "";
    return `
      <tr ondblclick="addQueryItemToMenu('${item.foodName}', '${item.category}', ${item.rawWeight})">
        <td><strong>${item.category}</strong></td>
        <td>${item.subCategory}</td>
        <td><strong>${item.foodName}</strong></td>
        <td>${item.portionDesc}</td>
        <td class="val-blue">${item.rawWeight} g${cooked}</td>
        <td class="val-red">${item.calories}</td>
        <td>${item.protein}</td>
        <td>${item.fat}</td>
        <td>${item.carbs}</td>
        <td class="text-muted">${item.notes || "—"}</td>
        <td><button class="btn btn-outline" onclick="addQueryItemToMenu('${item.foodName}', '${item.category}', ${item.rawWeight})">加入菜單</button></td>
      </tr>
    `;
  }).join("");
}

function clearFoodSearch() {
  document.getElementById("inFoodSearch").value = "";
  document.getElementById("cboCategoryFilter").value = "全部";
  filterFoodQuery();
}

function addQueryItemToMenu(foodName, category, rawWeight) {
  let meal = document.getElementById("cboMealFilter").value;
  if (meal === "全部") meal = "早餐";

  // AI 顏色自動推估
  let color = "綠色";
  if (foodName.includes("黃") || foodName.includes("香蕉") || foodName.includes("蛋") || foodName.includes("玉米")) color = "黃色";
  else if (foodName.includes("白") || foodName.includes("奶") || foodName.includes("豆腐") || foodName.includes("燕麥") || foodName.includes("魚") || foodName.includes("雞胸")) color = "白色";
  else if (foodName.includes("紅") || foodName.includes("番茄") || foodName.includes("肉") || foodName.includes("牛")) color = "紅色";
  else if (foodName.includes("胡蘿蔔") || foodName.includes("南瓜") || foodName.includes("橘")) color = "橙色";
  else if (foodName.includes("黑") || foodName.includes("紫") || foodName.includes("木耳")) color = "紫色/黑色";
  else if (foodName.includes("高麗菜") || foodName.includes("青江")) color = "粉綠/嫩綠";

  menuRecipes.push({
    meal: meal,
    dish: "",
    food: foodName,
    category: category,
    ex: 1,
    rawWeight: rawWeight,
    color: color
  });

  renderMenuRecipes();
  closeModal("modalExchangeQuery");
  alert(`已將「${foodName}」成功加入【${meal}】菜單！`);
}

// ==========================================
// 6. 商業配方庫彈窗
// ==========================================
function initFormulas() {
  document.getElementById("btnOpenFormulaWindow").addEventListener("click", () => {
    openModal("modalFormulas");
  });

  const tbody = document.getElementById("formulaTableBody");
  if (!tbody || typeof COMMERCIAL_FORMULAS_DB === "undefined") return;

  tbody.innerHTML = COMMERCIAL_FORMULAS_DB.map(f => `
    <tr>
      <td><strong>${f.name}</strong></td>
      <td>${f.spec}</td>
      <td class="val-green">${f.p} g</td>
      <td class="val-orange">${f.f} g</td>
      <td class="val-red">${f.c} g</td>
      <td class="val-blue"><strong>${f.kcal} kcal</strong></td>
      <td class="text-muted">${f.note}</td>
    </tr>
  `).join("");
}

// 通用 Modal
function openModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.add("show");
}

function closeModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.remove("show");
}
