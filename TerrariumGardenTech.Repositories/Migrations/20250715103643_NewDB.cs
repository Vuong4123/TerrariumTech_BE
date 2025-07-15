using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class NewDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategory",
                columns: table => new
                {
                    blogCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BlogCate__60848B8FC9999236", x => x.blogCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    categoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__23CAF1D819300390", x => x.categoryId);
                });

            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    EnvironmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvironmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnvironmentDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.EnvironmentId);
                });

            migrationBuilder.CreateTable(
                name: "Promotion",
                columns: table => new
                {
                    promotionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    startDate = table.Column<DateTime>(type: "date", nullable: true),
                    endDate = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__99EB696ECA1EAF5F", x => x.promotionId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false),
                    roleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__CD98462A2156B115", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "Shapes",
                columns: table => new
                {
                    ShapeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShapeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShapeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShapeSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShapeHeight = table.Column<int>(type: "int", nullable: false),
                    ShapeWidth = table.Column<int>(type: "int", nullable: false),
                    ShapeLength = table.Column<int>(type: "int", nullable: false),
                    ShapeVolume = table.Column<float>(type: "real", nullable: false),
                    ShapeMaterial = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shapes", x => x.ShapeId);
                });

            migrationBuilder.CreateTable(
                name: "TankMethods",
                columns: table => new
                {
                    TankMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TankMethodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TankMethodDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankMethods", x => x.TankMethodId);
                });

            migrationBuilder.CreateTable(
                name: "Terrariums",
                columns: table => new
                {
                    TerrariumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerrariumName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    bodyHTML = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terrariums", x => x.TerrariumId);
                });

            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    voucherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    discountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    discountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    validFrom = table.Column<DateTime>(type: "date", nullable: true),
                    validTo = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Voucher__F53389E9F60B1DB0", x => x.voucherId);
                });

            migrationBuilder.CreateTable(
                name: "Accessory",
                columns: table => new
                {
                    accessoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    stockQuantity = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Accessor__77E65FD722D6ED59", x => x.accessoryId);
                    table.ForeignKey(
                        name: "FK_Accessory_Category",
                        column: x => x.categoryId,
                        principalTable: "Category",
                        principalColumn: "categoryId");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    dateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    roleId = table.Column<int>(type: "int", nullable: true),
                    token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    startToken = table.Column<DateTime>(type: "datetime2", nullable: true),
                    endToken = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "active"),
                    FullName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Otp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OtpExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    refreshToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    refreshTokenExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__CB9A1CFFA89ED9C8", x => x.userId);
                    table.ForeignKey(
                        name: "FK_User_Role",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "roleId");
                });

            migrationBuilder.CreateTable(
                name: "TerrariumEnvironments",
                columns: table => new
                {
                    TerrariumEnvironmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerrariumId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumEnvironments", x => x.TerrariumEnvironmentId);
                    table.ForeignKey(
                        name: "FK_TerrariumEnvironments_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "EnvironmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumEnvironments_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumImage",
                columns: table => new
                {
                    terrariumImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    terrariumId = table.Column<int>(type: "int", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    altText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    isPrimary = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Terrariu__38A555784432CBBE", x => x.terrariumImageId);
                    table.ForeignKey(
                        name: "FK_TerrariumImage_Terrarium",
                        column: x => x.terrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId");
                });

            migrationBuilder.CreateTable(
                name: "TerrariumShapes",
                columns: table => new
                {
                    TerrariumShapeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerrariumId = table.Column<int>(type: "int", nullable: false),
                    ShapeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumShapes", x => x.TerrariumShapeId);
                    table.ForeignKey(
                        name: "FK_TerrariumShapes_Shapes_ShapeId",
                        column: x => x.ShapeId,
                        principalTable: "Shapes",
                        principalColumn: "ShapeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumShapes_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumTankMethods",
                columns: table => new
                {
                    TerrariumTankMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerrariumId = table.Column<int>(type: "int", nullable: false),
                    TankMethodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumTankMethods", x => x.TerrariumTankMethodId);
                    table.ForeignKey(
                        name: "FK_TerrariumTankMethods_TankMethods_TankMethodId",
                        column: x => x.TankMethodId,
                        principalTable: "TankMethods",
                        principalColumn: "TankMethodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumTankMethods_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumVariant",
                columns: table => new
                {
                    terrariumVariantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    terrariumId = table.Column<int>(type: "int", nullable: false),
                    variantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    additionalPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    stockQuantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Terrariu__B9E43448C71265AD", x => x.terrariumVariantId);
                    table.ForeignKey(
                        name: "FK_TerrariumVariant_Terrarium",
                        column: x => x.terrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId");
                });

            migrationBuilder.CreateTable(
                name: "AccessoryImage",
                columns: table => new
                {
                    accessoryImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    accessoryId = table.Column<int>(type: "int", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    altText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    isPrimary = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Accessor__FC8A6368CD989764", x => x.accessoryImageId);
                    table.ForeignKey(
                        name: "FK_AccessoryImage_Accessory",
                        column: x => x.accessoryId,
                        principalTable: "Accessory",
                        principalColumn: "accessoryId");
                });

            migrationBuilder.CreateTable(
                name: "AccessoryShapes",
                columns: table => new
                {
                    AccessoryShapeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessoryId = table.Column<int>(type: "int", nullable: false),
                    ShapeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoryShapes", x => x.AccessoryShapeId);
                    table.ForeignKey(
                        name: "FK_AccessoryShapes_Accessory_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessory",
                        principalColumn: "accessoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessoryShapes_Shapes_ShapeId",
                        column: x => x.ShapeId,
                        principalTable: "Shapes",
                        principalColumn: "ShapeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumAccessory",
                columns: table => new
                {
                    TerrariumAccessoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TerrariumId = table.Column<int>(type: "int", nullable: false),
                    AccessoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumAccessory", x => x.TerrariumAccessoryId);
                    table.ForeignKey(
                        name: "FK_TerrariumAccessory_Accessory_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessory",
                        principalColumn: "accessoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumAccessory_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tagName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: false),
                    receiverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverPhone = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.id);
                    table.ForeignKey(
                        name: "FK_Address_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "AISuggestLayout",
                columns: table => new
                {
                    layoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    layoutData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AISugges__023A37EFFA219D01", x => x.layoutId);
                    table.ForeignKey(
                        name: "FK_AISuggestLayout_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    blogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    blogCategoryId = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "draft"),
                    bodyHTML = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Blog__FA0AA72DCF7A4E3E", x => x.blogId);
                    table.ForeignKey(
                        name: "FK_Blog_BlogCategory",
                        column: x => x.blogCategoryId,
                        principalTable: "BlogCategory",
                        principalColumn: "blogCategoryId");
                    table.ForeignKey(
                        name: "FK_Blog_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    membershipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    membershipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    startDate = table.Column<DateTime>(type: "date", nullable: true),
                    endDate = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Membersh__86AA3B174168331C", x => x.membershipId);
                    table.ForeignKey(
                        name: "FK_Membership_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__4BA5CEA92CE3E468", x => x.notificationId);
                    table.ForeignKey(
                        name: "FK_Notification_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    orderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    voucherId = table.Column<int>(type: "int", nullable: true),
                    totalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    deposit = table.Column<decimal>(type: "decimal(12,2)", nullable: true, defaultValue: 0.00m),
                    orderDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending"),
                    paymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending"),
                    shippingStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__0809335D04D87A58", x => x.orderId);
                    table.ForeignKey(
                        name: "FK_Order_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                    table.ForeignKey(
                        name: "FK_Order_Voucher",
                        column: x => x.voucherId,
                        principalTable: "Voucher",
                        principalColumn: "voucherId");
                });

            migrationBuilder.CreateTable(
                name: "Personalizes",
                columns: table => new
                {
                    PersonalizeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TankMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personalizes", x => x.PersonalizeId);
                    table.ForeignKey(
                        name: "FK_Personalizes_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LayoutTerrarium",
                columns: table => new
                {
                    layoutTerrariumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    terrariumVariantId = table.Column<int>(type: "int", nullable: false),
                    layoutData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LayoutTe__ED2AF5EA0034035F", x => x.layoutTerrariumId);
                    table.ForeignKey(
                        name: "FK_LayoutTerrarium_TerrariumVariant",
                        column: x => x.terrariumVariantId,
                        principalTable: "TerrariumVariant",
                        principalColumn: "terrariumVariantId");
                });

            migrationBuilder.CreateTable(
                name: "PromotionTerrariumVariant",
                columns: table => new
                {
                    promotionTerrariumVariantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    promotionId = table.Column<int>(type: "int", nullable: false),
                    terrariumVariantId = table.Column<int>(type: "int", nullable: false),
                    discountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    startDate = table.Column<DateTime>(type: "date", nullable: true),
                    endDate = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__8F0EFF32523D8B83", x => x.promotionTerrariumVariantId);
                    table.ForeignKey(
                        name: "FK_PromotionTerrariumVariant_Promotion",
                        column: x => x.promotionId,
                        principalTable: "Promotion",
                        principalColumn: "promotionId");
                    table.ForeignKey(
                        name: "FK_PromotionTerrariumVariant_TerrariumVariant",
                        column: x => x.terrariumVariantId,
                        principalTable: "TerrariumVariant",
                        principalColumn: "terrariumVariantId");
                });

            migrationBuilder.CreateTable(
                name: "AddressDelivery",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    receiverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverPhone = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: false),
                    createdOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDelivery", x => x.id);
                    table.ForeignKey(
                        name: "FK_AddressDelivery_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    orderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    accessoryId = table.Column<int>(type: "int", nullable: true),
                    terrariumVariantId = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    unitPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    totalPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__3724BD5293DBCE99", x => x.orderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItem_Accessory",
                        column: x => x.accessoryId,
                        principalTable: "Accessory",
                        principalColumn: "accessoryId");
                    table.ForeignKey(
                        name: "FK_OrderItem_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                    table.ForeignKey(
                        name: "FK_OrderItem_TerrariumVariant",
                        column: x => x.terrariumVariantId,
                        principalTable: "TerrariumVariant",
                        principalColumn: "terrariumVariantId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransition",
                columns: table => new
                {
                    paymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    paymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    paymentAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    paymentDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentT__A0D9EFC6B78D0C95", x => x.paymentId);
                    table.ForeignKey(
                        name: "FK_PaymentTransition_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "ReturnExchangeRequest",
                columns: table => new
                {
                    requestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    requestDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending"),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReturnEx__E3C5DE3141AB3D28", x => x.requestId);
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequest_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequest_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "ShippingDetail",
                columns: table => new
                {
                    shippingDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    shippingMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    shippingCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    estimatedDeliveryDate = table.Column<DateTime>(type: "date", nullable: true),
                    trackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shipping__DDF63975E50C620D", x => x.shippingDetailId);
                    table.ForeignKey(
                        name: "FK_ShippingDetail_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    feedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderItemId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__2613FD244B84BCD6", x => x.feedbackId);
                    table.ForeignKey(
                        name: "FK_Feedback_OrderItem",
                        column: x => x.orderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "orderItemId");
                    table.ForeignKey(
                        name: "FK_Feedback_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "OrderItemDetail",
                columns: table => new
                {
                    orderItemDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderItemId = table.Column<int>(type: "int", nullable: false),
                    detailKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    detailValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__098BB1314F361970", x => x.orderItemDetailId);
                    table.ForeignKey(
                        name: "FK_OrderItemDetail_OrderItem",
                        column: x => x.orderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "orderItemId");
                });

            migrationBuilder.CreateTable(
                name: "ReturnExchangeRequestItem",
                columns: table => new
                {
                    requestItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    requestId = table.Column<int>(type: "int", nullable: false),
                    orderItemId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReturnEx__FDD6A58FF74806E9", x => x.requestItemId);
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequestItem_OrderItem",
                        column: x => x.orderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "orderItemId");
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequestItem_Request",
                        column: x => x.requestId,
                        principalTable: "ReturnExchangeRequest",
                        principalColumn: "requestId");
                });

            migrationBuilder.CreateTable(
                name: "FeedbackImage",
                columns: table => new
                {
                    feedbackImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedbackId = table.Column<int>(type: "int", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    altText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__5C1F8F3456C39EF3", x => x.feedbackImageId);
                    table.ForeignKey(
                        name: "FK_FeedbackImage_Feedback",
                        column: x => x.feedbackId,
                        principalTable: "Feedback",
                        principalColumn: "feedbackId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accessory_categoryId",
                table: "Accessory",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryImage_accessoryId",
                table: "AccessoryImage",
                column: "accessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryShapes_AccessoryId",
                table: "AccessoryShapes",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessoryShapes_ShapeId",
                table: "AccessoryShapes",
                column: "ShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_userId",
                table: "Address",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressDelivery_orderId",
                table: "AddressDelivery",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_AISuggestLayout_userId",
                table: "AISuggestLayout",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_blogCategoryId",
                table: "Blog",
                column: "blogCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_userId",
                table: "Blog",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_orderItemId",
                table: "Feedback",
                column: "orderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_userId",
                table: "Feedback",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackImage_feedbackId",
                table: "FeedbackImage",
                column: "feedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutTerrarium_terrariumVariantId",
                table: "LayoutTerrarium",
                column: "terrariumVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Membership_userId",
                table: "Membership",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_userId",
                table: "Notification",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_userId",
                table: "Order",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_voucherId",
                table: "Order",
                column: "voucherId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_accessoryId",
                table: "OrderItem",
                column: "accessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_orderId",
                table: "OrderItem",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_terrariumVariantId",
                table: "OrderItem",
                column: "terrariumVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemDetail_orderItemId",
                table: "OrderItemDetail",
                column: "orderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransition_orderId",
                table: "PaymentTransition",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Personalizes_UserId",
                table: "Personalizes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTerrariumVariant_promotionId",
                table: "PromotionTerrariumVariant",
                column: "promotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTerrariumVariant_terrariumVariantId",
                table: "PromotionTerrariumVariant",
                column: "terrariumVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequest_orderId",
                table: "ReturnExchangeRequest",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequest_userId",
                table: "ReturnExchangeRequest",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequestItem_orderItemId",
                table: "ReturnExchangeRequestItem",
                column: "orderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequestItem_requestId",
                table: "ReturnExchangeRequestItem",
                column: "requestId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingDetail_orderId",
                table: "ShippingDetail",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumAccessory_AccessoryId",
                table: "TerrariumAccessory",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumAccessory_TerrariumId",
                table: "TerrariumAccessory",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumEnvironments_EnvironmentId",
                table: "TerrariumEnvironments",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumEnvironments_TerrariumId",
                table: "TerrariumEnvironments",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumImage_terrariumId",
                table: "TerrariumImage",
                column: "terrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumShapes_ShapeId",
                table: "TerrariumShapes",
                column: "ShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumShapes_TerrariumId",
                table: "TerrariumShapes",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumTankMethods_TankMethodId",
                table: "TerrariumTankMethods",
                column: "TankMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumTankMethods_TerrariumId",
                table: "TerrariumTankMethods",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumVariant_terrariumId",
                table: "TerrariumVariant",
                column: "terrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_User_roleId",
                table: "User",
                column: "roleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessoryImage");

            migrationBuilder.DropTable(
                name: "AccessoryShapes");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "AddressDelivery");

            migrationBuilder.DropTable(
                name: "AISuggestLayout");

            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "FeedbackImage");

            migrationBuilder.DropTable(
                name: "LayoutTerrarium");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "OrderItemDetail");

            migrationBuilder.DropTable(
                name: "PaymentTransition");

            migrationBuilder.DropTable(
                name: "Personalizes");

            migrationBuilder.DropTable(
                name: "PromotionTerrariumVariant");

            migrationBuilder.DropTable(
                name: "ReturnExchangeRequestItem");

            migrationBuilder.DropTable(
                name: "ShippingDetail");

            migrationBuilder.DropTable(
                name: "TerrariumAccessory");

            migrationBuilder.DropTable(
                name: "TerrariumEnvironments");

            migrationBuilder.DropTable(
                name: "TerrariumImage");

            migrationBuilder.DropTable(
                name: "TerrariumShapes");

            migrationBuilder.DropTable(
                name: "TerrariumTankMethods");

            migrationBuilder.DropTable(
                name: "BlogCategory");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Promotion");

            migrationBuilder.DropTable(
                name: "ReturnExchangeRequest");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropTable(
                name: "Shapes");

            migrationBuilder.DropTable(
                name: "TankMethods");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Accessory");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "TerrariumVariant");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Voucher");

            migrationBuilder.DropTable(
                name: "Terrariums");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
