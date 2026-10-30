// 商業配方與各項常數定義
const COMMERCIAL_FORMULAS_DB = [
  { name: "低氮澱粉", spec: "100公克/包", p: 0.0, f: 0.0, c: 50.0, kcal: 200.0, note: "腎臟病低蛋白專用" },
  { name: "三多低蛋白配方LPF", spec: "50公克/包", p: 4.0, f: 8.5, c: 32.5, kcal: 222.0, note: "未洗腎慢性腎臟病適用" },
  { name: "桂格完膳營養素", spec: "250ml/罐", p: 10.6, f: 9.2, c: 31.0, kcal: 250.0, note: "均衡營養配方" },
  { name: "糖尿立攝適", spec: "237ml/罐", p: 11.8, f: 10.7, c: 23.7, kcal: 237.0, note: "糖尿病專用配方，低GI" },
  { name: "葡勝納", spec: "250ml/罐", p: 10.5, f: 11.2, c: 27.5, kcal: 250.0, note: "糖尿病專用穩醣配方" },
  { name: "普寧腎", spec: "237ml/罐", p: 19.0, f: 22.8, c: 47.4, kcal: 475.0, note: "洗腎透析高蛋白高熱量" },
  { name: "腎補納", spec: "237ml/罐", p: 8.0, f: 22.8, c: 59.0, kcal: 474.0, note: "未洗腎低蛋白高熱量" },
  { name: "愛速康", spec: "237ml/罐", p: 10.5, f: 8.3, c: 33.7, kcal: 250.0, note: "標準等滲透壓管灌/口飲" },
  { name: "愛速康精選", spec: "237ml/罐", p: 10.5, f: 8.3, c: 33.7, kcal: 250.0, note: "水溶性纖維配方" },
  { name: "健力體", spec: "237ml/罐", p: 10.5, f: 8.8, c: 34.0, kcal: 250.0, note: "含優質膳食纖維" },
  { name: "安素(原味)", spec: "237ml/罐", p: 10.5, f: 7.5, c: 35.0, kcal: 250.0, note: "經典均衡配方" },
  { name: "安素(香草)", spec: "237ml/罐", p: 10.5, f: 7.5, c: 35.0, kcal: 250.0, note: "香濃香草風味" },
  { name: "愛美力", spec: "237ml/罐", p: 9.5, f: 8.3, c: 34.5, kcal: 250.0, note: "經典管灌口飲配方" },
  { name: "倍力素", spec: "220ml/罐", p: 16.0, f: 7.0, c: 44.0, kcal: 302.0, note: "腫瘤癌症專用高蛋白EPA" },
  { name: "諾沛", spec: "250ml/罐", p: 15.8, f: 10.0, c: 38.0, kcal: 300.0, note: "重症高蛋白濃縮配方" },
  { name: "雙卡", spec: "237ml/罐", p: 20.0, f: 22.0, c: 48.0, kcal: 475.0, note: "2.0 kcal/ml 濃縮高熱量" },
  { name: "保惠壯", spec: "64公克/包", p: 0.0, f: 0.0, c: 64.0, kcal: 256.0, note: "純麥芽糊精熱量補充" },
  { name: "易能充", spec: "100ml/包", p: 0.0, f: 7.0, c: 24.0, kcal: 160.0, note: "濃縮熱量與優質油脂補充" },
  { name: "護力養PLUS", spec: "57克/2匙", p: 10.5, f: 8.5, c: 34.0, kcal: 250.0, note: "添加膳食纖維與微量元素" },
  { name: "優質營養配方(原味)", spec: "56克/7匙", p: 10.5, f: 8.5, c: 33.0, kcal: 250.0, note: "高鈣高纖粉末營養品" },
  { name: "護力養益生菌", spec: "25公克/包", p: 4.0, f: 3.0, c: 14.0, kcal: 100.0, note: "益生菌配方維持消化道機能" },
  { name: "護力養乳清蛋白", spec: "600克/袋(每匙10g)", p: 8.0, f: 0.2, c: 0.5, kcal: 36.0, note: "快速吸收高純度乳清蛋白" },
  { name: "美日均衡", spec: "250ml/罐", p: 10.5, f: 8.5, c: 33.0, kcal: 250.0, note: "每日健康維持均衡飲" },
  { name: "糖怡", spec: "25公克/匙", p: 0.0, f: 0.0, c: 24.0, kcal: 96.0, note: "水飴糖粉純熱量補充" },
  { name: "粉怡", spec: "100公克/包", p: 0.0, f: 0.0, c: 95.0, kcal: 380.0, note: "高溶解度熱量補充粉" },
  { name: "P93乳清蛋白粉", spec: "100公克(每匙10g)", p: 9.3, f: 0.1, c: 0.1, kcal: 38.5, note: "93%高純度濃縮分離乳清" }
];

