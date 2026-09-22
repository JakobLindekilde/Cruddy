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
	[Salary] [money] NOT NULL,
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
	[IdInt] [int] IDENTITY(1,1) NOT NULL,
	[IdGuid] [uniqueidentifier] NOT NULL,
	[Int32NoNull] [int] NOT NULL,
	[Int32Nullable] [int] NULL,
	[Int64NoNull] [bigint] NOT NULL,
	[Int64Nullable] [bigint] NULL,
	[Int16NoNull] [smallint] NOT NULL,
	[Int16Nullable] [smallint] NULL,
	[ByteNoNull] [tinyint] NOT NULL,
	[ByteNullable] [tinyint] NULL,
	[DecimalNoNull] [decimal](18, 4) NOT NULL,
	[DecimalNummalble] [decimal](18, 4) NULL,
	[DoubleNoNull] [float] NOT NULL,
	[DoubleNullable] [float] NULL,
	[SingleNoNull] [real] NOT NULL,
	[SingleNullable] [real] NULL,
	[BoolNoNull] [bit] NOT NULL,
	[BoolNullable] [bit] NULL,
	[StringNoNull] [nvarchar](50) NOT NULL,
	[StringNullable] [nvarchar](50) NULL,
	[DateTimeNoNull] [datetime] NOT NULL,
	[DateTimeNullable] [datetime] NULL,
	[TimeSpanNoNull] [time](7) NOT NULL,
	[TimeSpanNullable] [time](7) NULL,
	[DateTimeOffsetNoNull] [datetimeoffset](7) NOT NULL,
	[DateTimeOffsetNullable] [datetimeoffset](7) NULL,
	[GuidNoNull] [uniqueidentifier] NOT NULL,
	[GuidNullable] [uniqueidentifier] NULL,
 CONSTRAINT [PK_TestAllTypes] PRIMARY KEY CLUSTERED 
(
	[IdInt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET IDENTITY_INSERT [dbo].[Addresses] ON 
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (4, 1, N'1', N'1', 1, N'1', 1)
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
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (14, 1, N'Helsi', N'sadfasd', 5556666, N'555', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (15, 13, N'sdfef', N'sdf', 3, N'3', 1)
GO
SET IDENTITY_INSERT [dbo].[Addresses] OFF
GO
SET IDENTITY_INSERT [dbo].[Customers] ON 
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (1, N'Ole', N'Ole@valby.dk', 1, 1111111366, CAST(N'1995-10-10T00:00:00.000' AS DateTime), 12345.5600)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (2, N'Ove', N'Ove@valby.dk', 1, 11131111, CAST(N'1992-11-29T00:00:00.000' AS DateTime), 12345.5600)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (4, N'Kurt', N'krt@valby.dk', 1, 11151111, CAST(N'2000-02-10T00:00:00.000' AS DateTime), 12345.9199)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (5, N'Mia', N'mia@valby.dk', 0, 11171111, CAST(N'1975-10-20T00:00:00.000' AS DateTime), 12345.9999)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (6, N'Pia', N'pia@valby.dk', 0, 11161111, CAST(N'1983-04-04T00:00:00.000' AS DateTime), 12345.4999)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (13, N'Jjjjjjj', N'jj@mail.dk', 1, 333333333, CAST(N'1991-11-11T00:00:00.000' AS DateTime), 12345.5600)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (22, N'12', N'12', 1, 12, CAST(N'2012-11-11T00:00:00.000' AS DateTime), 12.0000)
GO
SET IDENTITY_INSERT [dbo].[Customers] OFF
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
INSERT [dbo].[TestAllTypes] ([IdInt], [IdGuid], [Int32NoNull], [Int32Nullable], [Int64NoNull], [Int64Nullable], [Int16NoNull], [Int16Nullable], [ByteNoNull], [ByteNullable], [DecimalNoNull], [DecimalNummalble], [DoubleNoNull], [DoubleNullable], [SingleNoNull], [SingleNullable], [BoolNoNull], [BoolNullable], [StringNoNull], [StringNullable], [DateTimeNoNull], [DateTimeNullable], [TimeSpanNoNull], [TimeSpanNullable], [DateTimeOffsetNoNull], [DateTimeOffsetNullable], [GuidNoNull], [GuidNullable]) VALUES (2, N'514a1c66-d201-4b2f-95fe-7725c97edbc5', 1, NULL, 2, NULL, 3, NULL, 4, NULL, CAST(5.5000 AS Decimal(18, 4)), NULL, 6.6, NULL, 7.7, NULL, 0, NULL, N'Abc', NULL, CAST(N'2026-01-01T11:12:13.457' AS DateTime), NULL, CAST(N'11:12:13' AS Time), NULL, CAST(N'2026-09-21T11:12:13.4567890+02:00' AS DateTimeOffset), NULL, N'00000000-0000-0000-0000-000000000000', NULL)
GO
INSERT [dbo].[TestAllTypes] ([IdInt], [IdGuid], [Int32NoNull], [Int32Nullable], [Int64NoNull], [Int64Nullable], [Int16NoNull], [Int16Nullable], [ByteNoNull], [ByteNullable], [DecimalNoNull], [DecimalNummalble], [DoubleNoNull], [DoubleNullable], [SingleNoNull], [SingleNullable], [BoolNoNull], [BoolNullable], [StringNoNull], [StringNullable], [DateTimeNoNull], [DateTimeNullable], [TimeSpanNoNull], [TimeSpanNullable], [DateTimeOffsetNoNull], [DateTimeOffsetNullable], [GuidNoNull], [GuidNullable]) VALUES (4, N'514a1c66-d201-4b2f-95fe-7725c97edbc6', 11, NULL, 22, NULL, 33, NULL, 44, NULL, CAST(5.1234 AS Decimal(18, 4)), NULL, 6.1234, NULL, 7.1234, NULL, 1, NULL, N'ÆØÅ æøå', NULL, CAST(N'9999-12-31T00:00:00.000' AS DateTime), NULL, CAST(N'00:00:00' AS Time), NULL, CAST(N'2016-01-01T00:00:00.0000000+01:00' AS DateTimeOffset), NULL, N'11111111-1111-1111-1111-111111111111', NULL)
GO
SET IDENTITY_INSERT [dbo].[TestAllTypes] OFF
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TestAllTypes]') AND name = N'IX_TestAllTypes')
ALTER TABLE [dbo].[TestAllTypes] ADD  CONSTRAINT [IX_TestAllTypes] UNIQUE NONCLUSTERED 
(
	[IdGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_TestAllTypes_IdGuid]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[TestAllTypes] ADD  CONSTRAINT [DF_TestAllTypes_IdGuid]  DEFAULT (newid()) FOR [IdGuid]
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
