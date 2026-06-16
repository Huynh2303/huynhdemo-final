IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] varchar(100) NOT NULL,
    [ParentId] int NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Categori__3214EC077F70D310] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_categories_parent] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id])
);
GO

CREATE TABLE [Contacts] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(max) NULL,
    [email] nvarchar(max) NULL,
    [phone] nvarchar(max) NULL,
    [DateSend] datetime2 NULL,
    [Message] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([id])
);
GO

CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(450) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [EmailConfirmedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [FailedLoginCount] int NOT NULL,
    [LockoutUntil] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] varchar(255) NOT NULL,
    [Description] varchar(max) NULL,
    [Brand] varchar(100) NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Products__3214EC07816071E2] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_products_category] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id])
);
GO

CREATE TABLE [Addresses] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [AddressLine] varchar(255) NOT NULL,
    [City] varchar(100) NOT NULL,
    [Country] varchar(100) NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Addresse__3214EC0758FDAC5E] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_addresses_user] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Carts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    [Status] varchar(20) NOT NULL DEFAULT 'active',
    CONSTRAINT [PK__Carts__3214EC073F6079E0] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_carts_user] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [TotalAmount] decimal(12,2) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT 'pending',
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Orders__3214EC079A4B06C9] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_orders_user] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [RoleUser] (
    [RolesId] int NOT NULL,
    [UsersId] int NOT NULL,
    CONSTRAINT [PK_RoleUser] PRIMARY KEY ([RolesId], [UsersId]),
    CONSTRAINT [FK_RoleUser_Roles_RolesId] FOREIGN KEY ([RolesId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RoleUser_Users_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [userTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Token] nvarchar(450) NOT NULL,
    [Type] int NOT NULL,
    [ExpiredAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [UsedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_userTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_userTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProductVariants] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Size] varchar(50) NULL,
    [Color] varchar(50) NULL,
    [Price] decimal(12,2) NOT NULL,
    [Stock] int NOT NULL,
    CONSTRAINT [PK__ProductV__3214EC078FDE3CF6] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_variants_product] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FraudAnalysis] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [RiskScore] decimal(4,3) NOT NULL,
    [ModelName] varchar(100) NOT NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__FraudAna__3214EC07D1803ABB] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_fraudanalysis_order] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrderLogs] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [Status] varchar(20) NOT NULL,
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__OrderLog__3214EC071A7FFF54] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_orderlogs_order] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [PaymentMethod] varchar(50) NOT NULL,
    [Amount] decimal(12,2) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT 'pending',
    [CreatedAt] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Payments__3214EC07AE94A3CB] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_payments_order] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CartItems] (
    [Id] int NOT NULL IDENTITY,
    [CartId] int NOT NULL,
    [VariantId] int NOT NULL,
    [Quantity] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__CartItem__3214EC0736AE55CA] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_cartitems_cart] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [fk_cartitems_variant] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariants] ([Id])
);
GO

CREATE TABLE [OrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [VariantId] int NOT NULL,
    [Price] decimal(12,2) NOT NULL,
    [Quantity] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK__OrderIte__3214EC0725749632] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_orderitems_order] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [fk_orderitems_variant] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariants] ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Code], [CreatedAt], [Description], [Name])
VALUES (1, N'USER', '0001-01-01T00:00:00.0000000', NULL, N'Người dùng'),
(2, N'ADMIN', '0001-01-01T00:00:00.0000000', NULL, N'Quản trị'),
(3, N'STAFF', '0001-01-01T00:00:00.0000000', NULL, N'Nhân viên');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

CREATE INDEX [idx_addresses_userid] ON [Addresses] ([UserId]);
GO

CREATE INDEX [idx_cartitems_cartid] ON [CartItems] ([CartId]);
GO

CREATE INDEX [idx_cartitems_variantid] ON [CartItems] ([VariantId]);
GO

CREATE UNIQUE INDEX [uq_cart_variant] ON [CartItems] ([CartId], [VariantId]);
GO

CREATE INDEX [idx_carts_userid] ON [Carts] ([UserId]);
GO

CREATE INDEX [idx_categories_parentid] ON [Categories] ([ParentId]);
GO

