using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseAdapters.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SensorId",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceAddress = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SensorId",
                table: "Measurements",
                column: "SensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_Sensors_SensorId",
                table: "Measurements",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_Sensors_SensorId",
                table: "Measurements");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_SensorId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "Measurements");
        }
    }
}
