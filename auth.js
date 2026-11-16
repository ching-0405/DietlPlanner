// ==========================================
// 飲食營養菜單設計系統 - 會員帳密驗證、授權期效與獨立儲存模組 (auth.js)
// ==========================================

const AUTH_CONFIG = {
  SALT: "DietPlanner_Salt_2026_Secure_Auth",
  STORAGE_CURRENT_USER: "diet_current_user_session",
  STORAGE_PREFIX: "diet_user_data_",
  ADMIN_CODE: "admin888" // 管理員快捷萬用通行碼
};

// 簡易加密雜湊演算法
function generateSecureHash(text) {
  let hash = 0;
  const str = text + AUTH_CONFIG.SALT;
  for (let i = 0; i < str.length; i++) {
    const char = str.charCodeAt(i);
    hash = ((hash << 5) - hash) + char;
    hash |= 0;
  }
  return Math.abs(hash).toString(16).padStart(8, '0');
}

// 產生授權密碼 (包含：帳號 + 到期日 + 數位簽章)
// 格式：DP-{到期日YYYYMMDD}-{簽章8碼}
function generateAccountLicenseKey(username, expireDateStr) {
  const cleanDate = expireDateStr.replace(/-/g, "");
  const signature = generateSecureHash(username.trim().toLowerCase() + cleanDate);
  return `DP-${cleanDate}-${signature.toUpperCase()}`;
}

