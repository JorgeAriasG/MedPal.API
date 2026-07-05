using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionModuleMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Supplements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Supplements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Supplements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Form",
                table: "Supplements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Indication",
                table: "Supplements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrescribedAt",
                table: "Supplements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescribedById",
                table: "Supplements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timing",
                table: "Supplements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Adherence",
                table: "NutritionProgresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DietPlanId",
                table: "NutritionProgresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "NutritionProgresses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Photos",
                table: "NutritionProgresses",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SkeletalMuscleMass",
                table: "NutritionProgresses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaistCircumference",
                table: "NutritionProgresses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergens",
                table: "FoodItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemItem",
                table: "FoodItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "FiberG",
                table: "DietPlans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objective",
                table: "DietPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterMl",
                table: "DietPlans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BodyFatMass",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EcwTbwRatio",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtracellularWater",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InbodyResultId",
                table: "BodyCompositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IntracellularWater",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Minerals",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PhaseAngle",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SegmentalLeanLeftArm",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SegmentalLeanLeftLeg",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SegmentalLeanRightArm",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SegmentalLeanRightLeg",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SegmentalLeanTrunk",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "BodyCompositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBodyWater",
                table: "BodyCompositions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BodyFatPercentageEstimated",
                table: "AnthropometryRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MidArmCircumference",
                table: "AnthropometryRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaistHeightRatio",
                table: "AnthropometryRecords",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Form",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Indication",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "PrescribedAt",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "PrescribedById",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Timing",
                table: "Supplements");

            migrationBuilder.DropColumn(
                name: "Adherence",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "DietPlanId",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "Photos",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "SkeletalMuscleMass",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "WaistCircumference",
                table: "NutritionProgresses");

            migrationBuilder.DropColumn(
                name: "Allergens",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "IsSystemItem",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "FiberG",
                table: "DietPlans");

            migrationBuilder.DropColumn(
                name: "Objective",
                table: "DietPlans");

            migrationBuilder.DropColumn(
                name: "WaterMl",
                table: "DietPlans");

            migrationBuilder.DropColumn(
                name: "BodyFatMass",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "EcwTbwRatio",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "ExtracellularWater",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "InbodyResultId",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "IntracellularWater",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "Minerals",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "PhaseAngle",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "SegmentalLeanLeftArm",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "SegmentalLeanLeftLeg",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "SegmentalLeanRightArm",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "SegmentalLeanRightLeg",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "SegmentalLeanTrunk",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "TotalBodyWater",
                table: "BodyCompositions");

            migrationBuilder.DropColumn(
                name: "BodyFatPercentageEstimated",
                table: "AnthropometryRecords");

            migrationBuilder.DropColumn(
                name: "MidArmCircumference",
                table: "AnthropometryRecords");

            migrationBuilder.DropColumn(
                name: "WaistHeightRatio",
                table: "AnthropometryRecords");
        }
    }
}