function getDefaultExchanges(isDiabeticLowProtein = false) {
  const list = [
    { category: "全脂奶", icon: "🥛", unit: "份 (240ml)", p: 8, f: 8, c: 12, kcal: 150, ex: 0, b: 0, ms: 0, l: 0, as: 0, d: 0, ns: 0 },
    { category: "低脂奶", icon: "🥛", unit: "份 (240ml)", p: 8, f: 4, c: 12, kcal: 120, ex: 1, b: 1, ms: 0, l: 0, as: 0, d: 0, ns: 0 },
    { category: "脫脂奶", icon: "🥛", unit: "份 (240ml)", p: 8, f: 0, c: 12, kcal: 80,  ex: 0, b: 0, ms: 0, l: 0, as: 0, d: 0, ns: 0 }
  ];

  if (isDiabeticLowProtein) {
    list.push({ category: "米食類", icon: "🍚", unit: "份 (1/4碗飯)", p: 1.5, f: 0, c: 15, kcal: 70, ex: 3, b: 1, ms: 0, l: 1, as: 0, d: 1, ns: 0 });
    list.push({ category: "麵食類", icon: "🍜", unit: "份 (1/2碗麵)", p: 2.5, f: 0, c: 15, kcal: 70, ex: 3, b: 1, ms: 0, l: 1, as: 0, d: 1, ns: 0 });
  } else {
    list.push({ category: "穀物雜糧類", icon: "🍚", unit: "份 (1/4碗飯或半碗麵)", p: 2, f: 0, c: 15, kcal: 70, ex: 10, b: 3, ms: 0, l: 3.5, as: 0, d: 3.5, ns: 0 });
  }

  list.push(
    { category: "豆魚蛋肉類(低脂)", icon: "🐟", unit: "份 (1兩瘦肉/蛋)", p: 7, f: 3, c: 0, kcal: 55, ex: 2, b: 1, ms: 0, l: 1, as: 0, d: 0, ns: 0 },
    { category: "豆魚蛋肉類(中脂)", icon: "🍗", unit: "份 (1兩肉/中脂豆腐)", p: 7, f: 5, c: 0, kcal: 75, ex: 3, b: 0, ms: 0, l: 1.5, as: 0, d: 1.5, ns: 0 },
    { category: "豆魚蛋肉類(高脂)", icon: "🥩", unit: "份 (五花/香腸/肉鬆)", p: 7, f: 9, c: 0, kcal: 120, ex: 0, b: 0, ms: 0, l: 0, as: 0, d: 0, ns: 0 },
    { category: "蔬菜類", icon: "🥦", unit: "份 (100g/半碗煮熟)", p: 1, f: 0, c: 5, kcal: 25, ex: 3, b: 0, ms: 0, l: 1.5, as: 0, d: 1.5, ns: 0 },
    { category: "水果類", icon: "🍎", unit: "份 (拳頭大/1碗切塊)", p: 0, f: 0, c: 15, kcal: 60, ex: 2, b: 0, ms: 1, l: 0, as: 1, d: 0, ns: 0 },
    { category: "油脂類", icon: "🫒", unit: "份 (1茶匙 5g)", p: 0, f: 5, c: 0, kcal: 45, ex: 5, b: 1, ms: 0, l: 2, as: 0, d: 2, ns: 0 },
    { category: "堅果種子類", icon: "🥜", unit: "份 (1湯匙約10g)", p: 0, f: 5, c: 0, kcal: 45, ex: 1, b: 0, ms: 0, l: 0, as: 1, d: 0, ns: 0 }
  );

  return list;
}
