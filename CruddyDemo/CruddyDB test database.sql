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
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (4, 1, N'Bothel', N'Napa Ct.', 1970, N'98011', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (5, 1, N'Bothel', N'Mt. Dias Blv.', 9833, N'98011', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (7, 4, N'Dallas', N'Jumping St.', 9178, N'75201', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (8, 4, N'Navada', N'Riverside Drive', 2487, N'84407', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (16, 2, N'Cambridge', N'Via De Luna', 3997, N'02139', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (22, 5, N'Racine', N'S. Greenbay Rd.', 99433, N'53182', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (23, 6, N'Denby', N'W. Empire', 44025, N'57716', 2)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (24, 13, N'Hamburg', N'Am Karlshof', 8200, N'22001', 1)
GO
INSERT [dbo].[Addresses] ([Id], [CustomerId], [Town], [Street], [StreetNo], [ZipCode], [AddressType]) VALUES (25, 39, N'Paris', N'rue Lamarck', 3383, N'75007', 1)
GO
SET IDENTITY_INSERT [dbo].[Addresses] OFF
GO
SET IDENTITY_INSERT [dbo].[Customers] ON 
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (1, N'Ken Sánchez', N'kensanchez@gmail.com', 0, 1234567890, CAST(N'1966-10-11T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (2, N'Lee Duffy', N'Lee@tomtom.com', 1, 1122334455, CAST(N'1992-12-25T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (4, N'Dylan Miller', N'dylanmiller@gmail.com', 1, 88887777, CAST(N'2000-02-10T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (5, N'Diane Margheim', N'dianamargheim@valby.dk', 1, 111711125, CAST(N'1975-10-20T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (6, N'Gigi Matthew', N'gigi@gmail.com', 0, 111611117, CAST(N'1983-04-04T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (13, N'Kim Brown', N'kimbrown@mail.com', 1, 123123123, CAST(N'2001-09-23T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (39, N'Johnny Hansen', N'johnny@hansen.com', 0, 34567123, CAST(N'1993-09-25T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (64, N'Janice Galvin', N'jgalvin@gmail.com', 1, 1111122222, CAST(N'1999-10-05T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate]) VALUES (74, N'Liza Sullivan', N'lizasullivan@gmail.com', 1, 1213235234, CAST(N'1992-10-19T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Customers] OFF
GO
SET IDENTITY_INSERT [dbo].[DateTimeTypes] ON 
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (1, CAST(N'2011-11-11T11:11:00.000' AS DateTime), NULL, CAST(N'11:11:00' AS Time), NULL, CAST(N'11:11:00' AS Time), NULL)
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (2, CAST(N'2012-12-12T12:00:00.000' AS DateTime), NULL, CAST(N'12:00:00' AS Time), CAST(N'12:01:00' AS Time), CAST(N'12:02:00' AS Time), CAST(N'12:03:00' AS Time))
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (4, CAST(N'1800-01-01T00:00:00.000' AS DateTime), NULL, CAST(N'00:00:00' AS Time), NULL, CAST(N'00:00:00' AS Time), NULL)
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (5, CAST(N'1999-12-31T23:59:00.000' AS DateTime), CAST(N'1999-12-31T05:59:00.000' AS DateTime), CAST(N'23:59:00' AS Time), CAST(N'23:59:00' AS Time), CAST(N'23:59:00' AS Time), CAST(N'23:59:00' AS Time))
GO
INSERT [dbo].[DateTimeTypes] ([Id], [DateTimeNotNull], [DateTimeNullable], [TimeSpanNotNull], [TimeSpanNullable], [TimeOnlyNotNull], [TimeOnlyNullable]) VALUES (6, CAST(N'1753-01-01T12:00:00.000' AS DateTime), CAST(N'9999-12-31T23:59:00.000' AS DateTime), CAST(N'12:00:00' AS Time), CAST(N'12:00:00' AS Time), CAST(N'12:00:00' AS Time), CAST(N'12:00:00' AS Time))
GO
SET IDENTITY_INSERT [dbo].[DateTimeTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Employees] ON 
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (1, N'Mark', 40400.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (2, N'Terry', 41200.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (3, N'Mary', 52000.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (4, N'Sariya', 46700.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (5, N'Bo', 50000.0000)
GO
INSERT [dbo].[Employees] ([Id], [Name], [Salary]) VALUES (6, N'James', 40000.0000)
GO
SET IDENTITY_INSERT [dbo].[Employees] OFF
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'f0000000-0000-0000-0000-000000000000', N'Frog', N'Kiss it!', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-00ff-000000000000', N'Fish', N'Slimy thingy', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-000000000099', N'Bee', N'Will sting you!', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-000000000555', N'Snake', N'Will bite you!', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'00000000-0000-0000-0000-00000000ff44', N'Whale', N'Big swimming creature', 0)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'de484d78-32f2-4481-bf31-1df61a4d8a4e', N'Dolphin', N'Says thanks for all the fish', 1)
GO
INSERT [dbo].[KeyColumnIsGuid] ([Id], [Name], [Description], [Active]) VALUES (N'ab1769f4-6f7f-48a8-a2f1-68cfa459945c', N'Dog', N'Will bite you!', 1)
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (7, 4, 5, 4, 10, 1, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (8, 1, 1, 2, 1, 2, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (9, 1, 5, 4, 2, 2, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (10, 5, 3, 5, 3, 0, CAST(N'2026-08-21T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (11, 1, 3, 2, 2, 1, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (12, 6, 6, 5, 1, 1, CAST(N'2026-08-21T09:52:22.520' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (14, 39, 3, 4, 12, 1, CAST(N'2026-08-30T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[Orders] ([Id], [CId], [PId], [EId], [Qty], [Paid], [OrderDate]) VALUES (15, 74, 5, 5, 5, 1, CAST(N'2026-10-19T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (1, N'TV', 2000)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (2, N'PC', 3500)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (3, N'Laptop', 2200)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (4, N'Mobile', 900)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (5, N'Table', 123)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (6, N'Chair', 100)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (9, N'Bed', 234)
GO
INSERT [dbo].[Products] ([Id], [Name], [Price]) VALUES (10, N'Bed', 667)
GO
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
SET IDENTITY_INSERT [dbo].[TestAllTypes] ON 
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (2, 2, NULL, 2, NULL, 2, NULL, 2, NULL, CAST(2.0000 AS Decimal(18, 4)), NULL, 2, NULL, 2, NULL, 0, NULL, N'2', N'', N'00000000-0000-0000-0000-000000000002', NULL)
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (4, 11, NULL, 11, NULL, 11, NULL, 11, NULL, CAST(11.1234 AS Decimal(18, 4)), NULL, 11.1234, NULL, 11.1234, NULL, 1, NULL, N'11', N'', N'11111111-1111-1111-1111-111111111111', NULL)
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (5, 12, 12, 12, 12, 12, 12, 12, 12, CAST(12.4999 AS Decimal(18, 4)), CAST(12.4999 AS Decimal(18, 4)), 12.4999, 12.4999, 12.4999, 12.4999, 0, NULL, N'12,4999', N'12,4999', N'00000000-0000-0000-0000-000000000012', N'00000000-0000-0000-0000-000000000012')
GO
INSERT [dbo].[TestAllTypes] ([Id], [Int32NotNull], [Int32Nullable], [Int64NotNull], [Int64Nullable], [Int16NotNull], [Int16Nullable], [ByteNotNull], [ByteNullable], [DecimalNotNull], [DecimalNullable], [DoubleNotNull], [DoubleNullable], [SingleNotNull], [SingleNullable], [BoolNotNull], [BoolNullable], [StringNotNull], [StringNullable], [GuidNotNull], [GuidNullable]) VALUES (6, 9, 9, 9, 9, 9, 9, 9, 9, CAST(9.9999 AS Decimal(18, 4)), CAST(9.9999 AS Decimal(18, 4)), 9.9999, 9.9999, 9.9999, 9.9999, 1, NULL, N'9,9999', N'9,9999', N'00000000-0000-0000-0000-000000000009', NULL)
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
