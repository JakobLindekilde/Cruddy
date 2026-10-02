USE [CruddyDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Addresses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Addresses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Town] [nvarchar](50) NOT NULL,
	[Street] [nvarchar](50) NOT NULL,
	[StreetNo] [int] NOT NULL,
	[ZipCode] [nvarchar](10) NOT NULL,
	[AddressType] [int] NOT NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[Vip] [bit] NOT NULL,
	[Phone] [int] NOT NULL,
	[Birthdate] [datetime] NOT NULL,
 CONSTRAINT [PK__Customer__3214EC07EA9BFF16] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DateTimeTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DateTimeTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTimeNotNull] [datetime] NOT NULL,
	[DateTimeNullable] [datetime] NULL,
	[TimeSpanNotNull] [time](7) NOT NULL,
	[TimeSpanNullable] [time](7) NULL,
	[TimeOnlyNotNull] [time](7) NOT NULL,
	[TimeOnlyNullable] [time](7) NULL,
 CONSTRAINT [PK_DateTimeType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Employees](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Salary] [money] NOT NULL,
 CONSTRAINT [PK__Employee__3214EC07F30F2950] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KeyColumnIsGuid]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[KeyColumnIsGuid](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_KeyColumnIsGuid] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CId] [int] NULL,
	[PId] [int] NULL,
	[EId] [int] NULL,
	[Qty] [int] NULL,
	[Paid] [int] NULL,
	[OrderDate] [datetime] NULL,
 CONSTRAINT [PK__Orders__3214EC07BD792216] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Price] [int] NOT NULL,
 CONSTRAINT [PK__Products__3214EC07CE96365F] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TestAllTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TestAllTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Int32NotNull] [int] NOT NULL,
	[Int32Nullable] [int] NULL,
	[Int64NotNull] [bigint] NOT NULL,
	[Int64Nullable] [bigint] NULL,
	[Int16NotNull] [smallint] NOT NULL,
	[Int16Nullable] [smallint] NULL,
	[ByteNotNull] [tinyint] NOT NULL,
	[ByteNullable] [tinyint] NULL,
	[DecimalNotNull] [decimal](18, 4) NOT NULL,
	[DecimalNullable] [decimal](18, 4) NULL,
	[DoubleNotNull] [float] NOT NULL,
	[DoubleNullable] [float] NULL,
	[SingleNotNull] [real] NOT NULL,
	[SingleNullable] [real] NULL,
	[BoolNotNull] [bit] NOT NULL,
	[BoolNullable] [bit] NULL,
	[StringNotNull] [nvarchar](50) NOT NULL,
	[StringNullable] [nvarchar](50) NULL,
	[GuidNotNull] [uniqueidentifier] NOT NULL,
	[GuidNullable] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TestAllTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET IDENTITY_INSERT [dbo].[Addresses] ON 
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (4, 1, N'1', N'1', 1, N'2', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (5, 1, N'27777', N'2', 2, N'2', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (6, 4, N'3', N'3', 3, N'3', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (7, 4, N'k', N'k', 4444, N'5555', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (8, 4, N'5555', N'5555', 5555, N'5555', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (9, 1, N'Herlev ', N'Hovedgaden', 123, N'3301', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (10, 4, N'Kbh', N'5555', 123, N'123', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (14, 1, N'Helsi', N'sadfasd', 5556666, N'555', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (15, 13, N'sdfef', N'sdf', 3, N'3', 2)
GO
SET IDENTITY_INSERT [dbo].[Addresses] OFF
GO
SET IDENTITY_INSERT [dbo].[Customers] ON 
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (1, N'OleOleo', N'Ole@val.dk', 1, 1111111366, CAST(N'1995-10-10T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (2, N'UUU', N'Ove@valby.dk', 1, 5, CAST(N'1992-12-29T12:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (4, N'Kurtd', N'krt@valby.dk', 1, 11151111, CAST(N'2000-02-10T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (5, N'Mia', N'mia@valby.dk', 1, 11171112, CAST(N'1975-10-20T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (6, N'looiu', N'pia@valby.dk', 0, 11161111, CAST(N'1983-04-04T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (13, N'asdasd', N'jj@mail.dk', 1, 1133, CAST(N'2026-09-23T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (38, N'Jakob Lindekilde', N'asd ', 1, 3234, CAST(N'2026-09-25T12:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (39, N'Jens Hansen', N'sdf', 0, 345, CAST(N'2026-09-25T12:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (46, N'Peter', N'sdf', 1, 3355, CAST(N'2026-09-27T12:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (48, N'IIIIIIIIII', N'sdfsdf', 1, 23, CAST(N'2026-09-28T12:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (49, N'1234', N'df', 0, 34, CAST(N'2026-09-28T12:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Customers] OFF
GO
SET IDENTITY_INSERT [dbo].[DateTimeTypes] ON 
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (1, CAST(N'2020-11-11T00:00:00.000' AS DateTime), NULL, CAST(N'18:18:00' AS Time), NULL, CAST(N'10:11:12' AS Time), NULL)
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (2, CAST(N'2026-09-28T12:00:00.000' AS DateTime), NULL, CAST(N'01:04:00' AS Time), NULL, CAST(N'00:00:00' AS Time), NULL)
GO
SET IDENTITY_INSERT [dbo].[DateTimeTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Employees] ON 
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (1, N'Jonas', 40400.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (2, N'Jones', 41200.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (3, N'Jens', 52000.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (4, N'Johan', 46700.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (5, N'Jani', 50000.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (6, N'Johnny', 40000.0000)
GO
SET IDENTITY_INSERT [dbo].[Employees] OFF
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'f0000000-0000-0000-0000-000000000000', N'frt', N'frt', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-00ff-000000000000', N'ff', N'ff', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-000000000034', N'zxc', N'zxcgg', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-000000000099', N'hhhhhggf', N'hhhhhgg', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-000000000555', N'Jakob3 gggg', N'qw', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'f0644cbf-be20-47a9-a4c8-0cfc948fba89', N'hhhhhhhhhhhhhhhhhhhhhhhhhhhhh', N'sdaf dfgdfgdfg', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'6f2f64bb-6f6c-489b-a211-1cf4eac80e5d', N'nyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy', N'dfg', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'de484d78-32f2-4481-bf31-1df61a4d8a4e', N'nnnnnnnnnnnnnnnnnnnn', N'dfg', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'387e650d-e418-4fa0-820c-31f8e799d796', N'dgfg', N'jj', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'08cae3c0-9d38-43e3-9e43-cac7ffb85779', N'qwert dd', N'dfdf', 1)
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (7, 4, 5, 2, 23, 1, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (8, 1, 1, 2, 1234567, 2, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (9, 1, 5, 4, 1, 2, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (10, 5, 3, 5, 66, 0, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (11, 1, 3, 2, 1, 1, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (12, 6, 6, 5, 1, 1, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (14, 6, 2, 5, 2, 1, CAST(N'2026-08-30T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (1, N'TV', 7654555)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (2, N'PC', 7254)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (3, N'Laptop', 8154)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (4, N'Mobile', 17654)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (5, N'Bord', 1654)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (6, N'Stol', 1354)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (9, N'Stol', 3)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (10, N'PC', 23)
GO
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
SET IDENTITY_INSERT [dbo].[TestAllTypes] ON 
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (2, 1, NULL, 2, NULL, 3, NULL, 4, NULL, CAST(223.5544 AS Decimal(18, 4)), NULL, 33, NULL, 0, NULL, 0, NULL, N'adasdf', N'', N'00000000-0000-0000-0000-000000000002', NULL)
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (4, 11, NULL, 22, NULL, 33, NULL, 44, NULL, CAST(5.1234 AS Decimal(18, 4)), NULL, 6.1234, NULL, 7.1234, NULL, 1, NULL, N'ÆØÅ æøå', N'fghgfh', N'11111111-1111-1111-1111-111111111111', NULL)
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (5, 12, NULL, 12, NULL, 12, NULL, 12, NULL, CAST(12.0000 AS Decimal(18, 4)), NULL, 12, NULL, 12, NULL, 0, NULL, N'12', NULL, N'00000000-0000-0000-0000-000000000000', NULL)
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (6, 9, NULL, 9, NULL, 9, NULL, 9, NULL, CAST(9.0000 AS Decimal(18, 4)), NULL, 9, NULL, 9, NULL, 1, NULL, N'9', NULL, N'00000000-0000-0000-0000-000000000009', NULL)
GO
SET IDENTITY_INSERT [dbo].[TestAllTypes] OFF
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_KeyColumnIsGuid_Id]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[KeyColumnIsGuid] ADD  CONSTRAINT [DF_KeyColumnIsGuid_Id]  DEFAULT (newid()) FOR [Id]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Orders__Qty__534D60F1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF__Orders__Qty__534D60F1]  DEFAULT ((1)) FOR [Qty]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Orders__Paid__5441852A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF__Orders__Paid__5441852A]  DEFAULT ((1)) FOR [Paid]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Orders__OrderDat__5535A963]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF__Orders__OrderDat__5535A963]  DEFAULT (getdate()) FOR [OrderDate]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Address_Customers]') AND parent_object_id = OBJECT_ID(N'[dbo].[Addresses]'))
ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Address_Customers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Address_Customers]') AND parent_object_id = OBJECT_ID(N'[dbo].[Addresses]'))
ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Address_Customers]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__CId__5070F446]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK__Orders__CId__5070F446] FOREIGN KEY([CId])
REFERENCES [dbo].[Customers] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__CId__5070F446]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK__Orders__CId__5070F446]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__EId__52593CB8]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK__Orders__EId__52593CB8] FOREIGN KEY([EId])
REFERENCES [dbo].[Employees] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__EId__52593CB8]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK__Orders__EId__52593CB8]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__PId__5165187F]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK__Orders__PId__5165187F] FOREIGN KEY([PId])
REFERENCES [dbo].[Products] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK__Orders__PId__5165187F]') AND parent_object_id = OBJECT_ID(N'[dbo].[Orders]'))
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK__Orders__PId__5165187F]
GO
