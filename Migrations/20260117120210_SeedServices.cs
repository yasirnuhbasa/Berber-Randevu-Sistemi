using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BerberRandevuSistemi.Migrations
{
    /// <inheritdoc />
    public partial class SeedServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.InsertData(
            //     table: "Services",
            //     columns: new[] { "Id", "Description", "DurationMinutes", "Price", "ServiceName" },
            //     values: new object[,]
            //     {
            //         { 1, "Klasik veya modern saç kesimi", 30, 200m, "Saç Kesimi" },
            //         { 2, "Sıcak havlu ve jiletli tıraş", 15, 100m, "Sakal Tıraşı" },
            //         { 3, "Komple saç ve sakal bakımı", 45, 280m, "Saç & Sakal" },
            //         { 4, "0-12 yaş özel kesim", 25, 150m, "Çocuk Tıraşı" },
            //         { 5, "Yıkama ve şekillendirme", 15, 80m, "Yıkama ve Fön" },
            //         { 6, "Siyah maske ve buhar", 20, 120m, "Cilt Bakımı" },
            //         { 7, "VIP özel gün paketi", 75, 750m, "Damat Tıraşı" }
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
