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
CREATE TABLE [Payments] (
    [PaymentId] uniqueidentifier NOT NULL,
    [JogoId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Currency] VARCHAR(3) NOT NULL,
    [StatusPayment] VARCHAR(20) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId])
);

CREATE TABLE [PaymentEvents] (
    [Id] uniqueidentifier NOT NULL,
    [PaymentId] uniqueidentifier NOT NULL,
    [EventType] VARCHAR(100) NOT NULL,
    [PayLoad] NVARCHAR(MAX) NOT NULL,
    [Version] INT NOT NULL,
    [EventDate] DATETIME NOT NULL,
    CONSTRAINT [PK_PaymentEvents] PRIMARY KEY ([Id]),
 );

CREATE TABLE [PaymentItens] (
    [PaymentItemId] uniqueidentifier NOT NULL,
    [PaymentId] uniqueidentifier NOT NULL,
    [ItemId] uniqueidentifier NOT NULL,
    [Description] nvarchar(255) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_PaymentItens] PRIMARY KEY ([PaymentItemId]),
    CONSTRAINT [FK_PaymentItens_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([PaymentId]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [UX_PaymentEvents_PaymentId_Version] ON [PaymentEvents] ([PaymentId], [Version]);

CREATE INDEX [IX_PaymentItems_ItemId] ON [PaymentItens] ([ItemId]);

CREATE INDEX [IX_PaymentItems_PaymentId] ON [PaymentItens] ([PaymentId]);

CREATE INDEX [IX_PAYMENTS_JOGOID] ON [Payments] ([JogoId]);

CREATE INDEX [IX_PAYMENTS_STATUSPAYMENT] ON [Payments] ([StatusPayment]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917022918_PaymentsTables', N'9.0.9');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'CreatedAt');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Payments] ALTER COLUMN [CreatedAt] DATETIME2 NOT NULL;
ALTER TABLE [Payments] ADD DEFAULT (GETDATE()) FOR [CreatedAt];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentEvents]') AND [c].[name] = N'EventDate');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PaymentEvents] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PaymentEvents] ALTER COLUMN [EventDate] DATETIME2 NOT NULL;
ALTER TABLE [PaymentEvents] ADD DEFAULT (GETDATE()) FOR [EventDate];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917030824_UpdatePaymentEvents', N'9.0.9');

DROP INDEX [IX_PAYMENTS_JOGOID] ON [Payments];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'JogoId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Payments] DROP COLUMN [JogoId];

EXEC sp_rename N'[PaymentItens].[ItemId]', N'JogoId', 'COLUMN';

EXEC sp_rename N'[PaymentItens].[IX_PaymentItems_ItemId]', N'IX_PaymentItems_JogoId', 'INDEX';

CREATE INDEX [IX_PAYMENTS_USERID] ON [Payments] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250919023231_PaymentsRefactored', N'9.0.9');

ALTER TABLE [Payments] ADD [OrderId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260103234122_AddOrderId', N'9.0.9');

COMMIT;
GO

