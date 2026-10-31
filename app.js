// 飲食營養菜單設計系統 - 前端核心邏輯

// 全域狀態
let isDiabeticMode = false;
let isDiseaseMode = false;
let lastRatioLimitSignature = "";
let ratioLimitDialogReturnFocus = null;
let currentDeviceMode = "auto";
let exchangesData = [];
let activeMealKeys = ["b", "ms", "l", "as", "d", "ns"];
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
  { meal: "晚餐", dish: "", food: "豆腐", category: "豆魚蛋肉", ex: 1, rawWeight: 110, color: "白色" },
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
}

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
  document.getElementById("valIbw").innerText = `${ibw.toFixed(2)}`;
  document.getElementById("valIbwPercent").innerText = `${ibwPercent.toFixed(1)} %`;
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
        <td>${item.p}</td>
        <td>${item.f}</td>
        <td>${item.c}</td>
        <td>${item.kcal}</td>
        <td class="design-ex-cell">
          <input type="number" class="cell-input design-ex-input" step="0.5" min="0"
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
  calcExchangeTotals();
}

function renderMealInput(item, idx, mealKey) {
  return `<td class="meal-col meal-${mealKey}"><input type="number" class="cell-input meal-ex-input" step="0.5" min="0" value="${item[mealKey]}" ${item.ex > 0 && activeMealKeys.includes(mealKey) ? "" : "disabled"} oninput="onMealExChanged(${idx}, '${mealKey}', this.value)"></td>`;
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

  document.getElementById("sumProteinGrams").innerText = `${Math.round(totalP)} g`;
  document.getElementById("sumFatGrams").innerText = `${Math.round(totalF)} g`;
  document.getElementById("sumCarbGrams").innerText = `${Math.round(totalC)} g`;
  document.getElementById("sumTotalCalories").innerText = `${Math.round(totalKcal)} kcal`;

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
    rawWeight: 100,
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
