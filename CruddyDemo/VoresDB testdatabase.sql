USE [VoresDB]
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
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (3, N'Kim', N'Kim@valby.dk', 0, 234234, CAST(N'1985-10-03T00:00:00.000' AS DateTime), 12345.5600)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (4, N'Kurt', N'krt@valby.dk', 1, 11151111, CAST(N'2000-02-10T00:00:00.000' AS DateTime), 12345.9199)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (5, N'Mia', N'mia@valby.dk', 0, 11171111, CAST(N'1975-10-20T00:00:00.000' AS DateTime), 12345.9999)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (6, N'Pia', N'pia@valby.dk', 0, 11161111, CAST(N'1983-04-04T00:00:00.000' AS DateTime), 12345.4999)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (13, N'Jjjjjjj', N'jj@mail.dk', 1, 333333333, CAST(N'1991-11-11T00:00:00.000' AS DateTime), 12345.5600)
GO
INSERT [dbo].[Customers] ([Id], [Name], [Email], [Vip], [Phone], [Birthdate], [Salary]) VALUES (14, N'Kurt', N'Kurt', 1, 44, CAST(N'1975-08-18T00:00:00.000' AS DateTime), 12345.5600)
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
