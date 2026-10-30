using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using NutritionPlannerApp.Models;

namespace NutritionPlannerApp.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataDir = Path.Combine(appDir, "Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _dbPath = Path.Combine(dataDir, "nutrition_database.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            // 1. 個案資料表
            string createClientsTable = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ChartNumber TEXT,
                    Gender TEXT DEFAULT '男',
                    Age REAL DEFAULT 30,
                    Height REAL DEFAULT 170,
                    Weight REAL DEFAULT 65,
                    Creatinine REAL DEFAULT 0.9,
                    ActivityFactor REAL DEFAULT 1.3,
                    StressFactor REAL DEFAULT 1.0,
                    KneeHeight REAL DEFAULT 50,
                    UlnaLength REAL DEFAULT 25,
                    HipCircumference REAL DEFAULT 90,
                    ArmCircumference REAL DEFAULT 28,
                    TargetCalories REAL DEFAULT 2000,
                    CarbRatio REAL DEFAULT 50,
                    ProteinRatio REAL DEFAULT 20,
                    FatRatio REAL DEFAULT 30,
                    Notes TEXT,
                    CreatedAt TEXT,
                    UpdatedAt TEXT
                );";

            // 2. 商業營養配方表
            string createFormulasTable = @"
                CREATE TABLE IF NOT EXISTS CommercialFormulas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Spec TEXT,
                    Protein REAL DEFAULT 0,
                    Fat REAL DEFAULT 0,
                    Carbs REAL DEFAULT 0,
                    Calories REAL DEFAULT 0,
                    Notes TEXT
                );";

            // 3. 菜單設計歷史紀錄表
            string createPlansTable = @"
                CREATE TABLE IF NOT EXISTS DietPlans (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientId INTEGER,
                    ClientName TEXT,
                    Title TEXT,
                    PlanDate TEXT,
                    TargetCalories REAL,
                    ExchangesJson TEXT,
                    FormulasJson TEXT,
                    SummaryNotes TEXT,
                    FOREIGN KEY(ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
                );";

            using (var cmd = new SqliteCommand(createClientsTable, conn)) cmd.ExecuteNonQuery();
            using (var cmd = new SqliteCommand(createFormulasTable, conn)) cmd.ExecuteNonQuery();
            using (var cmd = new SqliteCommand(createPlansTable, conn)) cmd.ExecuteNonQuery();

            // 若商業配方表為空，寫入 Excel 預載完整配方庫
            string countQuery = "SELECT COUNT(*) FROM CommercialFormulas;";
            using (var countCmd = new SqliteCommand(countQuery, conn))
            {
                long count = (long)(countCmd.ExecuteScalar() ?? 0);
                if (count == 0)
                {
                    SeedDefaultFormulas(conn);
                }
            }
        }

        private void SeedDefaultFormulas(SqliteConnection conn)
        {
            var defaultFormulas = new List<(string Name, string Spec, double P, double F, double C, double Kcal, string Note)>
            {
                ("低氮澱粉", "100公克/包", 0.0, 0.0, 50.0, 200.0, "腎臟病低蛋白專用"),
                ("三多低蛋白配方LPF", "50公克/包", 4.0, 8.5, 32.5, 222.0, "未洗腎慢性腎臟病適用"),
                ("桂格完膳營養素", "250ml/罐", 10.6, 9.2, 31.0, 250.0, "均衡營養配方"),
                ("糖尿立攝適", "237ml/罐", 11.8, 10.7, 23.7, 237.0, "糖尿病專用配方，低GI"),
                ("葡勝納", "250ml/罐", 10.5, 11.2, 27.5, 250.0, "糖尿病專用穩醣配方"),
                ("普寧腎", "237ml/罐", 19.0, 22.8, 47.4, 475.0, "洗腎透析高蛋白高熱量"),
                ("腎補納", "237ml/罐", 8.0, 22.8, 59.0, 474.0, "未洗腎低蛋白高熱量"),
                ("愛速康", "237ml/罐", 10.5, 8.3, 33.7, 250.0, "標準等滲透壓管灌/口飲"),
                ("愛速康精選", "237ml/罐", 10.5, 8.3, 33.7, 250.0, "水溶性纖維配方"),
                ("健力體", "237ml/罐", 10.5, 8.8, 34.0, 250.0, "含優質膳食纖維"),
                ("安素(原味)", "237ml/罐", 10.5, 7.5, 35.0, 250.0, "經典均衡配方"),
                ("安素(香草)", "237ml/罐", 10.5, 7.5, 35.0, 250.0, "香濃香草風味"),
                ("愛美力", "237ml/罐", 9.5, 8.3, 34.5, 250.0, "經典管灌口飲配方"),
                ("倍力素", "220ml/罐", 16.0, 7.0, 44.0, 302.0, "腫瘤癌症專用高蛋白EPA"),
                ("諾沛", "250ml/罐", 15.8, 10.0, 38.0, 300.0, "重症高蛋白濃縮配方"),
                ("雙卡", "237ml/罐", 20.0, 22.0, 48.0, 475.0, "2.0 kcal/ml 濃縮高熱量"),
                ("保惠壯", "64公克/包", 0.0, 0.0, 64.0, 256.0, "純麥芽糊精熱量補充"),
                ("易能充", "100ml/包", 0.0, 7.0, 24.0, 160.0, "濃縮熱量與優質油脂補充"),
                ("護力養PLUS", "57克/2匙", 10.5, 8.5, 34.0, 250.0, "添加膳食纖維與微量元素"),
                ("優質營養配方(原味)", "56克/7匙", 10.5, 8.5, 33.0, 250.0, "高鈣高纖粉末營養品"),
                ("護力養益生菌", "25公克/包", 4.0, 3.0, 14.0, 100.0, "益生菌配方維持消化道機能"),
                ("護力養乳清蛋白", "600克/袋(每匙10g)", 8.0, 0.2, 0.5, 36.0, "快速吸收高純度乳清蛋白"),
                ("美日均衡", "250ml/罐", 10.5, 8.5, 33.0, 250.0, "每日健康維持均衡飲"),
                ("糖怡", "25公克/匙", 0.0, 0.0, 24.0, 96.0, "水飴糖粉純熱量補充"),
                ("粉怡", "100公克/包", 0.0, 0.0, 95.0, 380.0, "高溶解度熱量補充粉"),
                ("P93乳清蛋白粉", "100公克(每匙10g)", 9.3, 0.1, 0.1, 38.5, "93%高純度濃縮分離乳清")
            };

            using var transaction = conn.BeginTransaction();
            foreach (var f in defaultFormulas)
            {
                string insertSql = @"
                    INSERT INTO CommercialFormulas (Name, Spec, Protein, Fat, Carbs, Calories, Notes)
                    VALUES (@Name, @Spec, @Protein, @Fat, @Carbs, @Calories, @Notes);";
                using var cmd = new SqliteCommand(insertSql, conn, transaction);
                cmd.Parameters.AddWithValue("@Name", f.Name);
                cmd.Parameters.AddWithValue("@Spec", f.Spec);
                cmd.Parameters.AddWithValue("@Protein", f.P);
                cmd.Parameters.AddWithValue("@Fat", f.F);
                cmd.Parameters.AddWithValue("@Carbs", f.C);
                cmd.Parameters.AddWithValue("@Calories", f.Kcal);
                cmd.Parameters.AddWithValue("@Notes", f.Note);
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        // 載入六大類食物份數標準對照表
        public static List<FoodExchangeItem> GetDefaultFoodExchanges(bool isDiabeticLowProtein = false)
        {
            var list = new List<FoodExchangeItem>
            {
                new FoodExchangeItem { Category = "全脂奶", Icon = "🥛", UnitName = "份 (240ml)", ProteinPerUnit = 8, FatPerUnit = 8, CarbPerUnit = 12, CaloriesPerUnit = 150 },
                new FoodExchangeItem { Category = "低脂奶", Icon = "🥛", UnitName = "份 (240ml)", ProteinPerUnit = 8, FatPerUnit = 4, CarbPerUnit = 12, CaloriesPerUnit = 120 },
                new FoodExchangeItem { Category = "脫脂奶", Icon = "🥛", UnitName = "份 (240ml)", ProteinPerUnit = 8, FatPerUnit = 0, CarbPerUnit = 12, CaloriesPerUnit = 80 },
            };

            if (isDiabeticLowProtein)
            {
                list.Add(new FoodExchangeItem { Category = "米食類", Icon = "🍚", UnitName = "份 (1/4碗飯)", ProteinPerUnit = 1.5, FatPerUnit = 0, CarbPerUnit = 15, CaloriesPerUnit = 70 });
                list.Add(new FoodExchangeItem { Category = "麵食類", Icon = "🍜", UnitName = "份 (1/2碗麵)", ProteinPerUnit = 2.5, FatPerUnit = 0, CarbPerUnit = 15, CaloriesPerUnit = 70 });
            }
            else
            {
                list.Add(new FoodExchangeItem { Category = "穀物雜糧類", Icon = "🍚", UnitName = "份 (1/4碗飯或半碗麵)", ProteinPerUnit = 2, FatPerUnit = 0, CarbPerUnit = 15, CaloriesPerUnit = 70 });
            }

            list.AddRange(new[]
            {
                new FoodExchangeItem { Category = "豆魚蛋肉類(低脂)", Icon = "🐟", UnitName = "份 (1兩瘦肉/蛋)", ProteinPerUnit = 7, FatPerUnit = 3, CarbPerUnit = 0, CaloriesPerUnit = 55 },
                new FoodExchangeItem { Category = "豆魚蛋肉類(中脂)", Icon = "🍗", UnitName = "份 (1兩肉/中脂豆腐)", ProteinPerUnit = 7, FatPerUnit = 5, CarbPerUnit = 0, CaloriesPerUnit = 75 },
                new FoodExchangeItem { Category = "豆魚蛋肉類(高脂)", Icon = "🥩", UnitName = "份 (五花/香腸/肉鬆)", ProteinPerUnit = 7, FatPerUnit = 9, CarbPerUnit = 0, CaloriesPerUnit = 120 },
                new FoodExchangeItem { Category = "蔬菜類", Icon = "🥦", UnitName = "份 (100g/半碗煮熟)", ProteinPerUnit = 1, FatPerUnit = 0, CarbPerUnit = 5, CaloriesPerUnit = 25 },
                new FoodExchangeItem { Category = "水果類", Icon = "🍎", UnitName = "份 (拳頭大/1碗切塊)", ProteinPerUnit = 0, FatPerUnit = 0, CarbPerUnit = 15, CaloriesPerUnit = 60 },
                new FoodExchangeItem { Category = "油脂類", Icon = "🫒", UnitName = "份 (1茶匙 5g)", ProteinPerUnit = 0, FatPerUnit = 5, CarbPerUnit = 0, CaloriesPerUnit = 45 },
                new FoodExchangeItem { Category = "堅果種子類", Icon = "🥜", UnitName = "份 (1湯匙約10g)", ProteinPerUnit = 0, FatPerUnit = 5, CarbPerUnit = 0, CaloriesPerUnit = 45 }
            });

            return list;
        }

        // 讀取所有個案
        public List<ClientProfile> GetAllClients()
        {
            var list = new List<ClientProfile>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM Clients ORDER BY UpdatedAt DESC, Id DESC;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ClientProfile
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    ChartNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Gender = reader.IsDBNull(3) ? "男" : reader.GetString(3),
                    Age = reader.GetDouble(4),
                    Height = reader.GetDouble(5),
                    Weight = reader.GetDouble(6),
                    Creatinine = reader.GetDouble(7),
                    ActivityFactor = reader.GetDouble(8),
                    StressFactor = reader.GetDouble(9),
                    KneeHeight = reader.GetDouble(10),
                    UlnaLength = reader.GetDouble(11),
                    HipCircumference = reader.GetDouble(12),
                    ArmCircumference = reader.GetDouble(13),
                    TargetCalories = reader.GetDouble(14),
                    CarbRatio = reader.GetDouble(15),
                    ProteinRatio = reader.GetDouble(16),
                    FatRatio = reader.GetDouble(17),
                    Notes = reader.IsDBNull(18) ? "" : reader.GetString(18),
                    CreatedAt = DateTime.TryParse(reader.IsDBNull(19) ? "" : reader.GetString(19), out var cd) ? cd : DateTime.Now,
                    UpdatedAt = DateTime.TryParse(reader.IsDBNull(20) ? "" : reader.GetString(20), out var ud) ? ud : DateTime.Now
                });
            }
            return list;
        }

        // 儲存或更新個案
        public int SaveClient(ClientProfile client)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            if (client.Id == 0)
            {
                string sql = @"
                    INSERT INTO Clients (
                        Name, ChartNumber, Gender, Age, Height, Weight, Creatinine,
                        ActivityFactor, StressFactor, KneeHeight, UlnaLength,
                        HipCircumference, ArmCircumference, TargetCalories,
                        CarbRatio, ProteinRatio, FatRatio, Notes, CreatedAt, UpdatedAt
                    ) VALUES (
                        @Name, @ChartNumber, @Gender, @Age, @Height, @Weight, @Creatinine,
                        @ActivityFactor, @StressFactor, @KneeHeight, @UlnaLength,
                        @HipCircumference, @ArmCircumference, @TargetCalories,
                        @CarbRatio, @ProteinRatio, @FatRatio, @Notes, @CreatedAt, @UpdatedAt
                    );
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", client.Name);
                cmd.Parameters.AddWithValue("@ChartNumber", client.ChartNumber);
                cmd.Parameters.AddWithValue("@Gender", client.Gender);
                cmd.Parameters.AddWithValue("@Age", client.Age);
                cmd.Parameters.AddWithValue("@Height", client.Height);
                cmd.Parameters.AddWithValue("@Weight", client.Weight);
                cmd.Parameters.AddWithValue("@Creatinine", client.Creatinine);
                cmd.Parameters.AddWithValue("@ActivityFactor", client.ActivityFactor);
                cmd.Parameters.AddWithValue("@StressFactor", client.StressFactor);
                cmd.Parameters.AddWithValue("@KneeHeight", client.KneeHeight);
                cmd.Parameters.AddWithValue("@UlnaLength", client.UlnaLength);
                cmd.Parameters.AddWithValue("@HipCircumference", client.HipCircumference);
                cmd.Parameters.AddWithValue("@ArmCircumference", client.ArmCircumference);
                cmd.Parameters.AddWithValue("@TargetCalories", client.TargetCalories);
                cmd.Parameters.AddWithValue("@CarbRatio", client.CarbRatio);
                cmd.Parameters.AddWithValue("@ProteinRatio", client.ProteinRatio);
                cmd.Parameters.AddWithValue("@FatRatio", client.FatRatio);
                cmd.Parameters.AddWithValue("@Notes", client.Notes);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("s"));
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("s"));

                client.Id = Convert.ToInt32(cmd.ExecuteScalar());
                return client.Id;
            }
            else
            {
                string sql = @"
                    UPDATE Clients SET
                        Name = @Name, ChartNumber = @ChartNumber, Gender = @Gender,
                        Age = @Age, Height = @Height, Weight = @Weight, Creatinine = @Creatinine,
                        ActivityFactor = @ActivityFactor, StressFactor = @StressFactor,
                        KneeHeight = @KneeHeight, UlnaLength = @UlnaLength,
                        HipCircumference = @HipCircumference, ArmCircumference = @ArmCircumference,
                        TargetCalories = @TargetCalories, CarbRatio = @CarbRatio,
                        ProteinRatio = @ProteinRatio, FatRatio = @FatRatio, Notes = @Notes,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id;";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", client.Id);
                cmd.Parameters.AddWithValue("@Name", client.Name);
                cmd.Parameters.AddWithValue("@ChartNumber", client.ChartNumber);
                cmd.Parameters.AddWithValue("@Gender", client.Gender);
                cmd.Parameters.AddWithValue("@Age", client.Age);
                cmd.Parameters.AddWithValue("@Height", client.Height);
                cmd.Parameters.AddWithValue("@Weight", client.Weight);
                cmd.Parameters.AddWithValue("@Creatinine", client.Creatinine);
                cmd.Parameters.AddWithValue("@ActivityFactor", client.ActivityFactor);
                cmd.Parameters.AddWithValue("@StressFactor", client.StressFactor);
                cmd.Parameters.AddWithValue("@KneeHeight", client.KneeHeight);
                cmd.Parameters.AddWithValue("@UlnaLength", client.UlnaLength);
                cmd.Parameters.AddWithValue("@HipCircumference", client.HipCircumference);
                cmd.Parameters.AddWithValue("@ArmCircumference", client.ArmCircumference);
                cmd.Parameters.AddWithValue("@TargetCalories", client.TargetCalories);
                cmd.Parameters.AddWithValue("@CarbRatio", client.CarbRatio);
                cmd.Parameters.AddWithValue("@ProteinRatio", client.ProteinRatio);
                cmd.Parameters.AddWithValue("@FatRatio", client.FatRatio);
                cmd.Parameters.AddWithValue("@Notes", client.Notes);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("s"));
                cmd.ExecuteNonQuery();
                return client.Id;
            }
        }

        public void DeleteClient(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM Clients WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // 讀取所有商業配方
        public List<CommercialFormula> GetAllFormulas()
        {
            var list = new List<CommercialFormula>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT Id, Name, Spec, Protein, Fat, Carbs, Calories, Notes FROM CommercialFormulas ORDER BY Id ASC;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CommercialFormula
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Spec = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Protein = reader.GetDouble(3),
                    Fat = reader.GetDouble(4),
                    Carbs = reader.GetDouble(5),
                    Calories = reader.GetDouble(6),
                    Notes = reader.IsDBNull(7) ? "" : reader.GetString(7)
                });
            }
            return list;
        }

        // 儲存或更新商業配方
        public void SaveFormula(CommercialFormula formula)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            if (formula.Id == 0)
            {
                string sql = @"
                    INSERT INTO CommercialFormulas (Name, Spec, Protein, Fat, Carbs, Calories, Notes)
                    VALUES (@Name, @Spec, @Protein, @Fat, @Carbs, @Calories, @Notes);
                    SELECT last_insert_rowid();";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", formula.Name);
                cmd.Parameters.AddWithValue("@Spec", formula.Spec);
                cmd.Parameters.AddWithValue("@Protein", formula.Protein);
                cmd.Parameters.AddWithValue("@Fat", formula.Fat);
                cmd.Parameters.AddWithValue("@Carbs", formula.Carbs);
                cmd.Parameters.AddWithValue("@Calories", formula.Calories);
                cmd.Parameters.AddWithValue("@Notes", formula.Notes);
                formula.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            else
            {
                string sql = @"
                    UPDATE CommercialFormulas SET
                        Name = @Name, Spec = @Spec, Protein = @Protein, Fat = @Fat,
                        Carbs = @Carbs, Calories = @Calories, Notes = @Notes
                    WHERE Id = @Id;";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", formula.Id);
                cmd.Parameters.AddWithValue("@Name", formula.Name);
                cmd.Parameters.AddWithValue("@Spec", formula.Spec);
                cmd.Parameters.AddWithValue("@Protein", formula.Protein);
                cmd.Parameters.AddWithValue("@Fat", formula.Fat);
                cmd.Parameters.AddWithValue("@Carbs", formula.Carbs);
                cmd.Parameters.AddWithValue("@Calories", formula.Calories);
                cmd.Parameters.AddWithValue("@Notes", formula.Notes);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteFormula(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM CommercialFormulas WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        // 儲存菜單設計計劃
        public int SaveDietPlan(DietPlanRecord plan)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"
                INSERT INTO DietPlans (ClientId, ClientName, Title, PlanDate, TargetCalories, ExchangesJson, FormulasJson, SummaryNotes)
                VALUES (@ClientId, @ClientName, @Title, @PlanDate, @TargetCalories, @ExchangesJson, @FormulasJson, @SummaryNotes);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", plan.ClientId);
            cmd.Parameters.AddWithValue("@ClientName", plan.ClientName);
            cmd.Parameters.AddWithValue("@Title", plan.Title);
            cmd.Parameters.AddWithValue("@PlanDate", plan.PlanDate.ToString("yyyy-MM-dd HH:mm"));
            cmd.Parameters.AddWithValue("@TargetCalories", plan.TargetCalories);
            cmd.Parameters.AddWithValue("@ExchangesJson", plan.ExchangesJson);
            cmd.Parameters.AddWithValue("@FormulasJson", plan.FormulasJson);
            cmd.Parameters.AddWithValue("@SummaryNotes", plan.SummaryNotes);

            plan.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return plan.Id;
        }

        // 取得個案的所有歷史菜單
        public List<DietPlanRecord> GetDietPlansByClientId(int clientId)
        {
            var list = new List<DietPlanRecord>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM DietPlans WHERE ClientId = @ClientId ORDER BY Id DESC;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ClientId", clientId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new DietPlanRecord
                {
                    Id = reader.GetInt32(0),
                    ClientId = reader.GetInt32(1),
                    ClientName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Title = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    PlanDate = DateTime.TryParse(reader.IsDBNull(4) ? "" : reader.GetString(4), out var dt) ? dt : DateTime.Now,
                    TargetCalories = reader.GetDouble(5),
                    ExchangesJson = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    FormulasJson = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    SummaryNotes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }
            return list;
        }

        // 取得所有歷史菜單
        public List<DietPlanRecord> GetAllDietPlans()
        {
            var list = new List<DietPlanRecord>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = "SELECT * FROM DietPlans ORDER BY Id DESC;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new DietPlanRecord
                {
                    Id = reader.GetInt32(0),
                    ClientId = reader.GetInt32(1),
                    ClientName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Title = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    PlanDate = DateTime.TryParse(reader.IsDBNull(4) ? "" : reader.GetString(4), out var dt) ? dt : DateTime.Now,
                    TargetCalories = reader.GetDouble(5),
                    ExchangesJson = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    FormulasJson = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    SummaryNotes = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }
            return list;
        }

        public void DeleteDietPlan(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM DietPlans WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
