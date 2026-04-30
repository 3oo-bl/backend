using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfitableViewInfra.Migrations
{
    /// <inheritdoc />
    public partial class PrefsWeightsDtoUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellerRating",
                table: "PrefsWeights",
                newName: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "PrefsWeights",
                newName: "SellerRating");
        }
    }
}