// 驗證授權密碼是否合法且回傳有效到期日
function verifyAccountLicense(username, password) {
  if (!username || !password) return { valid: false, error: "請輸入帳號與授權密碼" };
  
  const user = username.trim().toLowerCase();
  const pwd = password.trim();

  // 特殊管理員全權通行碼
  if (pwd === AUTH_CONFIG.ADMIN_CODE) {
    const farDate = new Date();
    farDate.setFullYear(farDate.getFullYear() + 99);
    return {
      valid: true,
      username: user,
      expireDate: farDate.toISOString().split("T")[0],
      isAdmin: true,
      daysLeft: 9999
    };
  }

  // 格式檢查：DP-YYYYMMDD-XXXXXXXX
  const parts = pwd.split("-");
  if (parts.length !== 3 || parts[0] !== "DP" || parts[1].length !== 8) {
    return { valid: false, error: "授權密碼格式不正確，請確認輸入由管理員核發之專屬密碼！" };
  }

  const datePart = parts[1];
  const expectedSig = generateSecureHash(user + datePart).toUpperCase();
  if (parts[2].toUpperCase() !== expectedSig) {
    return { valid: false, error: "此授權密碼與帳號不匹配，請確認帳號名稱是否拼寫正確！" };
  }

  // 檢查到期日
  const year = parseInt(datePart.substring(0, 4), 10);
  const month = parseInt(datePart.substring(4, 6), 10) - 1;
  const day = parseInt(datePart.substring(6, 8), 10);
  const expireDate = new Date(year, month, day, 23, 59, 59);

  const now = new Date();
  const diffTime = expireDate.getTime() - now.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (diffDays <= 0) {
    return { 
      valid: false, 
      error: `您的帳號授權已於 ${year}/${month+1}/${day} 到期，請聯繫管理員續約！`,
      isExpired: true,
      expireDate: `${year}-${String(month+1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
    };
  }

  return {
    valid: true,
    username: user,
    expireDate: `${year}/${String(month+1).padStart(2, '0')}/${String(day).padStart(2, '0')}`,
    daysLeft: diffDays
  };
}

// ==========================================
// 會員 Session 管理
// ==========================================
let currentAuthSession = null;

function getCurrentAuthUser() {
  if (currentAuthSession) return currentAuthSession;
  try {
    const raw = localStorage.getItem(AUTH_CONFIG.STORAGE_CURRENT_USER);
    if (raw) {
      const data = JSON.parse(raw);
      const check = verifyAccountLicense(data.username, data.password);
      if (check.valid) {
        currentAuthSession = { ...data, daysLeft: check.daysLeft, expireDate: check.expireDate };
        return currentAuthSession;
      }
    }
  } catch (e) {
    console.error("Auth session parse error:", e);
  }
  return null;
}

function initAuthSession() {
  const user = getCurrentAuthUser();
  if (user) {
    loadUserDataForCurrentUser();
    updateAuthStatusUI();
  } else {
    // 預先產生示範帳號給新使用者體驗
    showLoginModal();
  }
}

// 登入
function loginUser(username, password) {
  const result = verifyAccountLicense(username, password);
  if (!result.valid) {
    return result;
  }

  currentAuthSession = {
    username: result.username,
    password: password.trim(),
    expireDate: result.expireDate,
    daysLeft: result.daysLeft,
    loginTime: new Date().toISOString()
  };

  localStorage.setItem(AUTH_CONFIG.STORAGE_CURRENT_USER, JSON.stringify(currentAuthSession));
  
  // 載入該使用者專屬資料
  loadUserDataForCurrentUser();
  updateAuthStatusUI();
  return { valid: true, user: currentAuthSession };
}

// 登出
function logoutUser() {
  saveUserDataForCurrentUser();
  localStorage.removeItem(AUTH_CONFIG.STORAGE_CURRENT_USER);
  currentAuthSession = null;
  updateAuthStatusUI();
  showLoginModal();
}

// ==========================================
// 使用者獨立資料儲存 (依帳號隔離)
// ==========================================
function getUserStorageKey() {
  const user = getCurrentAuthUser();
  const uname = user ? user.username : "guest";
  return `${AUTH_CONFIG.STORAGE_PREFIX}${uname}`;
}

function saveUserDataForCurrentUser() {
  const user = getCurrentAuthUser();
  if (!user) return false;

  const storageKey = getUserStorageKey();
  
  const assessmentData = {};
  const inputIds = [
    'inName', 'inChartNo', 'inGender', 'inAge', 'inHeight', 'inWeight',
    'inCreatinine', 'inKneeHeight', 'inUlnaLength', 'inCalfCirc', 'inArmCirc',
    'inActivity', 'inStress', 'inTargetKcal',
    'inRatioC', 'inRatioP', 'inRatioF', 'inTotalMenuDays'
  ];

  inputIds.forEach(id => {
    const el = document.getElementById(id);
    if (el) assessmentData[id] = el.value;
  });

  const payload = {
    username: user.username,
    updatedAt: new Date().toISOString(),
    assessment: assessmentData,
    exchanges: exchangesData,
    activeMealKeys: activeMealKeys,
    totalMenuDays: typeof totalMenuDays !== "undefined" ? totalMenuDays : 7,
    currentMenuDay: typeof currentMenuDay !== "undefined" ? currentMenuDay : 1,
    menuRecipes: typeof menuRecipes !== "undefined" ? menuRecipes : []
  };

  try {
    localStorage.setItem(storageKey, JSON.stringify(payload));
    return true;
  } catch (e) {
    console.error("Save error:", e);
    return false;
  }
}

function loadUserDataForCurrentUser() {
  const user = getCurrentAuthUser();
  if (!user) return false;

  const storageKey = getUserStorageKey();
  const raw = localStorage.getItem(storageKey);
  if (!raw) return false;

  try {
    const data = JSON.parse(raw);
    
    if (data.assessment) {
      Object.keys(data.assessment).forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = data.assessment[id];
      });
    }

    if (Array.isArray(data.exchanges)) {
      exchangesData = data.exchanges;
    }

    if (Array.isArray(data.activeMealKeys)) {
      activeMealKeys = data.activeMealKeys;
    }

    if (data.totalMenuDays && typeof totalMenuDays !== "undefined") {
      totalMenuDays = data.totalMenuDays;
      const daysInput = document.getElementById("inTotalMenuDays");
      if (daysInput) daysInput.value = totalMenuDays;
      if (typeof updateMenuDayOptions === "function") updateMenuDayOptions();
    }

    if (data.currentMenuDay && typeof currentMenuDay !== "undefined") {
      currentMenuDay = data.currentMenuDay;
    }

    if (Array.isArray(data.menuRecipes) && typeof menuRecipes !== "undefined") {
      menuRecipes = data.menuRecipes;
    }

    if (typeof calcAssessment === "function") calcAssessment();
    if (typeof calcMacroTargets === "function") calcMacroTargets();
    if (typeof renderExchangeTable === "function") renderExchangeTable();
    if (typeof calcExchangeTotals === "function") calcExchangeTotals();
    if (typeof renderMenuRecipes === "function") renderMenuRecipes();
    if (typeof updateMealChipsBar === "function") updateMealChipsBar();
    if (typeof updateAllCompositionNutrientSummary === "function") updateAllCompositionNutrientSummary();
    if (typeof syncSidebarClientCard === "function") syncSidebarClientCard();

    return true;
  } catch (e) {
    console.error("Load user data error:", e);
    return false;
  }
}

// ==========================================
// 介面連動
// ==========================================
function updateAuthStatusUI() {
  const user = getCurrentAuthUser();
  const authBar = document.getElementById("authStatusBar");
  const modal = document.getElementById("modalAuthLogin");

  if (user) {
    if (authBar) {
      authBar.style.display = "flex";
      const lblUser = document.getElementById("lblAuthUsername");
      const lblExp = document.getElementById("lblAuthExpireDesc");
      if (lblUser) lblUser.innerText = user.username;
      if (lblExp) {
        lblExp.innerHTML = `會員有效期限：<strong style="color: #059669;">剩餘 ${user.daysLeft} 天</strong>（有效至 ${user.expireDate}）`;
      }
    }
    if (modal) modal.classList.remove("show");
  } else {
    if (authBar) authBar.style.display = "none";
    showLoginModal();
  }
}

function showLoginModal() {
  const modal = document.getElementById("modalAuthLogin");
  if (modal) {
    modal.classList.add("show");
    // 預設填入示範帳號 user1 與有效金鑰，方便使用者第一次測試體驗
    const inUser = document.getElementById("inAuthUsername");
    const inPass = document.getElementById("inAuthPassword");
    if (inUser && !inUser.value) inUser.value = "user1";
    if (inPass && !inPass.value) inPass.value = generateAccountLicenseKey("user1", "2027-11-16");
  }
}

function handleLoginSubmit(event) {
  if (event) event.preventDefault();
  const inUser = document.getElementById("inAuthUsername");
  const inPass = document.getElementById("inAuthPassword");
  const errBox = document.getElementById("authErrorMessage");

  const username = inUser ? inUser.value.trim() : "";
  const password = inPass ? inPass.value.trim() : "";

  const result = loginUser(username, password);
  if (result.valid) {
    if (errBox) errBox.style.display = "none";
  } else {
    if (errBox) {
      errBox.style.display = "block";
      errBox.innerText = result.error || "登入失敗！";
    }
  }
}

// 管理員發卡工具功能
function openAdminKeyGenModal() {
  const modal = document.getElementById("modalAdminKeyGen");
  if (modal) {
    modal.classList.add("show");
    setGenDays(365); // 預設 1 年
    generateKeyAndDisplay();
  }
}

function setGenDays(days) {
  const dateInput = document.getElementById("inGenExpireDate");
  if (dateInput) {
    const target = new Date();
    target.setDate(target.getDate() + days);
    dateInput.value = target.toISOString().split("T")[0];
  }
}

function generateKeyAndDisplay() {
  const inUser = document.getElementById("inGenUsername");
  const inDate = document.getElementById("inGenExpireDate");
  const resultBox = document.getElementById("genResultBox");
  const outKey = document.getElementById("outGeneratedKey");
  const outDesc = document.getElementById("outGenDesc");

  const username = inUser ? inUser.value.trim() : "user";
  const expireDate = inDate ? inDate.value : "2027-11-16";

  if (!username) {
    alert("請輸入客戶帳號名稱！");
    return;
  }

  const licenseKey = generateAccountLicenseKey(username, expireDate);
  if (outKey) outKey.value = licenseKey;
  if (outDesc) {
    outDesc.innerText = `帳號：${username} ｜ 到期日：${expireDate} ｜ 客戶以此密碼登入即可獲得對應使用天數`;
  }
  if (resultBox) resultBox.style.display = "block";
}

function copyGeneratedKey() {
  const outKey = document.getElementById("outGeneratedKey");
  if (outKey) {
    outKey.select();
    navigator.clipboard.writeText(outKey.value).then(() => {
      alert(`授權密碼已成功複製到剪貼簿：\n${outKey.value}`);
    }).catch(() => {
      alert(`授權密碼：\n${outKey.value}`);
    });
  }
}
