USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[AdminValuesOfID]    Script Date: 12/01/2014 16:18:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AdminValuesOfID](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DATE] [datetime] NOT NULL,
	[ID_COMPONENT] [int] NOT NULL,
	[REC] [float] NULL,
	[IS_PER] [int] NULL,
	[DIVIAT] [float] NULL,
	[SEASON] [int] NOT NULL,
	[FC] [tinyint] NOT NULL,
 CONSTRAINT [PK_AdminValuesOfID] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AdminValuesOfID] ADD  DEFAULT ((7)) FOR [SEASON]
GO

ALTER TABLE [dbo].[AdminValuesOfID] ADD  DEFAULT ((0)) FOR [FC]
GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[ALL_PARAM_ASKUE]    Script Date: 12/01/2014 16:19:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ALL_PARAM_ASKUE](
	[ID] [int] NOT NULL,
	[ID_TEC] [int] NOT NULL,
	[PARNUMBER] [int] NOT NULL,
	[DATA_DATE] [datetime] NOT NULL,
	[SEASON] [int] NOT NULL,
	[VALUE0] [bigint] NOT NULL,
 CONSTRAINT [PK_ALL_Param_Piramida2000_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[ID_TEC] ASC,
	[PARNUMBER] ASC,
	[DATA_DATE] ASC,
	[SEASON] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[ALL_PARAM_SOTIASSO]    Script Date: 12/01/2014 16:19:31 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ALL_PARAM_SOTIASSO](
	[ID] [numeric](15, 0) NOT NULL,
	[ID_TEC] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[last_changed_at] [datetime] NOT NULL,
	[tmdelta] [int] NOT NULL,
 CONSTRAINT [PK_ALL_states_real_his] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[last_changed_at] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[ALL_PARAM_SOTIASSO_0]    Script Date: 12/01/2014 16:19:39 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ALL_PARAM_SOTIASSO_0](
	[ID] [numeric](15, 0) NOT NULL,
	[ID_TEC] [int] NOT NULL,
	[Value] [float] NOT NULL,
	[last_changed_at] [datetime] NOT NULL,
	[tmdelta] [int] NOT NULL,
 CONSTRAINT [PK_ALL_states_real_his_0] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[last_changed_at] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[logging]    Script Date: 12/01/2014 16:19:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[logging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ID_LOGMSG] [int] NOT NULL,
	[ID_APP] [int] NOT NULL,
	[ID_USER] [int] NOT NULL,
	[DATETIME_WR] [datetime] NOT NULL,
	[MESSAGE] [nvarchar](max) NULL,
 CONSTRAINT [PK_logging] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[P_SUMM_TSN]    Script Date: 12/01/2014 16:20:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[P_SUMM_TSN](
	[ID_TEC] [int] NOT NULL,
	[SUM_P_SN] [float] NOT NULL,
	[LAST_UPDATE] [datetime] NULL
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[P_SUMM_TSN_ASKUE]    Script Date: 12/01/2014 16:20:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[P_SUMM_TSN_ASKUE](
	[ID_TEC] [int] NOT NULL,
	[SUM_P_SN] [float] NOT NULL,
	[LAST_UPDATE] [datetime] NULL
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[P_TSN_ASKUE]    Script Date: 12/01/2014 16:20:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[P_TSN_ASKUE](
	[ID_TEC] [int] NOT NULL,
	[ID_USPD] [int] NOT NULL,
	[ID_CHANNEL] [int] NOT NULL,
	[VALUE0] [float] NULL,
	[DATA_DATE] [datetime] NOT NULL,
 CONSTRAINT [PK_P_TSN_ASKUE] PRIMARY KEY CLUSTERED 
(
	[ID_USPD] ASC,
	[ID_CHANNEL] ASC,
	[DATA_DATE] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [techsite-2.X.X]
GO

/****** Object:  Table [dbo].[PPBRvsPBROfID]    Script Date: 12/01/2014 16:21:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PPBRvsPBROfID](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DATE_TIME] [datetime] NOT NULL,
	[WR_DATE_TIME] [datetime] NOT NULL,
	[PBR_NUMBER] [nvarchar](255) NULL,
	[ID_COMPONENT] [int] NOT NULL,
	[OWNER] [int] NOT NULL,
	[PBR] [float] NOT NULL,
	[Pmin] [float] NOT NULL,
	[Pmax] [float] NOT NULL,
 CONSTRAINT [PK_PPBR_PBROfID] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PPBRvsPBROfID] ADD  CONSTRAINT [DF_PPBR_PBROfID_WR_DATE_TIME]  DEFAULT (getdate()) FOR [WR_DATE_TIME]
GO