CREATE INDEX [idx_fraudanalysis_orderid] ON [FraudAnalysis] ([OrderId]);
GO

CREATE INDEX [idx_orderitems_orderid] ON [OrderItems] ([OrderId]);
GO

CREATE INDEX [idx_orderitems_variantid] ON [OrderItems] ([VariantId]);
GO

CREATE INDEX [idx_orderlogs_orderid] ON [OrderLogs] ([OrderId]);
GO

CREATE INDEX [idx_orders_userid] ON [Orders] ([UserId]);
GO

CREATE INDEX [idx_payments_orderid] ON [Payments] ([OrderId]);
GO

CREATE INDEX [idx_products_categoryid] ON [Products] ([CategoryId]);
GO

CREATE INDEX [idx_variants_productid] ON [ProductVariants] ([ProductId]);
GO

CREATE UNIQUE INDEX [IX_Roles_Code] ON [Roles] ([Code]);
GO

CREATE INDEX [IX_RoleUser_UsersId] ON [RoleUser] ([UsersId]);
GO

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

CREATE UNIQUE INDEX [IX_userTokens_Token] ON [userTokens] ([Token]);
GO

CREATE INDEX [IX_userTokens_UserId] ON [userTokens] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260321085657_update_new', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260324095301_update_crud_product', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ProductImages] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Url] varchar(500) NOT NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_productimages_product] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProductVariantImages] (
    [Id] int NOT NULL IDENTITY,
    [VariantId] int NOT NULL,
    [Url] varchar(500) NOT NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_ProductVariantImages] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_variantimages_variant] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserImages] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Url] varchar(500) NOT NULL,
    CONSTRAINT [PK_UserImages] PRIMARY KEY ([Id]),
    CONSTRAINT [fk_userimages_user] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [idx_productimages_productid] ON [ProductImages] ([ProductId]);
GO

CREATE INDEX [idx_variantimages_variantid] ON [ProductVariantImages] ([VariantId]);
GO

CREATE UNIQUE INDEX [uq_userimages_userid] ON [UserImages] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260325084443_image', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Categories] ALTER COLUMN [Name] varchar(100) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406183305_init', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Addresses] ADD [PhoneNumber] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Addresses] ADD [RecipientName] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260412201638_add_address', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Payments] DROP CONSTRAINT [fk_payments_order];
GO

ALTER TABLE [Payments] DROP CONSTRAINT [PK__Payments__3214EC07AE94A3CB];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'PaymentMethod');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Payments] DROP COLUMN [PaymentMethod];
GO

EXEC sp_rename N'[Payments].[idx_payments_orderid]', N'IX_Payments_OrderId', N'INDEX';
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'Status');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Payments] ALTER COLUMN [Status] nvarchar(max) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'CreatedAt');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Payments] ALTER COLUMN [CreatedAt] datetime2 NOT NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'Amount');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Payments] ALTER COLUMN [Amount] decimal(18,2) NOT NULL;
GO

ALTER TABLE [Payments] ADD [Method] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Orders] ADD [PaymentMethod] int NOT NULL DEFAULT 0;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderLogs]') AND [c].[name] = N'Status');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [OrderLogs] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [OrderLogs] ALTER COLUMN [Status] varchar(20) NULL;
GO

ALTER TABLE [OrderLogs] ADD [ActionBy] nvarchar(max) NULL;
GO

ALTER TABLE [OrderLogs] ADD [AdditionalInfo] nvarchar(max) NULL;
GO

ALTER TABLE [OrderLogs] ADD [ChangeType] nvarchar(max) NULL;
GO

ALTER TABLE [OrderLogs] ADD [PreviousStatus] nvarchar(max) NULL;
GO

ALTER TABLE [OrderLogs] ADD [Reason] nvarchar(max) NULL;
GO

ALTER TABLE [OrderLogs] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

ALTER TABLE [Payments] ADD CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]);
GO

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260414194646_requestlog', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Orders] ALTER COLUMN [Status] int NOT NULL;
ALTER TABLE [Orders] ADD DEFAULT 0 FOR [Status];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260503095912_oder', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Products] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260509071615_AddIsDeletedToProduct', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [FraudAnalysis] DROP CONSTRAINT [fk_fraudanalysis_order];
GO

