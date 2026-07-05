using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnthropometryRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Waist = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Hip = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    WaistHipRatio = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    Neck = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ShoulderBreadth = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Chest = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Arm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Forearm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Wrist = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Thigh = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Calf = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    TricepsSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    BicepsSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    SubscapularSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    SuprailiacSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    CalfSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ThighSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    AbdominalSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    PectoralSkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    AxillarySkinfold = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnthropometryRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnthropometryRecords_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BodyCompositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BodyFatPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MuscleMass = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    BoneMass = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    BodyWaterPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    VisceralFat = table.Column<int>(type: "int", nullable: true),
                    Bmr = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    MetabolicAge = table.Column<int>(type: "int", nullable: true),
                    ProteinMass = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    WaistHipRatio = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    BwImported = table.Column<bool>(type: "bit", nullable: false),
                    InBodyRawData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyCompositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodyCompositions_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DietPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DailyCalories = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ProteinG = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    CarbsG = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    FatG = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietPlans_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DietPlans_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subcategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServingSize = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ServingUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calories = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Protein = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Carbs = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fiber = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Sodium = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Sugar = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SaturatedFat = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TransFat = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Cholesterol = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Potassium = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    VitaminA = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    VitaminC = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Calcium = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Iron = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NutritionProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    BodyFatPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MuscleMass = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Waist = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    CaloriesConsumed = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ProteinConsumed = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    CarbsConsumed = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    FatConsumed = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    WaterGlasses = table.Column<int>(type: "int", nullable: true),
                    ExerciseMinutes = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionProgresses_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Supplements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplements_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DietPlanMeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DietPlanId = table.Column<int>(type: "int", nullable: false),
                    MealOrder = table.Column<int>(type: "int", nullable: false),
                    MealName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TimeOfDay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietPlanMeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietPlanMeals_DietPlans_DietPlanId",
                        column: x => x.DietPlanId,
                        principalTable: "DietPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DietPlanMealItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DietPlanMealId = table.Column<int>(type: "int", nullable: false),
                    FoodItemId = table.Column<int>(type: "int", nullable: true),
                    CustomFoodName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Calories = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Protein = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Carbs = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Fat = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietPlanMealItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietPlanMealItems_DietPlanMeals_DietPlanMealId",
                        column: x => x.DietPlanMealId,
                        principalTable: "DietPlanMeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DietPlanMealItems_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnthropometryRecords_PatientDetailsId_RecordedAt",
                table: "AnthropometryRecords",
                columns: new[] { "PatientDetailsId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BodyCompositions_PatientDetailsId_RecordedAt",
                table: "BodyCompositions",
                columns: new[] { "PatientDetailsId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DietPlanMealItems_DietPlanMealId",
                table: "DietPlanMealItems",
                column: "DietPlanMealId");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlanMealItems_FoodItemId",
                table: "DietPlanMealItems",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlanMeals_DietPlanId_MealOrder",
                table: "DietPlanMeals",
                columns: new[] { "DietPlanId", "MealOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_CreatedByUserId",
                table: "DietPlans",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_PatientDetailsId_Status",
                table: "DietPlans",
                columns: new[] { "PatientDetailsId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Category",
                table: "FoodItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Name",
                table: "FoodItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionProgresses_PatientDetailsId_RecordedAt",
                table: "NutritionProgresses",
                columns: new[] { "PatientDetailsId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Supplements_PatientDetailsId_IsActive",
                table: "Supplements",
                columns: new[] { "PatientDetailsId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnthropometryRecords");

            migrationBuilder.DropTable(
                name: "BodyCompositions");

            migrationBuilder.DropTable(
                name: "DietPlanMealItems");

            migrationBuilder.DropTable(
                name: "NutritionProgresses");

            migrationBuilder.DropTable(
                name: "Supplements");

            migrationBuilder.DropTable(
                name: "DietPlanMeals");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "DietPlans");
        }
    }
}
