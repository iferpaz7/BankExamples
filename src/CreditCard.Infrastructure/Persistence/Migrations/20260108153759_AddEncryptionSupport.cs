using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditCards_CardNumber",
                table: "CreditCards");

            migrationBuilder.AddColumn<string>(
                name: "CardNumberHash",
                table: "CreditCards",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_CardNumberHash",
                table: "CreditCards",
                column: "CardNumberHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditCards_CardNumberHash",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "CardNumberHash",
                table: "CreditCards");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_CardNumber",
                table: "CreditCards",
                column: "CardNumber",
                unique: true);
        }
    }
}
