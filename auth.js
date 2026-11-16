// ==========================================
// 飲食營養菜單設計系統 - Google 試算表即時會員帳密驗證與獨立存檔模組 (auth.js)
// ==========================================

const AUTH_CONFIG = {
  // Google Apps Script 雲端試算表 Web App URL
  API_URL: "https://script.google.com/macros/s/AKfycbyxmeB8NCJFKF4iY2Yu7dIBHUluQF8zMnJASJ_WRZq2DUSRlqcoey4b7UfkuiRW_FYeaA/exec",
  STORAGE_CURRENT_USER: "diet_current_user_session",
  STORAGE_PREFIX: "diet_user_data_",
  ADMIN_CODE: "admin888" // 管理員快捷萬用通行密碼
};

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
      // 重新檢查到期日
      if (data && data.expireDate) {
        const parts = data.expireDate.split("/");
        if (parts.length === 3) {
          const expDate = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10), 23, 59, 59);
          const now = new Date();
          const diffDays = Math.ceil((expDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
          if (diffDays <= 0) {
            localStorage.removeItem(AUTH_CONFIG.STORAGE_CURRENT_USER);
            return null;
          }
          data.daysLeft = diffDays;
        }
      }
      currentAuthSession = data;
      return currentAuthSession;
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
    showLoginModal();
  }
}

// 登入驗證（直接連線 Google 試算表即時驗證）
async function loginUser(username, password) {
  const user = (username || "").trim().toLowerCase();
  const pwd = (password || "").trim();

  if (!user || !pwd) {
    return { valid: false, error: "請輸入帳號與密碼！" };
  }

  // 管理員萬用通行
  if (pwd === AUTH_CONFIG.ADMIN_CODE) {
    const farDate = new Date();
    farDate.setFullYear(farDate.getFullYear() + 99);
    const expStr = `${farDate.getFullYear()}/12/31`;
    currentAuthSession = {
      username: user,
      expireDate: expStr,
      daysLeft: 9999,
      loginTime: new Date().toISOString()
    };
    localStorage.setItem(AUTH_CONFIG.STORAGE_CURRENT_USER, JSON.stringify(currentAuthSession));
    loadUserDataForCurrentUser();
    updateAuthStatusUI();
    return { valid: true, user: currentAuthSession };
  }

  try {
    const queryUrl = `${AUTH_CONFIG.API_URL}?username=${encodeURIComponent(user)}&password=${encodeURIComponent(pwd)}`;
    const response = await fetch(queryUrl);
    const result = await response.json();

    if (result && result.success) {
      currentAuthSession = {
        username: result.username,
        expireDate: result.expireDate,
        daysLeft: result.daysLeft,
        loginTime: new Date().toISOString()
      };
      localStorage.setItem(AUTH_CONFIG.STORAGE_CURRENT_USER, JSON.stringify(currentAuthSession));
      loadUserDataForCurrentUser();
      updateAuthStatusUI();
      return { valid: true, user: currentAuthSession };
    } else {
      return { valid: false, error: result.message || "帳號或密碼錯誤！" };
    }
  } catch (err) {
    console.error("API 連線失敗：", err);
    return { valid: false, error: "無法連線至 Google 試算表驗證伺服器，請檢查網路連線或稍後再試！" };
  }
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
// 使用者獨立資料儲存 (依 Google 試算表帳號隔離)
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
  if (modal) modal.classList.add("show");
}

async function handleLoginSubmit(event) {
  if (event) event.preventDefault();
  const inUser = document.getElementById("inAuthUsername");
  const inPass = document.getElementById("inAuthPassword");
  const errBox = document.getElementById("authErrorMessage");
  const btnSubmit = document.querySelector("#modalAuthLogin .auth-submit-btn");

  const username = inUser ? inUser.value.trim() : "";
  const password = inPass ? inPass.value.trim() : "";

  if (errBox) errBox.style.display = "none";
  if (btnSubmit) {
    btnSubmit.disabled = true;
    btnSubmit.innerText = "連線 Google 試算表驗證中...";
  }

  const result = await loginUser(username, password);

  if (btnSubmit) {
    btnSubmit.disabled = false;
    btnSubmit.innerText = "立即登入系統";
  }

  if (result.valid) {
    if (errBox) errBox.style.display = "none";
  } else {
    if (errBox) {
      errBox.style.display = "block";
      errBox.innerText = result.error || "登入失敗！";
    }
  }
}
