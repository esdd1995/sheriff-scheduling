using Microsoft.EntityFrameworkCore.Migrations;

namespace SS.Db.Migrations
{
    public partial class AddGroupAndExemptTrainingPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permission",
                columns: new[] { "Id", "CreatedById", "Description", "Name", "UpdatedById", "UpdatedOn" },
                values: new object[,]
                {
                    { 45, null, "View all Groups", "ViewGroups", null, null },
                    { 46, null, "Create and Assign Groups", "CreateAndAssignGroups", null, null },
                    { 47, null, "Edit Groups", "EditGroups", null, null },
                    { 48, null, "Expire Groups", "ExpireGroups", null, null },
                    { 49, null, "Exempt from training", "ExemptFromTraining", null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: 49);
        }
    }
}
