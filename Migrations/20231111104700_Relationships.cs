using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAPI.Migrations
{
    /// <inheritdoc />
    public partial class Relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieTheatersMovies_MoviesTheaters_MovieTheaterId",
                table: "MovieTheatersMovies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MoviesTheaters",
                table: "MoviesTheaters");

            migrationBuilder.RenameTable(
                name: "MoviesTheaters",
                newName: "MovieTheaters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovieTheaters",
                table: "MovieTheaters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieTheatersMovies_MovieTheaters_MovieTheaterId",
                table: "MovieTheatersMovies",
                column: "MovieTheaterId",
                principalTable: "MovieTheaters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieTheatersMovies_MovieTheaters_MovieTheaterId",
                table: "MovieTheatersMovies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MovieTheaters",
                table: "MovieTheaters");

            migrationBuilder.RenameTable(
                name: "MovieTheaters",
                newName: "MoviesTheaters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoviesTheaters",
                table: "MoviesTheaters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieTheatersMovies_MoviesTheaters_MovieTheaterId",
                table: "MovieTheatersMovies",
                column: "MovieTheaterId",
                principalTable: "MoviesTheaters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