DROP INDEX [idx_fraudanalysis_orderid] ON [FraudAnalysis];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FraudAnalysis]') AND [c].[name] = N'RiskScore');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [FraudAnalysis] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [FraudAnalysis] ALTER COLUMN [RiskScore] decimal(5,2) NOT NULL;
GO

ALTER TABLE [FraudAnalysis] ADD [InputSnapshot] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [FraudAnalysis] ADD [RiskLevel] varchar(20) NOT NULL DEFAULT '';
GO

ALTER TABLE [FraudAnalysis] ADD [RiskReasons] nvarchar(max) NOT NULL DEFAULT N'';
GO

CREATE UNIQUE INDEX [IX_FraudAnalysis_OrderId] ON [FraudAnalysis] ([OrderId]);
GO

ALTER TABLE [FraudAnalysis] ADD CONSTRAINT [fk_fraudanalysis_order] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260513084445_FraudAnalysisUpdateNew', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [DateOfBirth] datetime2 NULL;
GO

ALTER TABLE [Users] ADD [IsVip] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [LastBirthdayEmailYear] int NULL;
GO

ALTER TABLE [Products] ADD [SellerId] int NULL;
GO

CREATE INDEX [IX_Products_SellerId] ON [Products] ([SellerId]);
GO

ALTER TABLE [Products] ADD CONSTRAINT [fk_products_seller] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260524121334_AddSellerIdToProduct', N'8.0.23');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariants]') AND [c].[name] = N'Size');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ProductVariants] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [ProductVariants] ALTER COLUMN [Size] nvarchar(50) NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariants]') AND [c].[name] = N'Price');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ProductVariants] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [ProductVariants] ALTER COLUMN [Price] decimal(18,2) NOT NULL;
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductVariants]') AND [c].[name] = N'Color');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ProductVariants] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ProductVariants] ALTER COLUMN [Color] nvarchar(50) NULL;
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(255) NOT NULL;
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Description');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Products] ALTER COLUMN [Description] nvarchar(max) NULL;
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Brand');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Products] ALTER COLUMN [Brand] nvarchar(100) NULL;
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'TotalAmount');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Orders] ALTER COLUMN [TotalAmount] decimal(18,2) NOT NULL;
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderLogs]') AND [c].[name] = N'Status');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [OrderLogs] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [OrderLogs] ALTER COLUMN [Status] nvarchar(20) NULL;
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderItems]') AND [c].[name] = N'Price');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [OrderItems] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [OrderItems] ALTER COLUMN [Price] decimal(18,2) NOT NULL;
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FraudAnalysis]') AND [c].[name] = N'RiskScore');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [FraudAnalysis] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [FraudAnalysis] ALTER COLUMN [RiskScore] decimal(18,2) NOT NULL;
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FraudAnalysis]') AND [c].[name] = N'RiskLevel');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [FraudAnalysis] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [FraudAnalysis] ALTER COLUMN [RiskLevel] nvarchar(20) NOT NULL;
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FraudAnalysis]') AND [c].[name] = N'ModelName');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [FraudAnalysis] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [FraudAnalysis] ALTER COLUMN [ModelName] nvarchar(100) NOT NULL;
GO

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Name');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [Categories] ALTER COLUMN [Name] nvarchar(100) NULL;
GO

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Carts]') AND [c].[name] = N'Status');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Carts] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [Carts] ALTER COLUMN [Status] nvarchar(20) NOT NULL;
ALTER TABLE [Carts] ADD DEFAULT N'active' FOR [Status];
GO

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'Country');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [Addresses] ALTER COLUMN [Country] nvarchar(100) NOT NULL;
GO

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'City');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [Addresses] ALTER COLUMN [City] nvarchar(100) NOT NULL;
GO

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'AddressLine');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [Addresses] ALTER COLUMN [AddressLine] nvarchar(255) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260525164644_FixUnicodeAndDecimal', N'8.0.23');
GO

COMMIT;
GO

