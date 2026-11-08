// 飲食營養菜單設計系統 - 前端核心邏輯

// 全域狀態
let isDiabeticMode = false;
let isDiseaseMode = false;
let lastRatioLimitSignature = "";
let ratioLimitDialogReturnFocus = null;
let currentDeviceMode = "auto";
let exchangesData = [];
let activeMealKeys = ["b", "l", "d"];
const mealKeyOrder = ["b", "ms", "l", "as", "d", "ns"];
const mealCountMap = {
  3: ["b", "l", "d"],
  4: ["b", "ms", "l", "d"],
  5: ["b", "ms", "l", "as", "d"],
  6: ["b", "ms", "l", "as", "d", "ns"]
};
let menuRecipes = [
  { meal: "早餐", dish: "香蕉堅果燕麥粥", food: "香蕉", category: "水果類", ex: 1, rawWeight: 95, color: "黃色" },
  { meal: "早餐", dish: "", food: "牛奶", category: "低脂奶", ex: 1, rawWeight: 240, color: "白色" },
  { meal: "早餐", dish: "", food: "燕麥", category: "全穀雜糧", ex: 2, rawWeight: 40, color: "白色" },
  { meal: "午餐", dish: "胡蘿蔔炒雞胸肉", food: "胡蘿蔔", category: "蔬菜類", ex: 1, rawWeight: 100, color: "橙色" },
  { meal: "午餐", dish: "", food: "雞胸肉", category: "豆魚蛋肉", ex: 2, rawWeight: 60, color: "白色" },
  { meal: "午餐", dish: "蒜炒菠菜", food: "菠菜", category: "蔬菜類", ex: 1, rawWeight: 100, color: "綠色" },
  { meal: "晚餐", dish: "番茄豆腐湯", food: "番茄", category: "蔬菜類", ex: 1, rawWeight: 100, color: "紅色" },
  { meal: "晚餐", dish: "", food: "豆腐", category: "豆魚蛋肉", ex: 1, rawWeight: 110, color: "白色", sampleName: "傳統豆腐" },
  { meal: "晚餐", dish: "清炒高麗菜", food: "高麗菜", category: "蔬菜類", ex: 1, rawWeight: 100, color: "粉綠/嫩綠" },
  { meal: "晚餐", dish: "紫米飯", food: "紫米", category: "全穀雜糧", ex: 3, rawWeight: 60, color: "紫色/黑色" }
];

document.addEventListener("DOMContentLoaded", () => {
  initDeviceMode();
  initTabs();
  initAssessment();
  initExchanges();
  initMenu();
  initFormulas();
  initFoodExchangeQuery();
  initFoodComposition();
});

// ==========================================
// 0. 手機版 / 電腦版自動辨識與切換
// ==========================================
function initDeviceMode() {
  const isMobileDevice = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) || window.innerWidth < 768;
  setDeviceMode(isMobileDevice ? "mobile" : "desktop", false);

  window.addEventListener("resize", () => {
    if (currentDeviceMode === "auto") {
      const isMobile = window.innerWidth < 768;
      applyDeviceModeClass(isMobile);
    }
  });
}

function setDeviceMode(mode, userExplicit = true) {
  if (userExplicit) currentDeviceMode = mode;
  const isMobile = mode === "mobile";

  applyDeviceModeClass(isMobile);

  const btnDesktop = document.getElementById("btnModeDesktop");
  const btnMobile = document.getElementById("btnModeMobile");
  if (btnDesktop && btnMobile) {
    btnDesktop.classList.toggle("active", !isMobile);
    btnMobile.classList.toggle("active", isMobile);
  }
}

function applyDeviceModeClass(isMobile) {
  if (isMobile) {
    document.body.classList.add("mobile-mode");
  } else {
    document.body.classList.remove("mobile-mode");
  }
  if (typeof updateSubtotalColspans === "function") {
    updateSubtotalColspans();
  }
}

// ==========================================
// 1. 頁籤切換 (同步頂部頁籤與電腦版左側導航)
// ==========================================
function switchTab(target) {
  // 同步頂部頁籤
  document.querySelectorAll(".nav-tab").forEach(t => {
    t.classList.toggle("active", t.dataset.tab === target);
  });
  // 同步左側面板頁籤
  document.querySelectorAll(".sidebar-nav-item").forEach(item => {
    item.classList.toggle("active", item.dataset.tab === target);
  });
  // 切換內容容器
  document.querySelectorAll(".tab-content").forEach(content => {
    content.classList.toggle("active", content.id === target);
  });
}

function initTabs() {
  const tabs = document.querySelectorAll(".nav-tab");
  tabs.forEach(tab => {
    tab.addEventListener("click", () => {
      switchTab(tab.dataset.tab);
    });
  });

  const sidebarItems = document.querySelectorAll(".sidebar-nav-item");
  sidebarItems.forEach(item => {
    item.addEventListener("click", () => {
      switchTab(item.dataset.tab);
    });
  });
}

// ==========================================
// 2. 第一頁：個案評估與體位計算
// ==========================================
function initAssessment() {
  const inputs = ["inName", "inChartNo", "inGender", "inAge", "inHeight", "inWeight", "inActivity", "inStress", "inCreatinine", "inKneeHeight", "inUlnaLength", "inCalfCirc", "inArmCirc"];
  inputs.forEach(id => {
    const el = document.getElementById(id);
    if (el) {
      el.addEventListener("input", () => {
        calcAssessment();
        syncSidebarClientCard();
      });
      el.addEventListener("change", () => {
        calcAssessment();
        syncSidebarClientCard();
      });
    }
  });

  const inTargetKcal = document.getElementById("inTargetKcal");
  if (inTargetKcal) {
    inTargetKcal.addEventListener("input", syncSidebarClientCard);
  }

  document.getElementById("btnApplyTdee").addEventListener("click", () => {
    const tdeeText = document.getElementById("valTdee").innerText;
    const tdee = parseFloat(tdeeText) || 2000;
    document.getElementById("inTargetKcal").value = Math.round(tdee);
    calcExchangeTotals();
    syncSidebarClientCard();
    alert(`已將 TDEE (${Math.round(tdee)} kcal) 套用至第 2 頁目標熱量！`);
  });

  calcAssessment();
  syncSidebarClientCard();
}

function syncSidebarClientCard() {
  const name = document.getElementById("inName")?.value || "個案";
  const chartNo = document.getElementById("inChartNo")?.value || "P001";
  const age = document.getElementById("inAge")?.value || "30";
  const gender = document.getElementById("inGender")?.value || "男";
  const targetKcal = document.getElementById("inTargetKcal")?.value || "2000";

  const elName = document.getElementById("sidebarClientName");
  const elKcal = document.getElementById("sidebarTargetKcal");
  if (elName) elName.innerText = `${name} (${chartNo}, ${age}歲 ${gender})`;
  if (elKcal) elKcal.innerText = `${targetKcal} kcal`;
}

function resetNewClient() {
  if (confirm("確定要建立新個案嗎？將會清空當前輸入內容。")) {
    document.getElementById("inName").value = "新個案";
    document.getElementById("inChartNo").value = "P002";
    document.getElementById("inAge").value = "30";
    document.getElementById("inHeight").value = "170";
    document.getElementById("inWeight").value = "65";
    document.getElementById("inTargetKcal").value = "1800";
    calcAssessment();
    calcExchangeTotals();
    syncSidebarClientCard();
    switchTab("tab-assessment");
  }
}

