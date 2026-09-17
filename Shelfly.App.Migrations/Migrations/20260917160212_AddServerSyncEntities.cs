using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfly.App.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddServerSyncEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookProfileSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileUsername = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookProfileSyncRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookProfileSyncRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookServerMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServerAssignedId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookServerMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookServerMappings_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookServerMappings_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedServerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileUsername = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProfileEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedServerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedServerEntries_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActiveServerEntryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SyncEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSuccessfulSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSyncResult = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncStates_SavedServerEntries_ActiveServerEntryId",
                        column: x => x.ActiveServerEntryId,
                        principalTable: "SavedServerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookProfileSyncRecords_BookId_ProfileUsername",
                table: "BookProfileSyncRecords",
                columns: new[] { "BookId", "ProfileUsername" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookProfileSyncRecords_ProfileUsername",
                table: "BookProfileSyncRecords",
                column: "ProfileUsername");

            migrationBuilder.CreateIndex(
                name: "IX_BookServerMappings_BookId_ServerId",
                table: "BookServerMappings",
                columns: new[] { "BookId", "ServerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookServerMappings_ServerAssignedId_ServerId",
                table: "BookServerMappings",
                columns: new[] { "ServerAssignedId", "ServerId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookServerMappings_ServerId",
                table: "BookServerMappings",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedServerEntries_IsActive",
                table: "SavedServerEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SavedServerEntries_ServerId",
                table: "SavedServerEntries",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Url",
                table: "Servers",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncStates_ActiveServerEntryId",
                table: "SyncStates",
                column: "ActiveServerEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookProfileSyncRecords");

            migrationBuilder.DropTable(
                name: "BookServerMappings");

            migrationBuilder.DropTable(
                name: "SyncStates");

            migrationBuilder.DropTable(
                name: "SavedServerEntries");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
