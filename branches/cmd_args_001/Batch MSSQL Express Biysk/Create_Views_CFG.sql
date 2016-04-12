USE [techsite_cfg-2.X.X]
GO

/****** Object:  View [dbo].[v_ALL_PARAM_IN_SOTIASSO]    Script Date: 11/28/2014 10:07:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[v_ALL_PARAM_IN_SOTIASSO] AS
SELECT [ID],[NAME],[DESCRIPTION] FROM [10.100.104.248].[HISTORY].[dbo].[REALS_RV]


GO

USE [techsite_cfg-2.X.X]
GO

/****** Object:  View [dbo].[v_Versions_ID_TG_ASKUE_BiTEC]    Script Date: 11/28/2014 10:07:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[v_Versions_ID_TG_ASKUE_BiTEC] AS
SELECT DISTINCT [LAST_UPDATE], 
(ROW_NUMBER() OVER(ORDER BY [LAST_UPDATE] DESC) - 1) AS [NUMBER_VERSION] 
FROM [techsite_cfg-2.X.X].[dbo].[ID_TG_ASKUE_BiTEC] 
GROUP BY [LAST_UPDATE]


GO



