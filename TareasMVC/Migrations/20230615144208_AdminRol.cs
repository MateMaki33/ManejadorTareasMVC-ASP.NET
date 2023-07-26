using Microsoft.EntityFrameworkCore.Migrations;

//Esta migracion sirve para crear un rol por defecto en BD cuando la actualicemos o creemos

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (Select Id FROM AspNetRoles Where Id = 'e8754231-c465-43a2-8724-e34d19aa329f')
                BEGIN
                Insert AspNetRoles (Id, [Name], [NormalizedName]) 
                Values ('e8754231-c465-43a2-8724-e34d19aa329f', 'admin', 'ADMIN' )
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE AspNetRoles Where Id = 'e8754231-c465-43a2-8724-e34d19aa329f'");
        }
    }
}