function saveClientData() {
  const name = document.getElementById("inName")?.value || "個案";
  alert(`個案【${name}】的評估與菜單資料已成功儲存！`);
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
  document.getElementById("valIbw").innerText = `${ibw.toFixed(2)}`;
  document.getElementById("valIbwPercent").innerText = `${Math.round(ibwPercent)} %`;
  document.getElementById("valBmiRange").innerText = `${minNormalWeight.toFixed(1)} ~ ${maxNormalWeight.toFixed(1)} kg`;

  // BMI 數值顏色與分級標籤 (正常不顯示紅色，改為健康深綠色)
  const badge = document.getElementById("badgeBmiStatus");
  const cellBmiVal = document.getElementById("cellBmiVal");
  if (bmi < 18.5) {
    badge.innerText = "體重過輕";
    badge.className = "badge";
    badge.style.background = "#e0f2fe";
    badge.style.color = "#0284c7";
    if (cellBmiVal) { cellBmiVal.className = "val-highlight"; cellBmiVal.querySelector("strong").style.color = "#0284c7"; }
  } else if (bmi < 24) {
    badge.innerText = "體重正常 (理想)";
    badge.className = "badge badge-normal";
    badge.style.background = "";
    badge.style.color = "";
    if (cellBmiVal) { cellBmiVal.className = "val-highlight green"; cellBmiVal.querySelector("strong").style.color = "#16a34a"; }
  } else if (bmi < 27) {
    badge.innerText = "體重過重";
    badge.className = "badge";
    badge.style.background = "#fef3c7";
    badge.style.color = "#d97706";
    if (cellBmiVal) { cellBmiVal.className = "val-highlight"; cellBmiVal.querySelector("strong").style.color = "#d97706"; }
  } else if (bmi < 30) {
    badge.innerText = "輕度肥胖";
    badge.className = "badge";
    badge.style.background = "#ffedd5";
    badge.style.color = "#ea580c";
    if (cellBmiVal) { cellBmiVal.className = "val-highlight"; cellBmiVal.querySelector("strong").style.color = "#ea580c"; }
  } else {
    badge.innerText = "中重度肥胖";
    badge.className = "badge";
    badge.style.background = "#fee2e2";
    badge.style.color = "#dc2626";
    if (cellBmiVal) { cellBmiVal.className = "val-highlight red"; cellBmiVal.querySelector("strong").style.color = "#dc2626"; }
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
  initMealCountSelector();
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

function toggleDiseaseMode() {
  const button = document.getElementById("btnDiseaseMode");
  isDiseaseMode = !isDiseaseMode;
  if (button) button.classList.toggle("active", isDiseaseMode);
  updateDiseaseModeUI(button);
  calcMacroTargets();
  calcExchangeTotals();
}

function updateDiseaseModeUI(button) {
  const ratioTable = document.querySelector(".ratio-calc-grid");
  const status = document.getElementById("lblDiseaseDietStatus");
  const recommendationCells = document.querySelectorAll(".macro-recommendation");

  if (ratioTable) ratioTable.classList.toggle("disease-mode", isDiseaseMode);
  recommendationCells.forEach(cell => {
    cell.hidden = isDiseaseMode;
    cell.setAttribute("aria-hidden", isDiseaseMode ? "true" : "false");
  });
  if (button) {
    button.innerText = isDiseaseMode
      ? "疾病飲食模式 (已解除百分比限制)"
      : "疾病飲食模式 (解除百分比限制)";
  }
  if (status) {
    status.innerText = isDiseaseMode
      ? "疾病飲食模式：不受熱量百分比建議限制"
      : "";
  }
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

  // 4. 比例總和與超標檢核
  const totalRatio = Math.round(ratioC + ratioF + ratioP);
  const lblWarning = document.getElementById("lblRatioSumWarning");
  
  // 檢查個別營養素是否超出建議區間 (醣類 50~60%, 脂質 20~30%, 蛋白質 10~20%)
  const warnings = [];
  const limitWarnings = [];
  if (!isDiseaseMode) {
    if (ratioC < 50 || ratioC > 60) warnings.push(`醣類 (${ratioC}%) 未達或超出建議區間 50~60%`);
    if (ratioF < 20 || ratioF > 30) warnings.push(`脂質 (${ratioF}%) 未達或超出建議區間 20~30%`);
    if (ratioP < 10 || ratioP > 20) warnings.push(`蛋白質 (${ratioP}%) 未達或超出建議區間 10~20%`);
    if (ratioC > 60) limitWarnings.push({ key: "carb", label: "醣類", ratio: ratioC, max: 60 });
    if (ratioF > 30) limitWarnings.push({ key: "fat", label: "脂質", ratio: ratioF, max: 30 });
    if (ratioP > 20) limitWarnings.push({ key: "protein", label: "蛋白質", ratio: ratioP, max: 20 });
  }

  updateRatioLimitDialog(limitWarnings);

  if (lblWarning) {
    if (totalRatio === 100 && warnings.length === 0) {
      lblWarning.className = "text-green font-bold";
      lblWarning.innerText = isDiseaseMode
        ? `✨ 比例總和：${totalRatio}% (疾病飲食模式 ｜ 不受標準熱量百分比建議限制)`
        : `✨ 比例總和：${totalRatio}% (正常平衡 ｜ 符合標準熱量百分比建議)`;
      hideRatioAlert();
    } else if (totalRatio !== 100) {
      lblWarning.className = "text-red font-bold";
      lblWarning.innerText = `⚠️ 比例總和：${totalRatio}% (未滿或超出 100%，請調整)`;
      if (limitWarnings.length === 0) {
        showRatioAlert(`比例總和為 ${totalRatio}%（未達或超出 100%），請檢查並重新調整！`);
      } else {
        hideRatioAlert();
      }
    } else {
      lblWarning.className = "text-orange font-bold";
      lblWarning.innerText = `⚠️ 比例總和：100%（但 ${warnings.join('；')}）`;
      if (limitWarnings.length === 0) {
        showRatioAlert(`三大營養素百分比超出標準建議範圍：${warnings.join('、')}`);
      } else {
        hideRatioAlert();
      }
    }
  }
}

function updateRatioLimitDialog(warnings) {
  const dialog = document.getElementById("ratioLimitDialog");
  const list = document.getElementById("ratioLimitList");
  if (!dialog || !list) return;

  const signature = warnings
    .map(warning => `${warning.key}:${warning.ratio}`)
    .join(",");
  if (warnings.length === 0) {
    lastRatioLimitSignature = "";
    hideRatioLimitDialog();
    return;
  }

  if (signature !== lastRatioLimitSignature) {
    lastRatioLimitSignature = signature;
    showRatioLimitDialog();
  }

  if (dialog.classList.contains("show")) {
    list.replaceChildren(...warnings.map(warning => {
      const item = document.createElement("li");
      item.textContent = `${warning.label}目前設定 ${warning.ratio}%；建議上限為 ${warning.max}%`;
      return item;
    }));
  }
}

function showRatioLimitDialog() {
  const dialog = document.getElementById("ratioLimitDialog");
  if (!dialog) return;

  if (!dialog.classList.contains("show")) {
    ratioLimitDialogReturnFocus = document.activeElement;
    dialog.classList.add("show");
    const closeButton = document.getElementById("ratioLimitCloseButton");
    if (closeButton) closeButton.focus();
  }
}

function hideRatioLimitDialog() {
  const dialog = document.getElementById("ratioLimitDialog");
  const wasOpen = dialog && dialog.classList.contains("show");
  if (dialog) dialog.classList.remove("show");

  const returnFocus = ratioLimitDialogReturnFocus;
  ratioLimitDialogReturnFocus = null;
  if (wasOpen && returnFocus && typeof returnFocus.focus === "function") returnFocus.focus();
}

function closeRatioLimitDialog() {
  hideRatioLimitDialog();
}

// 警示橫幅彈窗控制
function showRatioAlert(msg) {
  const banner = document.getElementById("ratioAlertBanner");
  const msgEl = document.getElementById("ratioAlertMsg");
  if (banner && msgEl) {
    msgEl.innerText = msg;
    banner.style.display = "flex";
  }
}

function hideRatioAlert() {
  const banner = document.getElementById("ratioAlertBanner");
  if (banner) banner.style.display = "none";
}

function closeRatioAlert() {
  hideRatioAlert();
}

function renderExchangeTable() {
  const tbody = document.getElementById("exchangeTableBody");
  tbody.innerHTML = exchangesData.map((item, idx) => {
    return `
      <tr>
        <td class="cat-cell">${item.icon} ${item.category}</td>
        <td class="col-macro-cell">${item.p}</td>
        <td class="col-macro-cell">${item.f}</td>
        <td class="col-macro-cell">${item.c}</td>
        <td class="col-macro-cell">${item.kcal}</td>
        <td class="design-ex-cell">
          <input type="number" class="cell-input design-ex-input" step="any" min="0"
            value="${item.ex}" aria-label="${item.category}設計份數"
            oninput="onDesignExChanged(${idx}, this.value)">
          <span class="allocation-status" id="row_allocation_status_${idx}" hidden></span>
        </td>
        ${renderMealInput(item, idx, "b")}
        ${renderMealInput(item, idx, "ms")}
        ${renderMealInput(item, idx, "l")}
        ${renderMealInput(item, idx, "as")}
        ${renderMealInput(item, idx, "d")}
        ${renderMealInput(item, idx, "ns")}
        <td class="val-green" id="row_p_${idx}">0</td>
        <td class="val-orange" id="row_f_${idx}">0</td>
        <td class="val-red" id="row_c_${idx}">0</td>
        <td class="val-blue" id="row_kcal_${idx}">0</td>
      </tr>
    `;
  }).join("");

  exchangesData.forEach((_, idx) => validateMealAllocation(idx));
  updateMealCountUI();
  renderMobileMacroSpecsCard();
  updateSubtotalColspans();
  calcExchangeTotals();
}


function updateSubtotalColspans() {
  const isMobile = document.body.classList.contains("mobile-mode");
  const titles = document.querySelectorAll(".subtotal-tbody .subtotal-title");
  titles.forEach(td => {
    td.colSpan = isMobile ? 1 : 5;
  });
}

function renderMobileMacroSpecsCard() {
  const container = document.getElementById("mobileMacroSpecsList");
  if (!container || !Array.isArray(exchangesData)) return;

  container.innerHTML = `
    <div class="mobile-specs-table-wrapper">
      <table class="mobile-specs-table">
        <thead>
          <tr>
            <th class="spec-th-cat">食物大類</th>
            <th class="spec-th-macro th-p">蛋白質 (P) (g)</th>
            <th class="spec-th-macro th-f">脂肪 (F) (g)</th>
            <th class="spec-th-macro th-c">醣類 (C) (g)</th>
            <th class="spec-th-kcal th-k">熱量 (kcal)</th>
          </tr>
        </thead>
        <tbody>
          ${exchangesData.map(item => `
            <tr>
              <td class="spec-td-cat">
                <span class="cat-icon">${item.icon}</span>
                <span class="cat-name">${item.category}</span>
              </td>
              <td class="spec-td-val val-green font-bold">${item.p}</td>
              <td class="spec-td-val val-orange font-bold">${item.f}</td>
              <td class="spec-td-val val-red font-bold">${item.c}</td>
              <td class="spec-td-val val-blue font-bold">${item.kcal}</td>
            </tr>
          `).join("")}
        </tbody>
      </table>
    </div>
  `;
}

function renderMealInput(item, idx, mealKey) {
  return `<td class="meal-col meal-${mealKey}"><input type="number" class="cell-input meal-ex-input" step="any" min="0" value="${item[mealKey]}" ${item.ex > 0 && activeMealKeys.includes(mealKey) ? "" : "disabled"} oninput="onMealExChanged(${idx}, '${mealKey}', this.value)"></td>`;
}

function initMealCountSelector() {
  const selector = document.getElementById("mealCountSelect");
  if (!selector) return;
  selector.addEventListener("change", () => {
    const count = Number(selector.value);
    activeMealKeys = mealCountMap[count] || mealKeyOrder;
    exchangesData.forEach(item => {
      mealKeyOrder.filter(key => !activeMealKeys.includes(key)).forEach(key => {
        item[key] = 0;
      });
    });
    renderExchangeTable();
  });
}

function updateMealCountUI() {
  const selector = document.getElementById("mealCountSelect");
  const count = activeMealKeys.length;
  if (selector && Number(selector.value) !== count) selector.value = String(count);
  const mealHeader = document.querySelector("#exchangeTable .col-meals");
  if (mealHeader) mealHeader.colSpan = count;
  mealKeyOrder.forEach(key => {
    const visible = activeMealKeys.includes(key);
    document.querySelectorAll(`.meal-col.meal-${key}, .meal-summary-${key}`).forEach(el => {
      el.hidden = !visible;
    });
  });
  const placeholder = document.getElementById("mealSummaryPlaceholder");
  if (placeholder) placeholder.colSpan = count;
}

function onMealExChanged(idx, mealKey, val) {
  const num = parseFloat(val) || 0;
  exchangesData[idx][mealKey] = num;
  validateMealAllocation(idx);
  calcExchangeTotals();
  updateMealChipsBar();
}

function onDesignExChanged(idx, val) {
  const num = parseFloat(val);
  exchangesData[idx].ex = Number.isFinite(num) && num >= 0 ? num : 0;
  const item = exchangesData[idx];
  const mealKeys = ["b", "ms", "l", "as", "d", "ns"];
  const row = document.querySelectorAll("#exchangeTableBody tr")[idx];
  if (row) {
    row.querySelectorAll(".meal-ex-input").forEach(input => {
      input.disabled = item.ex <= 0;
    });
  }
  if (item.ex <= 0) {
    mealKeys.forEach(key => {
      item[key] = 0;
        const input = row?.querySelector(`input[oninput*="'${key}'"]`);
      if (input) input.value = "0";
    });
  }
  validateMealAllocation(idx);
  calcExchangeTotals();
}

function validateMealAllocation(idx) {
  const item = exchangesData[idx];
  const mealTotal = activeMealKeys.reduce((total, key) => total + item[key], 0);
  const isValid = item.ex <= 0 || Math.abs(mealTotal - item.ex) < 0.001;
  const row = document.querySelectorAll("#exchangeTableBody tr")[idx];
  const status = document.getElementById(`row_allocation_status_${idx}`);
  if (row) row.classList.toggle("allocation-invalid", !isValid);
  if (status) {
    status.hidden = isValid;
    status.textContent = `餐次合計需等於設計份數 ${item.ex}`;
    status.title = "早、早點、午、午點、晚、晚點的合計必須等於設計份數";
  }
  return isValid;
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
    if (activeMealKeys.includes("b")) sumB += item.b;
    if (activeMealKeys.includes("ms")) sumMs += item.ms;
    if (activeMealKeys.includes("l")) sumL += item.l;
    if (activeMealKeys.includes("as")) sumAs += item.as;
    if (activeMealKeys.includes("d")) sumD += item.d;
    if (activeMealKeys.includes("ns")) sumNs += item.ns;

    totalP += rowP;
    totalF += rowF;
    totalC += rowC;
    totalKcal += rowKcal;

    const elP = document.getElementById(`row_p_${idx}`);
    const elF = document.getElementById(`row_f_${idx}`);
    const elC = document.getElementById(`row_c_${idx}`);
    const elK = document.getElementById(`row_kcal_${idx}`);
    if (elP) elP.innerText = Math.round(rowP);
    if (elF) elF.innerText = Math.round(rowF);
    if (elC) elC.innerText = Math.round(rowC);
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

  document.getElementById("sumProteinGrams").innerText = `${Math.round(totalP)}`;
  document.getElementById("sumFatGrams").innerText = `${Math.round(totalF)}`;
  document.getElementById("sumCarbGrams").innerText = `${Math.round(totalC)}`;
  document.getElementById("sumTotalCalories").innerText = `${Math.round(totalKcal)}`;

  // 4-9-4 修正熱量與百分比
  const macroKcal = (totalC * 4) + (totalP * 4) + (totalF * 9);
  const pctC = macroKcal > 0 ? (totalC * 4 / macroKcal * 100) : 0;
  const pctP = macroKcal > 0 ? (totalP * 4 / macroKcal * 100) : 0;
  const pctF = macroKcal > 0 ? (totalF * 9 / macroKcal * 100) : 0;

  document.getElementById("pctCarb").innerText = `${pctC.toFixed(1)} %`;
  document.getElementById("pctProtein").innerText = `${pctP.toFixed(1)} %`;
  document.getElementById("pctFat").innerText = `${pctF.toFixed(1)} %`;
  document.getElementById("pctTotal").innerText = `${Math.round(macroKcal)} kcal`;

  const formulaEl = document.getElementById("caloricFormulaDesc");
  if (formulaEl) formulaEl.innerText = "";

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

  const mealOptions = ["早餐", "早點", "午餐", "午點", "晚餐", "晚點"];

  let totalEx = 0;
  let totalWeight = 0;
  let totalKcal = 0;
  let totalP = 0;
  let totalF = 0;
  let totalC = 0;
  const colorCountMap = {};

  tbody.innerHTML = filtered.map((r, i) => {
    const realIndex = menuRecipes.indexOf(r);
    
    // AI 自動辨識顏色（隨食材名稱與類別自動精準推算）
    r.color = detectFoodColor(r.food, r.category);
    const colorClass = r.color ? r.color.replace("/", "\/") : "白色";

    // 依據生重克數比對食品成分資料庫 (每 100g 含量精確換算)
    const nutrient = calculateRecipeNutrients(r.food, r.category, r.rawWeight, r.ex, r.sampleName);
    r._calc = nutrient;

    const rowEx = Number(r.ex) || 0;
    const rowWeight = Number(r.rawWeight) || 0;
    totalEx += rowEx;
    totalWeight += rowWeight;
    totalKcal += nutrient.kcal;
    totalP += nutrient.protein;
    totalF += nutrient.fat;
    totalC += nutrient.carbs;

    if (r.color) {
      colorCountMap[r.color] = (colorCountMap[r.color] || 0) + 1;
    }

    const mealSelectHtml = `
      <select class="form-select" style="padding: 4px 6px; font-size: 13px; font-weight: 600;" onchange="updateRecipeMeal(${realIndex}, this.value)">
        ${mealOptions.map(m => `<option value="${m}" ${m === r.meal ? "selected" : ""}>${m}</option>`).join("")}
      </select>
    `;

    // 食材顏色：完全由 AI 自動辨識純色膠囊呈現，絕非手動下拉
    const colorTagHtml = `
      <span class="color-tag ${colorClass}" title="AI 自動辨識：${r.color}">${r.color}</span>
    `;

    return `
      <tr data-recipe-index="${realIndex}">
        <td style="width: 125px;">
          <div style="display: flex; align-items: center; gap: 4px;">
            <button type="button" class="btn btn-teal" style="padding: 2px 7px; font-size: 13px; line-height: 1.2; border-radius: 4px; flex-shrink: 0;" title="在下方新增一列餐次/菜名/食材" onclick="insertRecipeItemAfter(${realIndex})">➕</button>
            <div style="flex: 1;">${mealSelectHtml}</div>
          </div>
        </td>
        <td>
          <input type="text" class="form-input" style="padding: 4px 8px; font-size: 13px;" value="${r.dish || ""}" placeholder="輸入菜名" 
            oninput="updateDishName(${realIndex}, this.value)">
        </td>
        <td>
          <input type="text" class="form-input" style="padding: 4px 8px; font-size: 13px; font-weight: 600;" value="${r.food || ""}" placeholder="輸入食材" 
            oninput="updateFoodName(${realIndex}, this.value)" onchange="updateFoodNameAndRender(${realIndex}, this.value)">
        </td>
        <td><span class="text-teal font-bold">${r.category || "自訂食材"}</span></td>
        <td class="val-red">
          <input type="number" class="form-input" style="width: 70px; text-align: center; padding: 4px;" step="any" min="0.1" 
            value="${r.ex}" oninput="updateRecipeEx(${realIndex}, this.value)">
        </td>
        <td>
          <input type="number" class="form-input" style="width: 80px; text-align: center; padding: 4px;" step="1" min="1" 
            value="${r.rawWeight}" oninput="updateRecipeRawWeight(${realIndex}, this.value)">
        </td>
        <td style="text-align: center;">${colorTagHtml}</td>
        <td style="text-align: center;"><button class="btn btn-danger" style="padding: 4px 8px; font-size: 12px;" onclick="deleteRecipeItem(${realIndex})">✖ 刪除</button></td>
      </tr>
    `;
  }).join("");

  // 更新表格 Footer 加總列
  const elEx = document.getElementById("recipeSumEx");
  const elWeight = document.getElementById("recipeSumWeight");
  const elKcal = document.getElementById("recipeSumCalories");
  const elP = document.getElementById("recipeSumProtein");
  const elF = document.getElementById("recipeSumFat");
  const elC = document.getElementById("recipeSumCarbs");
  const elColorSummary = document.getElementById("recipeColorSummary");

  if (elEx) elEx.innerText = totalEx.toFixed(1);
  if (elWeight) elWeight.innerText = `${Math.round(totalWeight)} g`;
  if (elKcal) elKcal.innerText = totalKcal.toFixed(1);
  if (elP) elP.innerText = `${totalP.toFixed(1)} g`;
  if (elF) elF.innerText = `${totalF.toFixed(1)} g`;
  if (elC) elC.innerText = `${totalC.toFixed(1)} g`;

  const distinctColors = Object.keys(colorCountMap);
  if (elColorSummary) {
    elColorSummary.innerText = distinctColors.length > 0 ? `${distinctColors.length} 種色彩` : "—";
  }

  // 更新下方卡片與熱量佔比
  const cardKcal = document.getElementById("cardRecipeTotalKcal");
  const cardP = document.getElementById("cardRecipeTotalProtein");
  const cardF = document.getElementById("cardRecipeTotalFat");
  const cardC = document.getElementById("cardRecipeTotalCarbs");
  const ratioDesc = document.getElementById("recipeNutrientRatioDesc");

  if (cardKcal) cardKcal.innerText = `${Math.round(totalKcal)} kcal`;
  if (cardP) cardP.innerText = `${totalP.toFixed(1)} g`;
  if (cardF) cardF.innerText = `${totalF.toFixed(1)} g`;
  if (cardC) cardC.innerText = `${totalC.toFixed(1)} g`;

  const macroKcal = (totalC * 4) + (totalP * 4) + (totalF * 9);
  if (ratioDesc) {
    if (macroKcal > 0) {
      const pctC = ((totalC * 4 / macroKcal) * 100).toFixed(1);
      const pctP = ((totalP * 4 / macroKcal) * 100).toFixed(1);
      const pctF = ((totalF * 9 / macroKcal) * 100).toFixed(1);
      ratioDesc.innerText = `三大營養素佔比（${filter}）：碳水化合物 ${pctC}% ｜ 蛋白質 ${pctP}% ｜ 脂肪 ${pctF}% （熱量加總共 ${Math.round(totalKcal)} kcal）`;
    } else {
      ratioDesc.innerText = `三大營養素熱量佔比：碳水化合物 0% ｜ 蛋白質 0% ｜ 脂肪 0%`;
    }
  }

  // 同步加總食品營養成分資料庫中所有的營養素（依菜單開立的所有食材克數進行加總）
  updateAllCompositionNutrientSummary();
}

/**
 * 依據菜單開立的食材(克數)進行加總，並將菜單食材逐一詳細列於營養成分資料庫介面中
 */
function updateAllCompositionNutrientSummary() {
  const sum = {
    count: menuRecipes.length,
    kcal: 0,
    protein: 0,
    fat: 0,
    saturatedFat: 0,
    carbs: 0,
    fiber: 0,
    sugars: 0,
    water: 0,
    na: 0,
    k: 0,
    ca: 0,
    mg: 0,
    fe: 0,
    zn: 0,
    p: 0,
    cholesterol: 0
  };

  const rowsHtml = menuRecipes.map((r, idx) => {
    const nut = calculateRecipeNutrients(r.food, r.category, r.rawWeight, r.ex, r.sampleName);
    sum.kcal += (nut.kcal || 0);
    sum.protein += (nut.protein || 0);
    sum.fat += (nut.fat || 0);
    sum.saturatedFat += (nut.saturatedFat || 0);
    sum.carbs += (nut.carbs || 0);
    sum.fiber += (nut.fiber || 0);
    sum.sugars += (nut.sugars || 0);
    sum.water += (nut.water || 0);
    sum.na += (nut.na || 0);
    sum.k += (nut.k || 0);
    sum.ca += (nut.ca || 0);
    sum.mg += (nut.mg || 0);
    sum.fe += (nut.fe || 0);
    sum.zn += (nut.zn || 0);
    sum.p += (nut.p || 0);
    sum.cholesterol += (nut.cholesterol || 0);

    return `
      <tr>
        <td><span class="badge" style="background: #e0f2fe; color: #0284c7; padding: 2px 8px; font-size: 11px;">${r.meal}</span></td>
        <td><strong>${r.dish || "—"}</strong></td>
        <td><strong style="color: #0f766e;">${r.food}</strong></td>
        <td>
          <div style="display: flex; align-items: center; gap: 6px;">
            <span class="badge" style="background: ${r.sampleName ? '#ecfdf5' : '#f1f5f9'}; color: ${r.sampleName ? '#047857' : '#334155'}; border: 1px solid ${r.sampleName ? '#6ee7b7' : '#cbd5e1'}; font-size: 11px; padding: 2px 6px; white-space: nowrap;" title="${r.sampleName ? '使用者指定選用樣品' : '系統自動智慧比對'}">
              ${r.sampleName ? '🎯 ' : '📖 '}${nut.matchedName || '食物代換估算'}
            </span>
            <button type="button" class="btn btn-outline" style="padding: 1px 7px; font-size: 11px; border-radius: 4px; white-space: nowrap;" onclick="openSampleSelectorModal(${idx})" title="搜尋並選取不同的樣品">🔍 選取</button>
          </div>
        </td>
        <td style="text-align: right; font-weight: 600;">${r.rawWeight}</td>
        <td style="text-align: right;" class="val-red font-bold">${Math.round(nut.kcal)}</td>
        <td style="text-align: right;" class="val-green font-bold">${nut.protein.toFixed(1)}</td>
        <td style="text-align: right;" class="val-orange font-bold">${nut.fat.toFixed(1)}</td>
        <td style="text-align: right;">${nut.saturatedFat ? nut.saturatedFat.toFixed(1) : 0}</td>
        <td style="text-align: right;" class="val-blue font-bold">${nut.carbs.toFixed(1)}</td>
        <td style="text-align: right;">${nut.fiber ? nut.fiber.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.sugars ? nut.sugars.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.water ? nut.water.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.na ? nut.na.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.k ? nut.k.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.ca ? nut.ca.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.mg ? nut.mg.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.fe ? nut.fe.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.zn ? nut.zn.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.p ? nut.p.toFixed(1) : 0}</td>
        <td style="text-align: right;">${nut.cholesterol ? nut.cholesterol.toFixed(1) : 0}</td>
      </tr>
    `;
  }).join("");

  const menuTableBody = document.getElementById("menuCompositionItemsBody");
  if (menuTableBody) {
    menuTableBody.innerHTML = rowsHtml || '<tr><td colspan="21" style="text-align:center;color:#94a3b8;padding:16px;">目前菜單尚未開立食材</td></tr>';
  }

  const setVal = (id, val, unit = "") => {
    const el = document.getElementById(id);
    if (el) el.innerText = `${val.toFixed(1)} ${unit}`.trim();
  };

  const elStats = document.getElementById("lblCompMenuStats");
  if (elStats) elStats.innerText = `菜單目前開立共 ${sum.count} 項食材`;

  const elKcal = document.getElementById("sumCompKcal");
  if (elKcal) elKcal.innerText = `${Math.round(sum.kcal)} kcal`;
  setVal("sumCompProtein", sum.protein, "g");
  setVal("sumCompFat", sum.fat, "g");
  setVal("sumCompSatFat", sum.saturatedFat, "g");
  setVal("sumCompCarbs", sum.carbs, "g");
  setVal("sumCompFiber", sum.fiber, "g");
  setVal("sumCompSugars", sum.sugars, "g");
  setVal("sumCompWater", sum.water, "g");
  setVal("sumCompNa", sum.na, "mg");
  setVal("sumCompK", sum.k, "mg");
  setVal("sumCompCa", sum.ca, "mg");
  setVal("sumCompMg", sum.mg, "mg");
  setVal("sumCompFe", sum.fe, "mg");
  setVal("sumCompZn", sum.zn, "mg");
  setVal("sumCompP", sum.p, "mg");
  setVal("sumCompCholesterol", sum.cholesterol, "mg");

  // 評估國人膳食營養素參考攝取量 (DRIs 第九版草案) 是否符合
  renderDriCompliance(sum);
}

/**
 * 智慧比對衛福部食品成分資料庫 (2025版，共2,213項)，依據生重克數精確換算完整營養素
 */
function calculateRecipeNutrients(foodName, category, rawWeight, ex, customSampleName = null) {
  const weight = Number(rawWeight) || 0;
  const portions = Number(ex) || 1;
  const name = (foodName || "").trim();

  if (!name && weight <= 0) {
    return {
      kcal: 0, protein: 0, fat: 0, saturatedFat: 0, carbs: 0,
      fiber: 0, sugars: 0, water: 0, na: 0, k: 0, ca: 0, mg: 0,
      fe: 0, zn: 0, p: 0, cholesterol: 0, matchedName: ""
    };
  }

  // 1. 優先精準 / 模糊比對 2025版食品成分資料庫 (每 100g 含量)
  if (typeof FOOD_COMPOSITION_2025 !== "undefined" && Array.isArray(FOOD_COMPOSITION_2025) && FOOD_COMPOSITION_2025.length > 0) {
    let matched = null;
    // 1. 若使用者主動指定選取資料庫樣品名稱，最優先使用指定樣品
    if (customSampleName) {
      matched = FOOD_COMPOSITION_2025.find(it => it.Name === customSampleName);
    }

    // 2. 常見日常食材精準官方樣品對應表 (避免如豆腐比對成俗名含芙蓉豆腐的蒸蛋)
    const COMMON_FOOD_SAMPLE_DEFAULTS = {
      "豆腐": "傳統豆腐",
      "板豆腐": "傳統豆腐",
      "嫩豆腐": "嫩豆腐",
      "百頁豆腐": "百頁豆腐",
      "油豆腐": "小三角油豆腐",
      "豆皮": "豆腐皮",
      "高麗菜": "甘藍平均值",
      "香蕉": "北蕉平均值",
      "牛奶": "高脂鮮乳",
      "燕麥": "燕麥",
      "胡蘿蔔": "胡蘿蔔平均值",
      "雞胸肉": "去皮清肉(土雞)",
      "菠菜": "菠菜",
      "番茄": "牛番茄",
      "紫米": "黑秈糯糙米",
      "白飯": "白飯",
      "雞蛋": "雞蛋平均值",
      "蛋": "雞蛋平均值",
      "豬肉": "豬肉火鍋片平均值",
      "牛肉": "牛肉火鍋片平均值"
    };

    if (!matched && COMMON_FOOD_SAMPLE_DEFAULTS[name]) {
      const defaultSample = COMMON_FOOD_SAMPLE_DEFAULTS[name];
      matched = FOOD_COMPOSITION_2025.find(it => it.Name === defaultSample);
    }

    // 3. 樣品名稱完全吻合
    if (!matched) {
      matched = FOOD_COMPOSITION_2025.find(it => it.Name === name);
    }

    // 4. 樣品名稱開頭吻合或包含 (優先找樣品名本身符合者)
    if (!matched) {
      matched = FOOD_COMPOSITION_2025.find(it => it.Name.startsWith(name) || it.Name.includes(name));
    }

    // 5. 俗名完全吻合單詞 (排除蛋類等非本質之俗名干擾)
    if (!matched) {
      matched = FOOD_COMPOSITION_2025.find(it => it.CommonName && it.CommonName.split(",").some(c => c.trim() === name));
    }

    // 6. 俗名包含或反向包含
    if (!matched) {
      matched = FOOD_COMPOSITION_2025.find(it => it.CommonName && it.CommonName.includes(name));
    }
    if (!matched) {
      matched = FOOD_COMPOSITION_2025.find(it => name.includes(it.Name));
    }

    if (matched) {
      const factor = (weight > 0 ? weight : 100) / 100.0;
      return {
        kcal: (matched.Calories || 0) * factor,
        protein: (matched.Protein || 0) * factor,
        fat: (matched.Fat || 0) * factor,
        saturatedFat: (matched.SaturatedFat || 0) * factor,
        carbs: (matched.Carbs || 0) * factor,
        fiber: (matched.DietaryFiber || 0) * factor,
        sugars: (matched.Sugars || 0) * factor,
        water: (matched.Water || 0) * factor,
        na: (matched.Sodium || 0) * factor,
        k: (matched.Potassium || 0) * factor,
        ca: (matched.Calcium || 0) * factor,
        mg: (matched.Magnesium || 0) * factor,
        fe: (matched.Iron || 0) * factor,
        zn: (matched.Zinc || 0) * factor,
        p: (matched.Phosphorus || 0) * factor,
        cholesterol: (matched.Cholesterol || 0) * factor,
        matchedName: matched.Name
      };
    }
  }

  // 2. 次要比對：衛福部 261 項食物代換表
  if (typeof FOOD_EXCHANGE_DB !== "undefined" && Array.isArray(FOOD_EXCHANGE_DB)) {
    let matchedEx = FOOD_EXCHANGE_DB.find(it => it.foodName === name || name.includes(it.foodName) || it.foodName.includes(name));
    if (matchedEx) {
      const factor = (weight > 0 && matchedEx.rawWeight > 0) 
        ? (weight / matchedEx.rawWeight) 
        : portions;
      const f = (matchedEx.fat || 0) * factor;
      return {
        kcal: (matchedEx.calories || 0) * factor,
        protein: (matchedEx.protein || 0) * factor,
        fat: f,
        saturatedFat: Number((f * 0.3).toFixed(1)),
        carbs: (matchedEx.carbs || 0) * factor,
        fiber: Number(((category && category.includes("蔬菜") ? 2 : (category && category.includes("水果") ? 2 : 1)) * portions).toFixed(1)),
        sugars: 0,
        water: Number((weight > 0 ? weight * 0.8 : 80).toFixed(1)),
        na: 0, k: 0, ca: 0, mg: 0, fe: 0, zn: 0, p: 0, cholesterol: 0,
        matchedName: `${matchedEx.foodName} (食物代換)`
      };
    }
  }

  // 3. 通用標準代換大類估算 (保底機制)
  const catDefaults = {
    "全脂奶": { kcal: 150, p: 8, f: 8, c: 12 },
    "低脂奶": { kcal: 120, p: 8, f: 4, c: 12 },
    "脫脂奶": { kcal: 80, p: 8, f: 0, c: 12 },
    "米食類": { kcal: 70, p: 2, f: 0, c: 15 },
    "麵食類": { kcal: 70, p: 2, f: 0, c: 15 },
    "全穀雜糧": { kcal: 70, p: 2, f: 0, c: 15 },
    "穀物類": { kcal: 70, p: 2, f: 0, c: 15 },
    "豆魚蛋肉(低脂)": { kcal: 55, p: 7, f: 3, c: 0 },
    "豆魚蛋肉(中脂)": { kcal: 75, p: 7, f: 5, c: 0 },
    "豆魚蛋肉(高脂)": { kcal: 120, p: 7, f: 10, c: 0 },
    "豆魚蛋肉": { kcal: 75, p: 7, f: 5, c: 0 },
    "蔬菜類": { kcal: 25, p: 1, f: 0, c: 5 },
    "水果類": { kcal: 60, p: 0, f: 0, c: 15 },
    "油脂類": { kcal: 45, p: 0, f: 5, c: 0 },
    "堅果種子類": { kcal: 45, p: 0, f: 5, c: 0 }
  };

  const def = catDefaults[category] || { kcal: 50, p: 2, f: 1, c: 8 };
  return {
    kcal: def.kcal * portions,
    protein: def.p * portions,
    fat: def.f * portions,
    saturatedFat: Number(((def.f || 0) * 0.3 * portions).toFixed(1)),
    carbs: def.c * portions,
    fiber: Number(((category && category.includes("蔬菜") ? 2 : (category && category.includes("水果") ? 2 : 1)) * portions).toFixed(1)),
    sugars: 0,
    water: Number((weight > 0 ? weight * 0.8 : 80).toFixed(1)),
    na: 0, k: 0, ca: 0, mg: 0, fe: 0, zn: 0, p: 0, cholesterol: 0,
    matchedName: ""
  };
}


/**
 * 局部更新菜單即時總結卡片與四大營養素佔比（不重繪整張輸入表，輸入極致流暢）
 */
function updateMenuSummaryCards() {
  const filter = document.getElementById("cboMealFilter").value;
  const filtered = filter === "全部" 
    ? menuRecipes 
    : menuRecipes.filter(r => r.meal === filter);

  let totalEx = 0;
  let totalWeight = 0;
  let totalKcal = 0;
  let totalP = 0;
  let totalF = 0;
  let totalC = 0;
  const colorCountMap = {};

  filtered.forEach(r => {
    const nut = calculateRecipeNutrients(r.food, r.category, r.rawWeight, r.ex, r.sampleName);
    r._calc = nut;
    totalEx += Number(r.ex) || 0;
    totalWeight += Number(r.rawWeight) || 0;
    totalKcal += nut.kcal || 0;
    totalP += nut.protein || 0;
    totalF += nut.fat || 0;
    totalC += nut.carbs || 0;
    if (r.color) colorCountMap[r.color] = (colorCountMap[r.color] || 0) + 1;
  });

  const cardKcal = document.getElementById("cardRecipeTotalKcal");
  const cardP = document.getElementById("cardRecipeTotalProtein");
  const cardF = document.getElementById("cardRecipeTotalFat");
  const cardC = document.getElementById("cardRecipeTotalCarbs");
  const ratioDesc = document.getElementById("recipeNutrientRatioDesc");

  if (cardKcal) cardKcal.innerText = `${Math.round(totalKcal)} kcal`;
  if (cardP) cardP.innerText = `${totalP.toFixed(1)} g`;
  if (cardF) cardF.innerText = `${totalF.toFixed(1)} g`;
  if (cardC) cardC.innerText = `${totalC.toFixed(1)} g`;

  if (ratioDesc) {
    const macroKcal = (totalC * 4) + (totalP * 4) + (totalF * 9);
    if (macroKcal > 0) {
      const pctC = ((totalC * 4 / macroKcal) * 100).toFixed(1);
      const pctP = ((totalP * 4 / macroKcal) * 100).toFixed(1);
      const pctF = ((totalF * 9 / macroKcal) * 100).toFixed(1);
      ratioDesc.innerText = `三大營養素佔比（${filter}）：碳水化合物 ${pctC}% ｜ 蛋白質 ${pctP}% ｜ 脂肪 ${pctF}% （熱量加總共 ${Math.round(totalKcal)} kcal）`;
    } else {
      ratioDesc.innerText = `三大營養素熱量佔比：碳水化合物 0% ｜ 蛋白質 0% ｜ 脂肪 0%`;
    }
  }
}

function updateRecipeMeal(index, val) {
  if (menuRecipes[index]) {
    menuRecipes[index].meal = val;
    const curFilter = document.getElementById("cboMealFilter").value;
    if (curFilter !== "全部" && curFilter !== val) {
      renderMenuRecipes();
    }
  }
}

function updateDishName(index, val) {
  if (menuRecipes[index]) menuRecipes[index].dish = val;
}

function updateFoodName(index, val) {
  if (menuRecipes[index]) {
    menuRecipes[index].food = val;
    menuRecipes[index].color = detectFoodColor(val, menuRecipes[index].category);
    // 更新即時加總與營養成分列表，但不觸發重新渲染整張表格以免輸入法中斷與失去游標焦點
    updateMenuSummaryCards();
    updateAllCompositionNutrientSummary();
  }
}

function updateFoodNameAndRender(index, val) {
  if (menuRecipes[index]) {
    menuRecipes[index].food = val;
    applyFoodExchangeMatch(menuRecipes[index]);
    renderMenuRecipes();
  }
}

function updateRecipeEx(index, val) {
  const num = parseFloat(val);
  if (menuRecipes[index] && Number.isFinite(num) && num > 0) {
    menuRecipes[index].ex = num;
    const matchedEx = findFoodExchangeMatch(menuRecipes[index].food);
    if (matchedEx) {
      menuRecipes[index].rawWeight = Number((matchedEx.rawWeight * num).toFixed(1));
    } else {
      menuRecipes[index].rawWeight = "";
    }
    updateMenuSummaryCards();
    updateAllCompositionNutrientSummary();
    const row = document.querySelector(`#menuRecipeTableBody tr[data-recipe-index="${index}"]`);
    if (row) {
      const weightInput = row.querySelector('td:nth-child(6) input');
      if (weightInput) weightInput.value = menuRecipes[index].rawWeight;
    }
    if (row && menuRecipes[index]._calc) {
      const kcalCell = row.querySelector(".rec-kcal");
      if (kcalCell) kcalCell.innerText = Math.round(menuRecipes[index]._calc.kcal);
    }
  }
}

function updateRecipeRawWeight(index, val) {
  const num = parseFloat(val);
  if (menuRecipes[index] && Number.isFinite(num) && num > 0) {
    menuRecipes[index].rawWeight = num;
    renderMenuRecipes();
  }
}

function findFoodExchangeMatch(foodName) {
  const name = (foodName || "").trim().toLowerCase();
  if (!name || typeof FOOD_EXCHANGE_DB === "undefined" || !Array.isArray(FOOD_EXCHANGE_DB)) {
    return null;
  }

  return FOOD_EXCHANGE_DB.find(item => {
    const itemName = (item.foodName || "").trim().toLowerCase();
    const aliases = itemName.split(/[、,，/]/).map(alias => alias.trim()).filter(Boolean);
    return itemName === name ||
      aliases.includes(name) ||
      name.includes(itemName) ||
      itemName.includes(name) ||
      aliases.some(alias => name.includes(alias) || alias.includes(name));
  }) || null;
}

function applyFoodExchangeMatch(recipe) {
  const matchedEx = findFoodExchangeMatch(recipe.food);
  recipe.color = detectFoodColor(recipe.food, matchedEx ? matchedEx.category : recipe.category);

  if (!matchedEx) {
    recipe.category = "自訂食材";
    recipe.rawWeight = "";
    return;
  }

  recipe.category = matchedEx.category;
  const portions = Number(recipe.ex);
  if (Number.isFinite(portions) && portions > 0 && Number(matchedEx.rawWeight) > 0) {
    recipe.rawWeight = Number((matchedEx.rawWeight * portions).toFixed(1));
  }
}

function detectFoodColor(foodName, category = "") {
  let color = "白色";
  if (!foodName) return color;
  const name = foodName.trim().toLowerCase();

  // 0. 特殊蔬菜專用
  if (name.includes("小白菜")) return "嫩綠色";
  if (name.includes("高麗菜") || name.includes("甘藍") || name.includes("包心菜")) return "粉綠/嫩綠";

  // 1. 橙色 (Orange)
  if (name.includes("胡蘿蔔") || name.includes("紅蘿蔔") || name.includes("南瓜") || 
      name.includes("木瓜") || name.includes("柳丁") || name.includes("橘子") || 
      name.includes("柑橘") || name.includes("柿子") || name.includes("哈密瓜") || 
      name.includes("鮭魚") || name.includes("地瓜") || name.includes("黃肉地瓜") ||
      name.includes("金針花") || name.includes("蛋黃") || name.includes("橙")) {
    return "橙色";
  }

  // 2. 黃色 (Yellow)
  if (name.includes("香蕉") || name.includes("玉米") || name.includes("蛋") || 
      name.includes("起司") || name.includes("黃豆") || name.includes("鳳梨") || 
      name.includes("檸檬") || name.includes("黃椒") || name.includes("黃甜椒") || 
      name.includes("芒果") || name.includes("小米") || name.includes("黃")) {
    return "黃色";
  }

  // 3. 紅色 (Red)
  if (name.includes("番茄") || name.includes("大番茄") || name.includes("小番茄") || 
      name.includes("紅椒") || name.includes("甜椒") || name.includes("紅甜椒") || 
      name.includes("辣椒") || name.includes("草莓") || name.includes("西瓜") || 
      name.includes("蘋果") || name.includes("牛肉") || name.includes("豬肉") || 
      name.includes("羊肉") || name.includes("紅豆") || name.includes("紅麴") || 
      name.includes("洛神花") || name.includes("蔓越莓") || name.includes("紅棗") || 
      name.includes("火龍果") || name.includes("紅")) {
    return "紅色";
  }

  // 4. 綠色 (Green)
  if (name.includes("青菜") || name.includes("蔬菜") || name.includes("時蔬") ||
      name.includes("菠菜") || name.includes("青江菜") || name.includes("空心菜") || 
      name.includes("花椰菜") || name.includes("青花菜") || name.includes("小黃瓜") || 
      name.includes("芹菜") || name.includes("萵苣") || name.includes("地瓜葉") || 
      name.includes("青椒") || name.includes("毛豆") || name.includes("綠豆") || 
      name.includes("奇異果") || name.includes("芭樂") || name.includes("蔥") || 
      name.includes("蘆筍") || name.includes("絲瓜") || name.includes("苦瓜") ||
      name.includes("秋葵") || name.includes("酪梨") || name.includes("綠") || 
      name.includes("菜") || name.includes("葉") || name.includes("苗")) {
    return "綠色";
  }

  // 5. 紫色/黑/藍 (Purple / Black)
  if (name.includes("茄子") || name.includes("紫甘藍") || name.includes("葡萄") || 
      name.includes("藍莓") || name.includes("紫米") || name.includes("黑米") || 
      name.includes("黑豆") || name.includes("黑木耳") || name.includes("木耳") || 
      name.includes("紫菜") || name.includes("海苔") || name.includes("海帶") || 
      name.includes("黑芝麻") || name.includes("香菇") || name.includes("黑") || name.includes("紫")) {
    return "紫色/黑色";
  }

  // 6. 棕褐色 (Brown)
  if (name.includes("堅果") || name.includes("核桃") || name.includes("杏仁") || 
      name.includes("腰果") || name.includes("花生") || name.includes("全麥") || 
      name.includes("糙米") || name.includes("蕎麥") || name.includes("可可") || 
      name.includes("茶葉蛋")) {
    return "棕褐色";
  }

  // 7. 白色 (White)
  if (name.includes("米") || name.includes("飯") || name.includes("麵") || 
      name.includes("豆腐") || name.includes("豆漿") || name.includes("牛奶") || 
      name.includes("白蘿蔔") || name.includes("洋蔥") || name.includes("大蒜") || 
      name.includes("蒜") || name.includes("山藥") || name.includes("白花椰") || 
      name.includes("冬瓜") || name.includes("竹筍") || name.includes("筊白筍") || 
      name.includes("雞肉") || name.includes("雞胸") || name.includes("魚") || 
      name.includes("蛋白") || name.includes("燕麥") || name.includes("水梨") || 
      name.includes("馬鈴薯") || name.includes("白")) {
    return "白色";
  }

  if (category && category.includes("蔬菜")) return "綠色";
  return "白色";
}

function deleteRecipeItem(index) {
  menuRecipes.splice(index, 1);
  renderMenuRecipes();
}

function addCustomRecipeItem() {
  const currentMeal = document.getElementById("cboMealFilter").value;
  const meal = currentMeal === "全部" ? "早餐" : currentMeal;

  // 直接新增一列可自由輸入菜名與食材名稱的空白列
  const newItem = {
    meal: meal,
    dish: "",
    food: "新食材",
    category: "自訂食材",
    ex: 1,
    rawWeight: "",
    color: "白色"
  };

  menuRecipes.unshift(newItem);
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
  const btn = document.getElementById("btnOpenFormulaWindow");
  if (btn) {
    btn.addEventListener("click", () => {
      openModal("modalFormulas");
    });
  }

  // 渲染獨立頁面配方庫與彈窗
  renderFormulasTable(COMMERCIAL_FORMULAS_DB);
}

function renderFormulasTable(list) {
  if (!Array.isArray(list)) return;

  const rowHtml = list.map(f => `
    <tr>
      <td><strong style="color: #4338ca; font-size: 14px;">${f.name}</strong></td>
      <td><span class="badge" style="background:#e0e7ff; color:#3730a3; font-weight:600;">${f.spec}</span></td>
      <td class="val-green" style="font-weight:700;">${f.p}</td>
      <td class="val-orange" style="font-weight:700;">${f.f}</td>
      <td class="val-red" style="font-weight:700;">${f.c}</td>
      <td class="val-blue" style="font-weight:700; font-size: 13px;">${f.kcal}</td>
      <td class="text-muted" style="font-size: 12px;">${f.note}</td>
    </tr>
  `).join("");

  // 渲染第 5 頁主畫面表格
  const pageTbody = document.getElementById("pageFormulaTableBody");
  if (pageTbody) {
    pageTbody.innerHTML = rowHtml || `<tr><td colspan="7" class="text-center text-muted" style="padding: 24px;">查無符合的商業配方</td></tr>`;
  }

  // 同步彈窗表格 (若彈窗仍被調用)
  const modalTbody = document.getElementById("formulaTableBody");
  if (modalTbody) {
    modalTbody.innerHTML = rowHtml;
  }
}

function filterFormulasPage() {
  if (typeof COMMERCIAL_FORMULAS_DB === "undefined") return;

  const keyword = (document.getElementById("inFormulaSearch")?.value || "").trim().toLowerCase();
  const cat = document.getElementById("cboFormulaCategory")?.value || "全部";

  const filtered = COMMERCIAL_FORMULAS_DB.filter(item => {
    let matchCat = true;
    if (cat === "均衡營養") {
      matchCat = item.note.includes("均衡") || item.note.includes("等滲透") || item.note.includes("纖維") || item.name.includes("安素") || item.name.includes("愛速康") || item.name.includes("健力體") || item.name.includes("愛美力") || item.name.includes("完膳");
    } else if (cat === "糖尿病") {
      matchCat = item.note.includes("糖尿病") || item.note.includes("穩醣") || item.note.includes("低GI") || item.name.includes("糖尿") || item.name.includes("葡勝納");
    } else if (cat === "腎臟病") {
      matchCat = item.note.includes("腎") || item.name.includes("腎") || item.name.includes("LPF");
    } else if (cat === "腫瘤重症") {
      matchCat = item.note.includes("腫瘤") || item.note.includes("癌症") || item.note.includes("重症") || item.note.includes("高蛋白") || item.name.includes("倍力素") || item.name.includes("諾沛") || item.name.includes("雙卡") || item.name.includes("創快復");
    } else if (cat === "其他特殊") {
      matchCat = item.note.includes("低氮") || item.name.includes("低氮") || item.note.includes("分離乳清") || item.note.includes("胜肽");
    }

    const matchText = !keyword ||
      item.name.toLowerCase().includes(keyword) ||
      item.spec.toLowerCase().includes(keyword) ||
      item.note.toLowerCase().includes(keyword);

    return matchCat && matchText;
  });

  renderFormulasTable(filtered);
}

function clearFormulaSearch() {
  const inSearch = document.getElementById("inFormulaSearch");
  const cboCat = document.getElementById("cboFormulaCategory");
  if (inSearch) inSearch.value = "";
  if (cboCat) cboCat.value = "全部";
  filterFormulasPage();
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


// ==========================================
// 7. 台灣食品營養成分資料庫 (2025版，共2,213項)
// ==========================================
function initFoodComposition() {
  const inSearch = document.getElementById("inCompositionSearch");
  const cboCat = document.getElementById("cboCompositionCategory");
  if (!inSearch || !cboCat) return;

  inSearch.addEventListener("input", filterFoodComposition);
  cboCat.addEventListener("change", filterFoodComposition);

  filterFoodComposition();
}

function filterFoodComposition() {
  if (typeof FOOD_COMPOSITION_2025 === "undefined") return;

  const keyword = (document.getElementById("inCompositionSearch").value || "").trim().toLowerCase();
  const cat = document.getElementById("cboCompositionCategory").value;

  const filtered = FOOD_COMPOSITION_2025.filter(item => {
    const matchCat = cat === "全部" || item.Category === cat;
    const matchText = !keyword || 
      (item.Name && item.Name.toLowerCase().includes(keyword)) ||
      (item.CommonName && item.CommonName.toLowerCase().includes(keyword)) ||
      (item.Code && item.Code.toLowerCase().includes(keyword));
    return matchCat && matchText;
  });

  const countEl = document.getElementById("lblCompositionCount");
  if (countEl) countEl.innerText = filtered.length.toLocaleString();

  const tbody = document.getElementById("compositionTableBody");
  if (!tbody) return;

  // 渲染前 200 筆以保證極致流暢度
  const displayItems = filtered.slice(0, 200);

  tbody.innerHTML = displayItems.map(item => `
    <tr>
      <td><code>${item.Code}</code></td>
      <td class="text-teal font-bold">${item.Category}</td>
      <td><strong>${item.Name}</strong></td>
      <td class="text-muted">${item.CommonName || "—"}</td>
      <td class="val-red font-bold">${item.Calories ? item.Calories.toFixed(1) : 0}</td>
      <td class="val-green font-bold">${item.Protein ? item.Protein.toFixed(1) : 0}</td>
      <td class="val-orange font-bold">${item.Fat ? item.Fat.toFixed(1) : 0}</td>
      <td>${item.SaturatedFat ? item.SaturatedFat.toFixed(1) : 0}</td>
      <td class="font-blue font-bold">${item.Carbs ? item.Carbs.toFixed(1) : 0}</td>
      <td>${item.DietaryFiber ? item.DietaryFiber.toFixed(1) : 0}</td>
      <td>${item.Sugars ? item.Sugars.toFixed(1) : 0}</td>
      <td>${item.Water ? item.Water.toFixed(1) : 0}</td>
      <td>
        <button class="btn btn-outline" onclick="addCompositionToMenu('${item.Name.replace(/'/g, "\\'")}', '${item.Category}')">加菜單</button>
      </td>
    </tr>
  `).join("");
}

function clearCompositionSearch() {
  const inSearch = document.getElementById("inCompositionSearch");
  const cboCat = document.getElementById("cboCompositionCategory");
  if (inSearch) inSearch.value = "";
  if (cboCat) cboCat.value = "全部";
  filterFoodComposition();
}

function addCompositionToMenu(name, category) {
  let meal = document.getElementById("cboMealFilter").value;
  if (meal === "全部") meal = "午餐";

  menuRecipes.push({
    meal: meal,
    dish: "",
    food: name,
    category: category,
    ex: 1,
    rawWeight: "",
    color: "白色"
  });

  renderMenuRecipes();
  alert(`已將「${name}」加入【${meal}】菜單！`);
}

// ==========================================
// 8. 列印衛教處方單張功能 (Print Report)
// ==========================================
function printPrescriptionReport() {
  const name = document.getElementById("inName").value || "個案";
  const chartNo = document.getElementById("inChartNo").value || "—";
  const gender = document.getElementById("inGender").value;
  const age = document.getElementById("inAge").value;
  const height = document.getElementById("inHeight").value;
  const weight = document.getElementById("inWeight").value;
  const bmi = document.getElementById("valBmi").innerText;
  const ibw = document.getElementById("valIbw").innerText;
  const targetKcal = document.getElementById("inTargetKcal").value;
  const carbGrams = document.getElementById("lblTargetCarbGrams").innerText;
  const fatGrams = document.getElementById("lblTargetFatGrams").innerText;
  const protGrams = document.getElementById("lblTargetProteinGrams").innerText;
  const egfr = document.getElementById("valEgfr").innerText;

  // 取得各餐菜單
  const mealGroups = {};
  menuRecipes.forEach(r => {
    if (!mealGroups[r.meal]) mealGroups[r.meal] = [];
    mealGroups[r.meal].push(`${r.dish ? r.dish + '-' : ''}${r.food} (${r.ex}份)`);
  });

  let menuHtml = "";
  for (let m in mealGroups) {
    menuHtml += `<tr><td style="font-weight:bold;width:90px;background:#f8fafc;padding:8px;border:1px solid #cbd5e1;">${m}</td><td style="padding:8px;border:1px solid #cbd5e1;">${mealGroups[m].join("、 ")}</td></tr>`;
  }

  const printWindow = window.open("", "_blank");
  printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8">
      <title>臨床營養諮詢衛教處方單 - ${name}</title>
      <style>
        body { font-family: -apple-system, "Microsoft JhengHei", sans-serif; padding: 24px; color: #1e293b; line-height: 1.6; }
        .header { text-align: center; border-bottom: 2px solid #0d9488; padding-bottom: 12px; margin-bottom: 16px; }
        .header h1 { margin: 0; color: #0f766e; font-size: 24px; }
        .section-title { font-size: 16px; font-weight: bold; color: #0d9488; margin: 16px 0 8px; border-left: 4px solid #0d9488; padding-left: 8px; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 12px; }
        th, td { border: 1px solid #cbd5e1; padding: 8px 12px; text-align: left; font-size: 14px; }
        th { background: #f1f5f9; }
        .highlight { font-weight: bold; color: #0f766e; }
        .footer { margin-top: 30px; text-align: right; font-size: 13px; color: #64748b; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🏥 飲食營養諮詢評估與菜單處方單</h1>
        <p>專業臨床營養照護記錄 ｜ 衛教指導單張</p>
      </div>

      <div class="section-title">一、個案基本體位與生理評估</div>
      <table>
        <tr>
          <th>姓名 / 暱稱</th><td>${name}</td>
          <th>病歷號</th><td>${chartNo}</td>
          <th>性別 / 年齡</th><td>${gender} / ${age} 歲</td>
        </tr>
        <tr>
          <th>身高</th><td>${height} cm</td>
          <th>體重</th><td>${weight} kg</td>
          <th>BMI 指數</th><td class="highlight">${bmi}</td>
        </tr>
        <tr>
          <th>理想體重 (IBW)</th><td>${ibw}</td>
          <th>腎功能 (eGFR)</th><td>${egfr} ml/min</td>
          <th>目標規劃熱量</th><td class="highlight" style="color:#e11d48;font-size:16px;">${targetKcal} kcal</td>
        </tr>
      </table>

      <div class="section-title">二、三大營養素目標熱量與克數分配</div>
      <table>
        <tr>
          <th>醣類 (碳水化合物)</th>
          <th>脂質 (Lipids)</th>
          <th>蛋白質 (Protein)</th>
        </tr>
        <tr>
          <td style="color:#e11d48;font-weight:bold;font-size:16px;">${carbGrams}</td>
          <td style="color:#d97706;font-weight:bold;font-size:16px;">${fatGrams}</td>
          <td style="color:#16a34a;font-weight:bold;font-size:16px;">${protGrams}</td>
        </tr>
      </table>

      <div class="section-title">三、客製化飲食菜單規劃</div>
      <table>
        ${menuHtml || '<tr><td colspan="2" style="text-align:center;color:#94a3b8;">尚未開立菜單食材</td></tr>'}
      </table>

      <div class="section-title">四、營養師專業指導與衛教叮嚀</div>
      <div style="border:1px solid #cbd5e1;padding:12px;border-radius:6px;background:#f8fafc;font-size:14px;">
        1. 每日維持規律三餐與點心，補充足量水分（建議每日至少 2000 ml）。<br>
        2. 烹調宜清淡，以蒸、煮、滷、涼拌為原則，避免油炸及高鈉加工食品。<br>
        3. 如有任何身體不適或體重急遽變化，請立即向營養諮詢門診洽詢。
      </div>

      <div class="footer">
        評估日期：${new Date().toLocaleDateString()} ｜ 營養師簽章：____________________
      </div>
      <script>
        window.onload = function() { window.print(); };
      </script>
    </body>
    </html>
  `);
  printWindow.document.close();
}

/**
 * 國人膳食營養素參考攝取量 (DRIs) 第九版草案 比對與達成率分析
 */
let currentSelectedDriId = "19_30y_m";

function onDriAgeGroupChange(newId) {
  currentSelectedDriId = newId;
  updateAllCompositionNutrientSummary();
}

function renderDriCompliance(sum) {
  if (typeof DRIS_NINTH_EDITION === "undefined" || !Array.isArray(DRIS_NINTH_EDITION)) return;

  const select = document.getElementById("cboDriAgeGroup");
  if (select && select.children.length === 0) {
    select.innerHTML = DRIS_NINTH_EDITION.map(d => `<option value="${d.id}" ${d.id === currentSelectedDriId ? "selected" : ""}>${d.label}</option>`).join("");
  }
  if (select) {
    currentSelectedDriId = select.value || currentSelectedDriId;
  }

  const dri = DRIS_NINTH_EDITION.find(d => d.id === currentSelectedDriId) || DRIS_NINTH_EDITION[12]; // 預設 19-30 歲男
  const container = document.getElementById("driComplianceContainer");
  if (!container) return;

  // 核心指標檢核清單
  const items = [
    { name: "總熱量", val: Math.round(sum.kcal), target: dri.kcal.min, targetMax: dri.kcal.max, unit: "kcal", note: dri.kcal.note },
    { name: "蛋白質", val: sum.protein, target: dri.protein.val, unit: dri.protein.unit, type: dri.protein.type, max: dri.protein.max },
    { name: "總碳水", val: sum.carbs, target: dri.carbs.val, unit: dri.carbs.unit, note: `${dri.carbs.minPct || 50}-${dri.carbs.maxPct || 65}%` },
    { name: "膳食纖維", val: sum.fiber, target: dri.fiber.val, unit: dri.fiber.unit, type: dri.fiber.type },
    { name: "鈉 (Sodium)", val: sum.na, max: dri.na.max || 2300, unit: "mg", isLimit: true, type: dri.na.type },
    { name: "鉀 (Potassium)", val: sum.k, target: dri.k.val, unit: "mg", type: dri.k.type },
    { name: "鈣 (Calcium)", val: sum.ca, target: dri.ca.val, unit: "mg", type: dri.ca.type, max: dri.ca.max },
    { name: "鎂 (Magnesium)", val: sum.mg, target: dri.mg.val, unit: "mg", type: dri.mg.type, max: dri.mg.max },
    { name: "鐵 (Iron)", val: sum.fe, target: dri.fe.val, unit: "mg", type: dri.fe.type, max: dri.fe.max },
    { name: "鋅 (Zinc)", val: sum.zn, target: dri.zn.val, unit: "mg", type: dri.zn.type, max: dri.zn.max },
    { name: "磷 (Phosphorus)", val: sum.p, target: dri.p.val, unit: "mg", type: dri.p.type, max: dri.p.max }
  ];

  const cardsHtml = items.map(it => {
    const val = Number(it.val) || 0;
    let statusBadge = "";
    let rateText = "";
    let barColor = "#0d9488";
    let pct = 0;

    if (it.isLimit) {
      // 限制型營養素（如鈉）
      pct = Math.round((val / it.max) * 100);
      if (val === 0) {
        statusBadge = '<span class="badge" style="background:#f1f5f9;color:#64748b;">未計算</span>';
      } else if (val <= it.max) {
        statusBadge = '<span class="badge" style="background:#dcfce7;color:#166534;">✔ 適量安全</span>';
        barColor = "#16a34a";
      } else {
        statusBadge = '<span class="badge" style="background:#fee2e2;color:#991b1b;">⚠️ 超標注意</span>';
        barColor = "#dc2626";
      }
      rateText = `上限 ${it.max} ${it.unit} (已達 ${pct}%)`;
    } else if (it.targetMax && it.target) {
      // 區間型（如熱量）
      pct = Math.round((val / it.target) * 100);
      if (val === 0) {
        statusBadge = '<span class="badge" style="background:#f1f5f9;color:#64748b;">未計算</span>';
      } else if (val >= it.target && val <= it.targetMax) {
        statusBadge = '<span class="badge" style="background:#dcfce7;color:#166534;">✔ 符合建議</span>';
        barColor = "#16a34a";
      } else if (val < it.target) {
        statusBadge = '<span class="badge" style="background:#fef3c7;color:#92400e;">⚡ 攝取不足</span>';
        barColor = "#f59e0b";
      } else {
        statusBadge = '<span class="badge" style="background:#e0f2fe;color:#0369a1;">充足高於目標</span>';
        barColor = "#0284c7";
      }
      rateText = `目標 ${it.target}-${it.targetMax} ${it.unit} (${pct}%)`;
    } else if (it.target) {
      // 一般建議量 (RDA/AI)
      pct = Math.round((val / it.target) * 100);
      if (val === 0) {
        statusBadge = '<span class="badge" style="background:#f1f5f9;color:#64748b;">未開立</span>';
      } else if (pct >= 100) {
        statusBadge = '<span class="badge" style="background:#dcfce7;color:#166534;">✔ 達成標準</span>';
        barColor = "#16a34a";
      } else if (pct >= 70) {
        statusBadge = '<span class="badge" style="background:#fef3c7;color:#92400e;">接近標準</span>';
        barColor = "#f59e0b";
      } else {
        statusBadge = '<span class="badge" style="background:#fee2e2;color:#991b1b;">需補充</span>';
        barColor = "#ef4444";
      }
      rateText = `建議 ${it.target} ${it.unit} (${pct}%)`;
    }

    const clampedPct = Math.min(Math.max(pct, 0), 100);

    return `
      <div style="background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 10px 12px; display: flex; flex-direction: column; justify-content: space-between; gap: 6px;">
        <div style="display: flex; justify-content: space-between; align-items: center;">
          <strong style="color: #1e293b; font-size: 13px;">${it.name}</strong>
          ${statusBadge}
        </div>
        <div style="display: flex; justify-content: space-between; align-items: baseline;">
          <span style="font-size: 16px; font-weight: 800; color: #0f172a;">${typeof val === 'number' && val % 1 !== 0 ? val.toFixed(1) : val} <small style="font-size: 11px; font-weight: 500; color: #64748b;">${it.unit}</small></span>
          <span style="font-size: 11px; color: #64748b;">${rateText}</span>
        </div>
        <div style="width: 100%; background: #e2e8f0; height: 6px; border-radius: 3px; overflow: hidden;">
          <div style="width: ${clampedPct}%; background: ${barColor}; height: 100%; border-radius: 3px; transition: width 0.3s ease;"></div>
        </div>
      </div>
    `;
  }).join("");

  container.innerHTML = cardsHtml;
}

/**
 * 在指定行下方快速插入新的一列（繼承當前餐次與菜名，方便連續新增食材或菜式）
 */
function insertRecipeItemAfter(index) {
  const current = menuRecipes[index];
  const meal = current ? current.meal : (document.getElementById("cboMealFilter").value === "全部" ? "早餐" : document.getElementById("cboMealFilter").value);
  const dish = current ? current.dish : "";

  const newItem = {
    meal: meal,
    dish: dish,
    food: "新食材",
    category: "自訂食材",
    ex: 1,
    rawWeight: "",
    color: "白色",
    sampleName: null
  };

  menuRecipes.splice(index + 1, 0, newItem);
  renderMenuRecipes();
}

/**
 * ==========================================
 * 食品成分資料庫樣品選取彈窗 (Sample Selector)
 * ==========================================
 */
let targetRecipeItemIndex = null;

function openSampleSelectorModal(recipeIndex) {
  targetRecipeItemIndex = recipeIndex;
  const item = menuRecipes[recipeIndex];
  if (!item) return;

  const badgeEl = document.getElementById("lblTargetMealBadge");
  const titleEl = document.getElementById("lblTargetFoodTitle");
  if (badgeEl) badgeEl.innerText = item.meal || "餐次";
  if (titleEl) titleEl.innerText = `${item.dish ? item.dish + ' — ' : ''}${item.food || '未命名食材'}`;

  const searchInput = document.getElementById("inSampleSearch");
  const catSelect = document.getElementById("cboSampleCategory");

  // 預設搜尋關鍵字為食材名稱（若包含多餘烹調字詞則精簡）
  if (searchInput) {
    searchInput.value = item.food ? item.food.replace(/炒|湯|飯|麵|清蒸|紅燒|烤|煎/g, "").trim() : "";
  }
  if (catSelect) {
    catSelect.value = "全部";
  }

  openModal("modalSampleSelector");
  filterSampleSelectorList();
}

function filterSampleSelectorList() {
  if (typeof FOOD_COMPOSITION_2025 === "undefined" || !Array.isArray(FOOD_COMPOSITION_2025)) return;

  const keyword = (document.getElementById("inSampleSearch").value || "").trim().toLowerCase();
  const cat = document.getElementById("cboSampleCategory").value;

  const filtered = FOOD_COMPOSITION_2025.filter(item => {
    const matchCat = cat === "全部" || item.Category === cat;
    const matchText = !keyword || 
      (item.Name && item.Name.toLowerCase().includes(keyword)) ||
      (item.CommonName && item.CommonName.toLowerCase().includes(keyword)) ||
      (item.Code && item.Code.toLowerCase().includes(keyword));
    return matchCat && matchText;
  });

  const countEl = document.getElementById("lblSampleCount");
  if (countEl) countEl.innerText = filtered.length.toLocaleString();

  const tbody = document.getElementById("sampleSelectorTableBody");
  if (!tbody) return;

  const currentItem = targetRecipeItemIndex !== null ? menuRecipes[targetRecipeItemIndex] : null;
  const currentSelectedSample = currentItem ? currentItem.sampleName : null;

  // 呈現前 150 筆以保證極致流暢度
  const displayItems = filtered.slice(0, 150);

  if (displayItems.length === 0) {
    tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; color: #94a3b8; padding: 24px;">查無符合的樣品，請更換關鍵字或類別</td></tr>';
    return;
  }

  tbody.innerHTML = displayItems.map(item => {
    const isSelected = currentSelectedSample === item.Name;
    return `
      <tr style="${isSelected ? 'background: #f0fdf4;' : ''}">
        <td><code style="font-size: 11px;">${item.Code}</code></td>
        <td><span class="text-teal font-bold">${item.Category}</span></td>
        <td>
          <strong style="color: #0f172a;">${item.Name}</strong>
          ${isSelected ? ' <span class="badge" style="background:#10b981; color:#fff; font-size:10px; padding:1px 4px;">當前選定</span>' : ''}
        </td>
        <td class="text-muted" style="font-size: 12px;">${item.CommonName || "—"}</td>
        <td style="text-align: right;" class="val-red font-bold">${Math.round(item.Calories || 0)}</td>
        <td style="text-align: right;" class="val-green font-bold">${item.Protein ? item.Protein.toFixed(1) : 0}</td>
        <td style="text-align: right;" class="val-orange font-bold">${item.Fat ? item.Fat.toFixed(1) : 0}</td>
        <td style="text-align: right;" class="val-blue font-bold">${item.Carbs ? item.Carbs.toFixed(1) : 0}</td>
        <td style="text-align: center;">
          <button type="button" class="btn ${isSelected ? 'btn-teal' : 'btn-outline'}" style="padding: 3px 8px; font-size: 11px;" onclick="applySampleToTargetRecipe('${item.Name.replace(/'/g, "\\'")}')">
            ${isSelected ? '✔ 已選用' : '選用此樣品'}
          </button>
        </td>
      </tr>
    `;
  }).join("");
}

function clearSampleSearch() {
  const inSearch = document.getElementById("inSampleSearch");
  const cboCat = document.getElementById("cboSampleCategory");
  if (inSearch) inSearch.value = "";
  if (cboCat) cboCat.value = "全部";
  filterSampleSelectorList();
}

function applySampleToTargetRecipe(sampleName) {
  if (targetRecipeItemIndex !== null && menuRecipes[targetRecipeItemIndex]) {
    menuRecipes[targetRecipeItemIndex].sampleName = sampleName;
    updateAllCompositionNutrientSummary();
    closeModal("modalSampleSelector");
  }
}

function resetTargetSampleToAuto() {
  if (targetRecipeItemIndex !== null && menuRecipes[targetRecipeItemIndex]) {
    menuRecipes[targetRecipeItemIndex].sampleName = null;
    updateAllCompositionNutrientSummary();
    closeModal("modalSampleSelector");
  }
}


// ==========================================
// 6. 「請引導我」智慧菜單設計嚮導 (Guided Menu Wizard)
// 順序：乳製品 ➔ 蔬菜 ➔ 水果 ➔ 豆魚蛋肉類、油脂類及堅果種子
// ==========================================

const GUIDED_STEPS = [
  {
    stepIndex: 0,
    title: "開立乳製品",
    badge: "第 1 步：乳製品",
    subtitle: "步驟引導 1/4：開立乳製品（每天建議 1~2 杯優質乳品，補充蛋白質與鈣質）",
    desc: "請選擇適合的乳製品（全脂奶、低脂奶、脫脂奶、起司、乳酪等），為早餐或點心注入鈣質與優質蛋白質。",
    defaultMeal: "早餐",
    categoryFilter: (item) => {
      return item.category === "乳品類" || (item.category && item.category.includes("奶"));
    }
  },
  {
    stepIndex: 1,
    title: "開立蔬菜類",
    badge: "第 2 步：蔬菜類",
    subtitle: "步驟引導 2/4：開立蔬菜類（每餐至少 1~2 份各色蔬菜，補充膳食纖維與植化素）",
    desc: "開立深綠色與各色彩色蔬菜（如菠菜、甘藍/高麗菜、胡蘿蔔、花椰菜、菇類等），促進腸道健康並穩定血糖。",
    defaultMeal: "午餐",
    categoryFilter: (item) => {
      return item.category === "蔬菜類" || (item.category && item.category.includes("菜"));
    }
  },
  {
    stepIndex: 2,
    title: "開立水果類",
    badge: "第 3 步：水果類",
    subtitle: "步驟引導 3/4：開立水果類（每天建議 2 份新鮮水果，補充維生素 C 與多酚）",
    desc: "選擇新鮮季節水果（如蘋果、奇異果、香蕉、芭樂、柑橘等），可規劃於早點、午點或隨餐食用。",
    defaultMeal: "早點",
    categoryFilter: (item) => {
      return item.category === "水果類" || (item.category && item.category.includes("果"));
    }
  },
  {
    stepIndex: 3,
    title: "豆魚蛋肉類、油脂類及堅果種子",
    badge: "第 4 步：蛋白質與好油堅果",
    subtitle: "步驟引導 4/4：開立豆魚蛋肉類、油脂類及堅果種子（建構肌肉組織與優質脂肪酸）",
    desc: "先豆、次魚、後蛋、再肉，搭配健康植物油脂（橄欖油、芥花油）與堅果種子（核桃、杏仁等），完成全日健康營養平衡！",
    defaultMeal: "午餐",
    categoryFilter: (item) => {
      return item.category === "豆魚蛋肉" || 
             item.category === "油脂與堅果種子" || 
             (item.category && (item.category.includes("肉") || item.category.includes("豆") || item.category.includes("魚") || item.category.includes("蛋") || item.category.includes("油") || item.category.includes("堅果")));
    }
  }
];

let currentGuidedStepIndex = 0;
let wizardAddedItems = [];

function startGuidedMenuWizard() {
  currentGuidedStepIndex = 0;
  wizardAddedItems = [];
  openModal("modalGuidedMenuWizard");
  renderGuidedStep();
}

function renderGuidedStep() {
  const step = GUIDED_STEPS[currentGuidedStepIndex];
  if (!step) return;

  // 更新頂部步驟資訊
  const titleEl = document.getElementById("wizardStepTitleText");
  const descEl = document.getElementById("wizardStepDescText");
  const subEl = document.getElementById("wizardStepSubtitle");
  const mealSelect = document.getElementById("wizardTargetMeal");

  if (titleEl) titleEl.innerText = step.badge;
  if (descEl) descEl.innerText = step.desc;
  if (subEl) subEl.innerText = step.subtitle;
  if (mealSelect) {
    // 預設切換為該步驟推薦餐次
    mealSelect.value = step.defaultMeal;
  }

  // 渲染步驟導航進度標籤
  const navContainer = document.getElementById("wizardStepperNav");
  if (navContainer) {
    navContainer.innerHTML = GUIDED_STEPS.map((s, idx) => {
      let stateCls = "";
      if (idx === currentGuidedStepIndex) stateCls = "active";
      else if (idx < currentGuidedStepIndex) stateCls = "completed";

      return `
        <div class="wizard-step-item ${stateCls}" onclick="jumpToGuidedStep(${idx})">
          <div class="wizard-step-number">${idx < currentGuidedStepIndex ? "✓" : (idx + 1)}</div>
          <div class="wizard-step-label">${s.title}</div>
        </div>
      `;
    }).join("");
  }

  // 底部按鈕狀態
  const btnPrev = document.getElementById("btnWizardPrev");
  const btnNext = document.getElementById("btnWizardNext");

  if (btnPrev) {
    btnPrev.style.display = currentGuidedStepIndex > 0 ? "inline-block" : "none";
  }

  if (btnNext) {
    if (currentGuidedStepIndex === GUIDED_STEPS.length - 1) {
      btnNext.innerText = "🎉 完成並前往菜單開立";
      btnNext.className = "btn btn-teal";
    } else {
      btnNext.innerText = "下一步 ➔";
      btnNext.className = "btn btn-teal";
    }
  }

  // 清空搜尋框並渲染食材列表
  const searchIn = document.getElementById("wizardSearchInput");
  if (searchIn) searchIn.value = "";
  filterWizardFoodList();
  renderWizardAddedItems();
}

function filterWizardFoodList() {
  const step = GUIDED_STEPS[currentGuidedStepIndex];
  if (!step) return;

  const container = document.getElementById("wizardFoodListContainer");
  if (!container) return;

  if (typeof FOOD_EXCHANGE_DB === "undefined" || !Array.isArray(FOOD_EXCHANGE_DB)) {
    container.innerHTML = '<div style="color: #ef4444; padding: 20px;">食物代換資料庫載入中，請稍候...</div>';
    return;
  }

  const keyword = (document.getElementById("wizardSearchInput")?.value || "").trim().toLowerCase();

  const filtered = FOOD_EXCHANGE_DB.filter(item => {
    const matchCat = step.categoryFilter(item);
    if (!matchCat) return false;
    if (!keyword) return true;
    return (item.foodName && item.foodName.toLowerCase().includes(keyword)) ||
           (item.subCategory && item.subCategory.toLowerCase().includes(keyword)) ||
           (item.notes && item.notes.toLowerCase().includes(keyword));
  });

  if (filtered.length === 0) {
    container.innerHTML = '<div style="color: #94a3b8; grid-column: 1/-1; text-align: center; padding: 30px;">查無符合的食物，請嘗試其他關鍵字</div>';
    return;
  }

  container.innerHTML = filtered.map(item => {
    const p = item.protein || 0;
    const f = item.fat || 0;
    const c = item.carbs || 0;
    const kcal = item.calories || (p * 4 + f * 9 + c * 4);

    return `
      <div class="wizard-food-card" onclick="addFoodFromWizard('${item.foodName.replace(/'/g, "\\'")}', '${(item.category || "自訂").replace(/'/g, "\\'")}', ${item.rawWeight || 100})">
        <div class="wizard-food-header">
          <div class="wizard-food-name">${item.foodName}</div>
          <span class="wizard-food-badge">${item.subCategory || item.category}</span>
        </div>
        <div class="wizard-food-portion">每份份量：${item.portionDesc || (item.rawWeight + 'g')}</div>
        <div class="wizard-food-macros">
          <span>🔥 <strong>${kcal}</strong> kcal</span>
          <span>蛋 ${p}g</span>
          <span>脂 ${f}g</span>
          <span>醣 ${c}g</span>
          <span style="margin-left: auto; color: #0d9488; font-weight: 700;">➕ 加入</span>
        </div>
      </div>
    `;
  }).join("");
}

function addFoodFromWizard(foodName, category, rawWeight) {
  const mealSelect = document.getElementById("wizardTargetMeal");
  const meal = mealSelect ? mealSelect.value : "午餐";

  // 計算顏色
  let color = detectFoodColor(foodName, category);

  const newRecipe = {
    meal: meal,
    dish: "",
    food: foodName,
    category: category,
    ex: 1,
    rawWeight: rawWeight || 100,
    color: color
  };

  // 加入全域菜單
  menuRecipes.push(newRecipe);

  // 記錄於嚮導本輪加入紀錄
  wizardAddedItems.push(newRecipe);

  // 即時更新介面
  renderWizardAddedItems();
  renderMenuRecipes();
}

function renderWizardAddedItems() {
  const container = document.getElementById("wizardAddedItemsList");
  const countBadge = document.getElementById("wizardAddedCount");
  if (countBadge) countBadge.innerText = wizardAddedItems.length;

  if (!container) return;

  if (wizardAddedItems.length === 0) {
    container.innerHTML = '<div style="color: #94a3b8; text-align: center; padding: 10px;">尚未加入任何食材，請從上方點選食物加入。</div>';
    return;
  }

  container.innerHTML = wizardAddedItems.map((r, idx) => {
    return `
      <span style="display: inline-flex; align-items: center; gap: 4px; background: #ffffff; border: 1px solid #cbd5e1; border-radius: 14px; padding: 3px 10px; margin: 3px; font-size: 12px;">
        <span style="color: #0369a1; font-weight: 600;">[${r.meal}]</span>
        <strong>${r.food}</strong> (${r.rawWeight}g)
        <button type="button" onclick="removeWizardAddedItem(${idx})" style="background: none; border: none; color: #ef4444; cursor: pointer; font-weight: bold; margin-left: 2px;">×</button>
      </span>
    `;
  }).join("");
}

function removeWizardAddedItem(idx) {
  const target = wizardAddedItems[idx];
  if (!target) return;

  // 從 menuRecipes 中移除
  const recIndex = menuRecipes.lastIndexOf(target);
  if (recIndex !== -1) {
    menuRecipes.splice(recIndex, 1);
  }

  wizardAddedItems.splice(idx, 1);
  renderWizardAddedItems();
  renderMenuRecipes();
}

function clearWizardAddedItems() {
  if (wizardAddedItems.length === 0) return;
  if (!confirm("確定要清空本次引導所加入的所有食材嗎？")) return;

  wizardAddedItems.forEach(target => {
    const recIndex = menuRecipes.lastIndexOf(target);
    if (recIndex !== -1) {
      menuRecipes.splice(recIndex, 1);
    }
  });

  wizardAddedItems = [];
  renderWizardAddedItems();
  renderMenuRecipes();
}

function jumpToGuidedStep(idx) {
  if (idx >= 0 && idx < GUIDED_STEPS.length) {
    currentGuidedStepIndex = idx;
    renderGuidedStep();
  }
}

function prevGuidedStep() {
  if (currentGuidedStepIndex > 0) {
    currentGuidedStepIndex--;
    renderGuidedStep();
  }
}

function nextGuidedStep() {
  if (currentGuidedStepIndex < GUIDED_STEPS.length - 1) {
    currentGuidedStepIndex++;
    renderGuidedStep();
  } else {
    // 完成所有引導步驟
    closeModal("modalGuidedMenuWizard");
    // 平滑切換至「菜單開立」分頁
    switchTab("tab-menu");
    alert("🎉 恭喜！引導式菜單設計已順序完成！\n\n已為您切換至「菜單開立」分頁，您可檢視完整菜單、調整克數與菜名，或查看營養素比對。");
  }
}
