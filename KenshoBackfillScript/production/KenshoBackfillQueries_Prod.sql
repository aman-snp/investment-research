USE [Codex_IR_00]
GO

IF Object_Id('codemigrationscriptlog_tbl') IS NOT NULL  
BEGIN
	Drop table CodeMigrationScriptLog_tbl
END
Go

CREATE TABLE [dbo].[codemigrationscriptlog_tbl](
	[keycodemigrationscriptlog] [uniqueidentifier] NULL,
	[stepId] [int] NULL,
	[stepname] [varchar](500) NULL,
	[startdate] [datetime] NULL,
	[enddate] [datetime] NULL,
	[inserteddatetime] [datetime] NULL
) ON [PRIMARY]
GO

IF Object_Id('CodexWorkingSet_tbl') IS NOT NULL  
BEGIN
	Drop table [CodexWorkingSet_tbl]
END
Go

CREATE TABLE [dbo].[CodexWorkingSet_tbl](
	[newKeyFileCollectionDetail] [int] IDENTITY(570785657 ,1) NOT NULL, ---this has to change for production 
	[KeyCodexIntermediate] [uniqueidentifier] NOT NULL,
	[KeyFileVersion] [uniqueidentifier] NOT NULL,
	[KeyFileCollectionDetail] [int] NOT NULL,
	[pages] [int] NULL,
	CodexDocubotVersion nvarchar(60) null,
PRIMARY KEY CLUSTERED 
(
	[newKeyFileCollectionDetail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


IF Object_Id('CodexStepLastId_tbl') IS NOT NULL  
BEGIN
	Drop table CodexStepLastId_tbl
END
Go

Create TABLE CodexStepLastId_tbl 
(
	StepId int, 
	lastRecordId int
)
Go
Insert into CodexStepLastId_tbl values (2, 570785657)
Insert into CodexStepLastId_tbl values (3, 570785657)
Insert into CodexStepLastId_tbl values (4, 570785657)

IF Object_Id('FileCollectionDetailWO') IS NOT NULL  
BEGIN
	Drop table FileCollectionDetailWO
END
Go
CREATE TABLE [dbo].[FileCollectionDetailWO](
	[KeyFileCollectionDetail] [int] NOT NULL,
	[KeyFileCollection] [int] NOT NULL,
	[TranslationLevel] [tinyint] NULL,
	[FileTranslated]  AS (CONVERT([tinyint],case when [TranslationLevel] IS NOT NULL then (1) else (0) end)),
	[KeyFileSource] [smallint] NOT NULL,
	[KeyForeignLanguage] [smallint] NOT NULL,
	[MainFile] [tinyint] NOT NULL,
	[Pages] [int] NULL,
	[AccessionNumberSEDAR] [varchar](169) NULL,
	[FileDescription] [varchar](150) NULL,
	[TranslationPublishable] [tinyint] NULL,
	[RootXBRLInstanceFile] [bit] NULL,
	[MaskFileExclusion] [int] NULL,
	[FilePublishable]  AS (CONVERT([bit],case when [KeyFileSource]=(124) OR [KeyFileSource]=(47) OR [KeyFileSource]=(45) OR [KeyFileSource]=(7) OR [KeyFileSource]=(6) OR [TranslationPublishable]=(0) OR [MaskFileExclusion]>(0) OR ([KeyFileSourceType]=(558) OR [KeyFileSourceType]=(557) OR [KeyFileSourceType]=(556) OR [KeyFileSourceType]=(555) OR [KeyFileSourceType]=(554) OR [KeyFileSourceType]=(553) OR [KeyFileSourceType]=(552) OR [KeyFileSourceType]=(551) OR [KeyFileSourceType]=(550) OR [KeyFileSourceType]=(549) OR [KeyFileSourceType]=(548) OR [KeyFileSourceType]=(547) OR [KeyFileSourceType]=(546) OR [KeyFileSourceType]=(545) OR [KeyFileSourceType]=(544) OR [KeyFileSourceType]=(543) OR [KeyFileSourceType]=(542) OR [KeyFileSourceType]=(541) OR [KeyFileSourceType]=(540) OR [KeyFileSourceType]=(539) OR [KeyFileSourceType]=(538) OR [KeyFileSourceType]=(537) OR [KeyFileSourceType]=(536) OR [KeyFileSourceType]=(535) OR [KeyFileSourceType]=(534) OR [KeyFileSourceType]=(532) OR [KeyFileSourceType]=(531) OR [KeyFileSourceType]=(530) OR [KeyFileSourceType]=(529) OR [KeyFileSourceType]=(528) OR [KeyFileSourceType]=(527) OR [KeyFileSourceType]=(526) OR [KeyFileSourceType]=(525) OR [KeyFileSourceType]=(524) OR [KeyFileSourceType]=(523) OR [KeyFileSourceType]=(522) OR [KeyFileSourceType]=(521) OR [KeyFileSourceType]=(520) OR [KeyFileSourceType]=(519) OR [KeyFileSourceType]=(518) OR [KeyFileSourceType]=(517) OR [KeyFileSourceType]=(516) OR [KeyFileSourceType]=(515) OR [KeyFileSourceType]=(514) OR [KeyFileSourceType]=(513) OR [KeyFileSourceType]=(512) OR [KeyFileSourceType]=(511) OR [KeyFileSourceType]=(510) OR [KeyFileSourceType]=(509) OR [KeyFileSourceType]=(507) OR [KeyFileSourceType]=(506) OR [KeyFileSourceType]=(505) OR [KeyFileSourceType]=(504) OR [KeyFileSourceType]=(503) OR [KeyFileSourceType]=(502) OR [KeyFileSourceType]=(501) OR [KeyFileSourceType]=(500) OR [KeyFileSourceType]=(499) OR [KeyFileSourceType]=(498) OR [KeyFileSourceType]=(497) OR [KeyFileSourceType]=(496) OR [KeyFileSourceType]=(495) OR [KeyFileSourceType]=(494) OR [KeyFileSourceType]=(493) OR [KeyFileSourceType]=(492) OR [KeyFileSourceType]=(491) OR [KeyFileSourceType]=(490) OR [KeyFileSourceType]=(489) OR [KeyFileSourceType]=(488) OR [KeyFileSourceType]=(487) OR [KeyFileSourceType]=(486) OR [KeyFileSourceType]=(485) OR [KeyFileSourceType]=(484) OR [KeyFileSourceType]=(483) OR [KeyFileSourceType]=(482) OR [KeyFileSourceType]=(481) OR [KeyFileSourceType]=(480) OR [KeyFileSourceType]=(479) OR [KeyFileSourceType]=(478) OR [KeyFileSourceType]=(477) OR [KeyFileSourceType]=(476) OR [KeyFileSourceType]=(475) OR [KeyFileSourceType]=(474) OR [KeyFileSourceType]=(473) OR [KeyFileSourceType]=(472) OR [KeyFileSourceType]=(471) OR [KeyFileSourceType]=(470) OR [KeyFileSourceType]=(469) OR [KeyFileSourceType]=(468) OR [KeyFileSourceType]=(467) OR [KeyFileSourceType]=(466) OR [KeyFileSourceType]=(465) OR [KeyFileSourceType]=(464) OR [KeyFileSourceType]=(463) OR [KeyFileSourceType]=(462) OR [KeyFileSourceType]=(461) OR [KeyFileSourceType]=(460) OR [KeyFileSourceType]=(459) OR [KeyFileSourceType]=(458) OR [KeyFileSourceType]=(457) OR [KeyFileSourceType]=(406) OR [KeyFileSourceType]=(405) OR [KeyFileSourceType]=(404) OR [KeyFileSourceType]=(403) OR [KeyFileSourceType]=(402) OR [KeyFileSourceType]=(401) OR [KeyFileSourceType]=(400) OR [KeyFileSourceType]=(399) OR [KeyFileSourceType]=(398) OR [KeyFileSourceType]=(397) OR [KeyFileSourceType]=(396) OR [KeyFileSourceType]=(394) OR [KeyFileSourceType]=(393) OR [KeyFileSourceType]=(391) OR [KeyFileSourceType]=(390) OR [KeyFileSourceType]=(389) OR [KeyFileSourceType]=(386) OR [KeyFileSourceType]=(385) OR [KeyFileSourceType]=(384) OR [KeyFileSourceType]=(383) OR [KeyFileSourceType]=(382) OR [KeyFileSourceType]=(378) OR [KeyFileSourceType]=(374) OR [KeyFileSourceType]=(373) OR [KeyFileSourceType]=(371) OR [KeyFileSourceType]=(369) OR [KeyFileSourceType]=(367) OR [KeyFileSourceType]=(365) OR [KeyFileSourceType]=(364) OR [KeyFileSourceType]=(363) OR [KeyFileSourceType]=(362) OR [KeyFileSourceType]=(360) OR [KeyFileSourceType]=(357) OR [KeyFileSourceType]=(356) OR [KeyFileSourceType]=(354) OR [KeyFileSourceType]=(353) OR [KeyFileSourceType]=(352) OR [KeyFileSourceType]=(351) OR [KeyFileSourceType]=(350) OR [KeyFileSourceType]=(347) OR [KeyFileSourceType]=(345) OR [KeyFileSourceType]=(342) OR [KeyFileSourceType]=(341) OR [KeyFileSourceType]=(338) OR [KeyFileSourceType]=(336) OR [KeyFileSourceType]=(335) OR [KeyFileSourceType]=(334) OR [KeyFileSourceType]=(333) OR [KeyFileSourceType]=(331) OR [KeyFileSourceType]=(329) OR [KeyFileSourceType]=(328) OR [KeyFileSourceType]=(327) OR [KeyFileSourceType]=(326) OR [KeyFileSourceType]=(325) OR [KeyFileSourceType]=(322) OR [KeyFileSourceType]=(320) OR [KeyFileSourceType]=(319) OR [KeyFileSourceType]=(316) OR [KeyFileSourceType]=(315) OR [KeyFileSourceType]=(313) OR [KeyFileSourceType]=(310) OR [KeyFileSourceType]=(309) OR [KeyFileSourceType]=(308) OR [KeyFileSourceType]=(307) OR [KeyFileSourceType]=(306) OR [KeyFileSourceType]=(304) OR [KeyFileSourceType]=(303) OR [KeyFileSourceType]=(302) OR [KeyFileSourceType]=(300) OR [KeyFileSourceType]=(297) OR [KeyFileSourceType]=(295) OR [KeyFileSourceType]=(294) OR [KeyFileSourceType]=(293) OR [KeyFileSourceType]=(291) OR [KeyFileSourceType]=(287) OR [KeyFileSourceType]=(286) OR [KeyFileSourceType]=(258) OR [KeyFileSourceType]=(242) OR [KeyFileSourceType]=(235) OR [KeyFileSourceType]=(224) OR [KeyFileSourceType]=(205) OR [KeyFileSourceType]=(179) OR [KeyFileSourceType]=(117) OR [KeyFileSourceType]=(115) OR [KeyFileSourceType]=(83) OR [KeyFileSourceType]=(77) OR [KeyFileSourceType]=(76) OR [KeyFileSourceType]=(73) OR [KeyFileSourceType]=(36) OR [KeyFileSourceType]=(24) OR [KeyFileSourceType]=(18) OR [KeyFileSourceType]=(14) OR [KeyFileSourceType]=(8) OR [KeyFileSourceType]=(3)) then (0) else (1) end)),
	[KeyFileSourceType] [int] NULL,
	[RootIXBRLInstanceFile] [bit] NULL,
	[FilePDFConvertible] [tinyint] NOT NULL,
	[FileGroup] [int] NULL,
	[KeyFileType] [smallint] NULL,
	[KeyFileTypeSourced] [int] NULL,
	[KeyTranscriptPubType] [tinyint] NULL,
	[SearchIndexed] [bit] NOT NULL,
	[MaskDataCtrSearchIndexLoc] [bigint] NULL,
	[MaskDataCtrSearchIndexAvailLoc] [bigint] NULL,
	[AppStatus] [smallint] NOT NULL,
	[UpdOperation] [tinyint] NOT NULL,
	[UpdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_FileCollectionDetailWO] PRIMARY KEY NONCLUSTERED 
(
	[KeyFileCollectionDetail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF Object_Id('fileconversioninputoutput_tbl') IS NOT NULL  
BEGIN
	Drop table fileconversioninputoutput_tbl
END
Go
CREATE TABLE [dbo].[fileconversioninputoutput_tbl](
	[KeyFileConversionCodexAnnot] [uniqueidentifier] NULL,
	[KeyFileConversionCodexText] [uniqueidentifier] NULL,
	[KeyFileConversionCodexCoord] [uniqueidentifier] NULL,
	[KeyFileConversionCodexPDFInfo] [uniqueidentifier] NULL,
	[KeyFileConversionCodexDisplay] [uniqueidentifier] NULL,
	[InputKeyFileVersion] [uniqueidentifier] NULL,
	[OutPutKeyFileVersionCodexAnnot] [uniqueidentifier] NULL,
	[OutPutKeyFileVersionCodexText] [uniqueidentifier] NULL,
	[OutPutKeyFileVersionCodexCoord] [uniqueidentifier] NULL,
	[OutPutKeyFileVersionCodexPDFInfo] [uniqueidentifier] NULL,
	[OutPutKeyFileVersionCodexDisplay] [uniqueidentifier] NULL,
	[CodexDocubotVersion] [varchar](30) NULL
) ON [PRIMARY]
GO

IF Object_Id('FileConversionInputWO') IS NOT NULL  
BEGIN
	Drop table FileConversionInputWO
END

CREATE TABLE [dbo].[FileConversionInputWO](
	[KeyFileConversionInput] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[KeyFileConversion] [uniqueidentifier] NOT NULL,
	[KeyFileVersion] [uniqueidentifier] NOT NULL,
	[UpdOperation] [tinyint] NOT NULL,
	[UpdDate] [datetime] NOT NULL,
	[KeyConversionProcessVersion] int NULL,
 CONSTRAINT [PK_FileConversionInputWO] PRIMARY KEY NONCLUSTERED 
(
	[KeyFileConversionInput] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO


IF Object_Id('FileConversionOutputWO') IS NOT NULL  
BEGIN
	Drop table FileConversionOutputWO
END
GO

CREATE TABLE [dbo].[FileConversionOutputWO](
	[KeyFileConversionOutput] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[KeyFileConversion] [uniqueidentifier] NOT NULL,
	[KeyFileVersion] [uniqueidentifier] NOT NULL,
	[UpdOperation] [tinyint] NOT NULL,
	[UpdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_FileConversionOutputWO] PRIMARY KEY NONCLUSTERED 
(
	[KeyFileConversionOutput] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

IF Object_Id('FileConversionWO') IS NOT NULL  
BEGIN
	Drop table FileConversionWO
END
GO
CREATE TABLE [dbo].[FileConversionWO](
	[KeyFileConversion] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[KeyConversionProcessVersion] [int] NOT NULL,
	[KeyFileConversionStatus] [tinyint] NOT NULL,
	[FileConversionStart] [datetime] NULL,
	[FileConversionEnd] [datetime] NULL,
	[UpdOperation] [tinyint] NOT NULL,
	[UpdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_FileConversionWO] PRIMARY KEY CLUSTERED 
(
	[KeyFileConversion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
IF Object_Id('FileVersionWO') IS NOT NULL  
BEGIN
	Drop table FileVersionWO
END
GO

CREATE TABLE [dbo].[FileVersionWO](
	[KeyFileVersion] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[FileURI] [varchar](255) NOT NULL,
	[KeyFileFormat] [smallint] NOT NULL,
	[HashValue] [varchar](255) NULL,
	[BytesExtended] [bigint] NULL,
	[FilePersisted] [bit] NOT NULL,
	[KeyFileCollectionDetail] [int] NOT NULL,
	[FileStorageDate] [smalldatetime] NOT NULL,
	[MainVersion] [bit] NOT NULL,
	[KeyFileVersionType] [tinyint] NULL,
	[WriteRestricted] [bit] NULL,
	[PublicDocument] [bit] NULL,
	[AppStatus] [smallint] NOT NULL,
	[UpdOperation] [tinyint] NOT NULL,
	[UpdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_FileVersionWO] PRIMARY KEY NONCLUSTERED 
(
	[KeyFileVersion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [KeyForeignLanguage]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((1)) FOR [MainFile]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [FilePDFConvertible]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [SearchIndexed]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [MaskDataCtrSearchIndexLoc]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [MaskDataCtrSearchIndexAvailLoc]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [AppStatus]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT ((0)) FOR [UpdOperation]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO] ADD  DEFAULT (getdate()) FOR [UpdDate]
GO
ALTER TABLE [dbo].[FileConversionInputWO] ADD  DEFAULT (newid()) FOR [KeyFileConversionInput]
GO
ALTER TABLE [dbo].[FileConversionInputWO] ADD  DEFAULT ((0)) FOR [UpdOperation]
GO
ALTER TABLE [dbo].[FileConversionInputWO] ADD  DEFAULT (getdate()) FOR [UpdDate]
GO
ALTER TABLE [dbo].[FileConversionOutputWO] ADD  DEFAULT (newid()) FOR [KeyFileConversionOutput]
GO
ALTER TABLE [dbo].[FileConversionOutputWO] ADD  DEFAULT ((0)) FOR [UpdOperation]
GO
ALTER TABLE [dbo].[FileConversionOutputWO] ADD  DEFAULT (getdate()) FOR [UpdDate]
GO
ALTER TABLE [dbo].[FileConversionWO] ADD  DEFAULT (newid()) FOR [KeyFileConversion]
GO
ALTER TABLE [dbo].[FileConversionWO] ADD  DEFAULT ((0)) FOR [KeyFileConversionStatus]
GO
ALTER TABLE [dbo].[FileConversionWO] ADD  DEFAULT ((0)) FOR [UpdOperation]
GO
ALTER TABLE [dbo].[FileConversionWO] ADD  DEFAULT (getdate()) FOR [UpdDate]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT (newid()) FOR [KeyFileVersion]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT ((0)) FOR [FilePersisted]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT (getdate()) FOR [FileStorageDate]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT ((1)) FOR [MainVersion]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT ((0)) FOR [AppStatus]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT ((0)) FOR [UpdOperation]
GO
ALTER TABLE [dbo].[FileVersionWO] ADD  DEFAULT (getdate()) FOR [UpdDate]
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  ((NOT [AccessionNumberSEDAR] like '' AND NOT [AccessionNumberSEDAR] like ' %' AND NOT [AccessionNumberSEDAR] like '% '))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([AppStatus]<(3)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  ((NOT [FileDescription] like '' AND NOT [FileDescription] like ' %' AND NOT [FileDescription] like '% '))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([FilePDFConvertible]<(2)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([MainFile]<(2)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([Pages]>(0)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([TranslationLevel]=(3) OR [TranslationLevel]=(2) OR [TranslationLevel]=(1) OR [TranslationLevel]=(0)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([TranslationPublishable]<(2)))
GO
ALTER TABLE [dbo].[FileCollectionDetailWO]  WITH CHECK ADD CHECK  (([UpdOperation]<(3)))
GO
ALTER TABLE [dbo].[FileConversionInputWO]  WITH CHECK ADD CHECK  (([UpdOperation]<(3)))
GO
ALTER TABLE [dbo].[FileConversionOutputWO]  WITH CHECK ADD CHECK  (([UpdOperation]<(3)))
GO
ALTER TABLE [dbo].[FileConversionWO]  WITH CHECK ADD CHECK  (([UpdOperation]<(3)))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  (([AppStatus]<(3)))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  (([BytesExtended]>=(0)))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  ((NOT [FileURI] like '' AND NOT [FileURI] like ' %' AND NOT [FileURI] like '% '))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  ((NOT [HashValue] like '' AND NOT [HashValue] like ' %' AND NOT [HashValue] like '% '))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  (([PublicDocument]=(1)))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  (([UpdOperation]<(3)))
GO
ALTER TABLE [dbo].[FileVersionWO]  WITH CHECK ADD CHECK  (([WriteRestricted]=(1)))
GO

IF EXISTS(Select top 1 1 from sys.tables where name like '%TempCodexWorkingSet%')
BEGIN
Drop table TempCodexWorkingSet
END
Go
CREATE TABLE [dbo].[TempCodexWorkingSet](
	[newKeyFileCollectionDetail] [int] NOT NULL,
	[KeyFileVersion] [uniqueidentifier] NOT NULL,
	[KeyFileVersionCodexDisplay] [uniqueidentifier] NULL,
	[BytesExtendedCodexDisplay] [bigint] NULL,
	[HashValueCodexDisplay] [varchar](255) NULL,
	[KeyFileFormatCodexDisplay] [smallint] NULL,
	[KeyFileVersionCodexText] [uniqueidentifier] NULL,
	[BytesExtendedCodexText] [bigint] NULL,
	[HashValueCodexText] [varchar](255) NULL,
	[KeyFileVersionCodexAnnot] [uniqueidentifier] NULL,
	[BytesExtendedCodexAnnot] [bigint] NULL,
	[HashValueCodexAnnot] [varchar](255) NULL,
	[KeyFileVersionCodexCoord] [uniqueidentifier] NULL,
	[BytesExtendedCodexCoord] [bigint] NULL,
	[HashValueCodexCoord] [varchar](255) NULL,
	[KeyFileVersionCodexPDFInfo] [uniqueidentifier] NULL,
	[BytesExtendedCodexPDFInfo] [bigint] NULL,
	[HashValueCodexPDFInfo] [varchar](255) NULL
) ON [PRIMARY]
GO

IF EXISTS(Select top 1 1 from sys.tables where name like '%TempCodexConversionWorkingSet%')
BEGIN
Drop table TempCodexConversionWorkingSet
END
Go

CREATE TABLE [dbo].[TempCodexConversionWorkingSet](
	[newKeyFileCollectionDetail] [int] NOT NULL,
	[InputKeyFileVersion] [uniqueidentifier] NOT NULL,
	[KeyFileVersionCodexAnnot] [uniqueidentifier] NULL,
	[KeyFileVersionCodexText] [uniqueidentifier] NULL,
	[KeyFileVersionCodexCoord] [uniqueidentifier] NULL,
	[KeyFileVersionCodexPDFInfo] [uniqueidentifier] NULL,
	[KeyFileVersionCodexDisplay] [uniqueidentifier] NULL,
	[CodexDocubotVersion] [varchar](30) NOT NULL
) ON [PRIMARY]
Go

/*
Use Codex_IR_00
go
 EXEC Populate_CodexWorkingSet_prc
*/
---------------------------------------------Step 1 Populate Codex Working Set Start -------------------------------------   2 minutes -- 1.5 million  - (15 million) - 15 -20 min (DMZSQLTEST)
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_CodexWorkingSet_prc%')
	Drop  Procedure Populate_CodexWorkingSet_prc
GO
Create Procedure Populate_CodexWorkingSet_prc
AS 
BEGIN

INSERT INTO CodeMigrationScriptLog_tbl (
	stepId
	,stepName
	,startDate
	,enddate
	,InsertedDatetime
	)
VALUES (
	1
	,'Begin Populate CodexWorkingSet'
	,GETDATE()
	,NULL
	,GETDATE()
	)


;WITH CTE
AS (
	SELECT cix.KeyCodexIntermediate, cix.KeyFileVersion, cix.CodexDocubotVersion
	FROM InternalUseOnly..CodexIntermediate cix WITH (NOLOCK)
	WHERE cix.updoperation < 2
		AND SweeperProcessingPriority = 202
		AND SweeperProcessingCompleted IS NULL
	)
Insert into  Codex_IR_00.dbo.CodexWorkingSet_tbl (KeyCodexIntermediate, KeyFileVersion, KeyFileCollectionDetail, CodexDocubotVersion)
SELECT DISTINCT cix.KeyCodexIntermediate
	,cix.KeyFileVersion
	,fv.KeyFileCollectionDetail
	,cix.CodexDocubotVersion
FROM CTE cix
INNER JOIN objectviews..Docsfileversion fv WITH (NOLOCK) ON cix.KeyFileVersion = fv.KeyFileVersion


CREATE NONCLUSTERED INDEX cx_temp ON Codex_IR_00..CodexWorkingSet_tbl (KeyFileVersion, CodexDocubotVersion)
CREATE NONCLUSTERED INDEX cx_temp1 ON Codex_IR_00..CodexWorkingSet_tbl (KeyFileCollectionDetail)


;WITH CTE AS
(
Select ROW_NUMBER() OVER(PARTITION BY KeyFileCollectionDetail Order by KeyFileCollectionDetail) ROWNUMBER,* from CodexWorkingSet_tbl where KeyFileCollectionDetail in 
(
Select  KeyFileCollectionDetail from CodexWorkingSet_tbl
group by KeyFileCollectionDetail
Having Count(KeyFileCollectionDetail) >1
)
--Order by KeyFileCollectionDetail
)
Delete from CTE Where ROWNUMBER >1

UPDATE CodeMigrationScriptLog_tbl
SET enddate = GETDATE()
WHERE stepId = 1
---------------------------------------------Step 1 Populate Codex Working Set Complete ------------------------------------- 
EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_CodexWorkingSet',@enabled = 0
EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_FileCollectionDetailWO',@enabled = 1


END
GO
--EXEC Populate_FileCollectionDetailWO_prc 1690000
---------------------------------------------Step 2 Populate FileCollectionDetail WO Start ------------------------------------- 3minutes
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_FileCollectionDetailWO_prc%')
	Drop  Procedure Populate_FileCollectionDetailWO_prc
GO
Create Procedure Populate_FileCollectionDetailWO_prc
(
@BatchCount int =10000
)
AS 
BEGIN
	Declare @NewId uniqueidentifier = (Select NEWID())
	Declare @KFCDID int = (Select Top 1 lastRecordId from CodexStepLastId_tbl WITH(NOLOCK) Where StepId =2)
	INSERT INTO CodeMigrationScriptLog_tbl (
		[keycodemigrationscriptlog],
		stepId
		,stepName
		,startDate
		,enddate
		,InsertedDatetime
		)
	VALUES (
		@newId
		,2
		,'Begin Populate FileCollectionDetailWO'
		,GETDATE()
		,NULL
		,GETDATE()
		)
		
	SELECT Top (@BatchCount)  cxws.newKeyFileCollectionDetail
		,'Codex' AS FileDescription
		,0 AS MainFile
		,2 AS AppStatus
		,0 AS UpdOperation
		,getdate() AS UpdDate
		,cxws.KeyFileCollectionDetail
		into #TempCodexWorkingSet
	FROM Codex_IR_00..CodexWorkingSet_tbl cxws  WITH (NOLOCK)
	Where cxws.newKeyFileCollectionDetail >@KFCDID
	
	INSERT INTO Codex_IR_00..FileCollectionDetailWO (
	KeyFileCollectionDetail
	,FileDescription
	,KeyFileCollection
	,KeyFileSource
	,KeyFileType
	,MainFile
	,MaskDataCtrSearchIndexAvailLoc
	,KeyForeignLanguage
	,pages
	,AppStatus
	,UpdOperation
	,UpdDate
	)
	SELECT cxws.newKeyFileCollectionDetail
	,'Codex' AS FileDescription
	,fcd.KeyFileCollection
	,fcd.KeyFileSource
	,fcd.KeyFileType
	,0 AS MainFile
	,3 AS MaskDataCtrSearchIndexAvailLoc
	,fcd.KeyForeignLanguage
	,fcd.pages
	,2 AS AppStatus
	,0 AS UpdOperation
	,getdate() AS UpdDate
	FROM #TempCodexWorkingSet cxws  WITH (NOLOCK)
	INNER JOIN objectviews..DocsFileCollectionDetail fcd(NOLOCK) ON fcd.KeyFileCollectionDetail = cxws.KeyFileCollectionDetail
	

	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE [keycodemigrationscriptlog] = @NewId

	Declare @MaxId  bigint =(Select MAX(newKeyFileCollectionDetail) from #TempCodexWorkingSet)
	Update
	CodexStepLastId_tbl 
	Set lastRecordId = @MaxId
	Where StepId =2

	IF NOT EXISTS (Select Top 1 1 from Codex_IR_00..CodexWorkingSet_tbl WITH(NOLOCK) Where newKeyFileCollectionDetail > @MaxId)
	BEGIN
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_FileCollectionDetailWO',@enabled = 0
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_FileVersionWO',@enabled = 1
	END 
--- Inserting one new record in FileCollectionDetail table for each basic workingsetId with new filecollection Detail Id
---7/21/22 - pageCount. Intermediate does nto have it. We use the one from existing record but this may not be accurate?
----pulling mainfile from FileCollectionDetail but we need something to reset the nonCodex to 0? 
END
Go

---------------------------------------------Step 2 Populate FileCollectionDetail WO End -------------------------------------

---------------------------------------------Step 3 Populate FileVersionWO WO Start -------------------------------------  7 minutes
--EXEC Populate_FileCollectionVersionWO_prc 1690000
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_FileCollectionVersionWO_prc%')
	Drop  Procedure Populate_FileCollectionVersionWO_prc
Go
Create Procedure Populate_FileCollectionVersionWO_prc
(
@BatchCount int =10000
)
AS 
BEGIN

	Declare @NewId uniqueidentifier = (Select NEWID())
	Declare @KFCDID int = (Select Top 1 lastRecordId from CodexStepLastId_tbl WITH(NOLOCK) Where StepId =3)

	INSERT INTO CodeMigrationScriptLog_tbl (
		[keycodemigrationscriptlog]
		,stepId
		,stepName
		,startDate
		,enddate
		,InsertedDatetime
		)
	VALUES (
		@NewId
		,3
		,'Begin Populate FileVersionWO'
		,GETDATE()
		,NULL
		,GETDATE()
		)
	
	Declare @S3URIPath nvarchar(MAX)='sc://codex/';
	Declare @CurrentDate datetime= GETDATE(); 

	IF EXISTS (Select top 1 1 from sys.tables where name like '%TempCodexWorkingSet%')
	BEGIN
		Drop table TempCodexWorkingSet
	END

	SELECT 
			cxwt.newKeyFileCollectionDetail,
			cxwt.KeyFileVersion,
			KeyFileVersionCodexDisplay,
			BytesExtendedCodexDisplay,
			HashValueCodexDisplay,
			KeyFileFormatCodexDisplay,
			KeyFileVersionCodexText,
			BytesExtendedCodexText,
			HashValueCodexText,
			KeyFileVersionCodexAnnot,
			BytesExtendedCodexAnnot,
			HashValueCodexAnnot,
			KeyFileVersionCodexCoord,
			BytesExtendedCodexCoord,
			HashValueCodexCoord,
			KeyFileVersionCodexPDFInfo,
			BytesExtendedCodexPDFInfo,
			HashValueCodexPDFInfo
			into TempCodexWorkingSet
		FROM Codex_IR_00..CodexWorkingSet_tbl cxwt  WITH (NOLOCK)
		Join InternalUseOnly..CodexIntermediate cx WITH (NOLOCK) 
		 ON cx.KeyCodexIntermediate = cxwt.KeyCodexIntermediate
    WHERE cxwt.newKeyFileCollectionDetail > @KFCDID and cxwt.newKeyFileCollectionDetail <= (select @KFCDID + @BatchCount);
	
	--Display
	INSERT INTO Codex_IR_00..FileVersionWO (
		KeyFileCollectionDetail
		,KeyFileVersion
		,FileURI
		,Hashvalue
		,BytesExtended
		,KeyFileFormat
		,MainVersion
		,KeyFileVersionType
		,FileStorageDate
		,AppStatus
		,UpdOperation
		,UpdDate
		,FilePersisted
		)
	SELECT newKeyFileCollectionDetail
		,KeyFileVersionCodexDisplay KeyFileVersion
		,@S3URIPath + Lower(replace(KeyFileVersionCodexDisplay, '-', '')) AS FileURI
		,HashValueCodexDisplay Hashvalue
		,BytesExtendedCodexDisplay BytesExtended
		,KeyFileFormatCodexDisplay AS KeyFileFormat
		,1 AS MainVersion
		,0 AS KeyFileVersionType
		,@CurrentDate AS FileStorageDate
		,2 AS AppStatus
		,0 AS UpdOperation
		,@CurrentDate AS UpdDate
		,1 AS FilePersisted
	FROM TempCodexWorkingSet
	WHERE KeyFileVersionCodexDisplay IS NOT NULL
	--Text
	INSERT INTO Codex_IR_00..FileVersionWO (
	KeyFileCollectionDetail
	,KeyFileVersion
	,FileURI
	,Hashvalue
	,BytesExtended
	,KeyFileFormat
	,MainVersion
	,KeyFileVersionType
	,FileStorageDate
	,AppStatus
	,UpdOperation
	,UpdDate
	,FilePersisted
	)
	SELECT DISTINCT newKeyFileCollectionDetail
		,KeyFileVersionCodexText KeyFileVersion
		,@S3URIPath + Lower(replace(KeyFileVersionCodexText, '-', '')) AS FileURI
		,HashValueCodexText Hashvalue
		,BytesExtendedCodexText BytesExtended
		,0 AS KeyFileFormat
		,0 AS Mainversion
		,2 AS KeyFileVersionType
		,@CurrentDate AS FileStorageDate
		,2 AS AppStatus
		,0 AS UpdOperation
		,@CurrentDate AS UpdDate
		,1 AS FilePersisted
	FROM TempCodexWorkingSet
	WHERE KeyFileVersionCodexText IS NOT NULL
	--Annot
	INSERT INTO Codex_IR_00..FileVersionWO (
	KeyFileCollectionDetail
	,KeyFileVersion
	,FileURI
	,Hashvalue
	,BytesExtended
	,KeyFileFormat
	,MainVersion
	,KeyFileVersionType
	,FileStorageDate
	,AppStatus
	,UpdOperation
	,UpdDate
	,FilePersisted
	)
	SELECT newKeyFileCollectionDetail
		,KeyFileVersionCodexAnnot KeyFileVersion
		,@S3URIPath + Lower(replace(KeyFileVersionCodexAnnot, '-', '')) AS FileURI
		,HashValueCodexAnnot Hashvalue
		,BytesExtendedCodexAnnot BytesExtended
		,45 AS KeyFileFormat
		,0 AS MainVersion
		,0 AS KeyFileVersionType
		,@CurrentDate AS FileStorageDate
		,2 AS AppStatus
		,0 AS UpdOperation
		,@CurrentDate AS UpdDate
		,1 AS FilePersisted
	FROM TempCodexWorkingSet
	WHERE KeyFileVersionCodexAnnot IS NOT NULL
	--Coord
	INSERT INTO Codex_IR_00..FileVersionWO (
		KeyFileCollectionDetail
		,KeyFileVersion
		,FileURI
		,Hashvalue
		,BytesExtended
		,KeyFileFormat
		,MainVersion
		,KeyFileVersionType
		,FileStorageDate
		,AppStatus
		,UpdOperation
		,UpdDate
		,FilePersisted
		)
	SELECT newKeyFileCollectionDetail
		,KeyFileVersionCodexCoord KeyFileVersion
		,@S3URIPath + Lower(replace(KeyFileVersionCodexCoord, '-', '')) AS FileURI
		,HashValueCodexCoord HashValue
		,BytesExtendedCodexCoord BytesExtended
		,45 AS KeyFileFormat
		,0 AS Mainversion
		,2 AS KeyFileVersionType
		,@CurrentDate AS FileStorageDate
		,2 AS AppStatus
		,0 AS UpdOperation
		,@CurrentDate AS UpdDate
		,1 AS FilePersisted
	FROM TempCodexWorkingSet
	WHERE KeyFileVersionCodexCoord IS NOT NULL
	--PDF
	INSERT INTO Codex_IR_00..FileVersionWO (
		KeyFileCollectionDetail
		,KeyFileVersion
		,FileURI
		,Hashvalue
		,BytesExtended
		,KeyFileFormat
		,MainVersion
		,KeyFileVersionType
		,FileStorageDate
		,AppStatus
		,UpdOperation
		,UpdDate
		,FilePersisted
		)
	SELECT newKeyFileCollectionDetail
		,KeyFileVersionCodexPDFInfo
		,@S3URIPath + Lower(replace(KeyFileVersionCodexPDFInfo, '-', '')) AS URI
		,HashValueCodexPDFInfo HashValue
		,BytesExtendedCodexPDFInfo BytesExtended
		,45 AS KeyFileFormat
		,0 AS Mainversion
		,2 AS KeyFileVersionType
		,@CurrentDate AS FileStorageDate
		,2 AS AppStatus
		,0 AS UpdOperation
		,@CurrentDate AS UpdDate
		,1 AS FilePersisted
	FROM TempCodexWorkingSet
	WHERE KeyFileVersionCodexPDFInfo IS NOT NULL

	
	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE [keycodemigrationscriptlog] = @NewId

	Declare @MaxId  bigint =(Select MAX(newKeyFileCollectionDetail) from TempCodexWorkingSet)
	Update
	CodexStepLastId_tbl 
	Set lastRecordId = @MaxId
	Where StepId =3

	IF NOT EXISTS (Select Top 1 1 from Codex_IR_00..CodexWorkingSet_tbl WITH(NOLOCK) Where newKeyFileCollectionDetail > @MaxId)
	BEGIN
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_FileVersionWO',@enabled = 0
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversioninputoutputWO',@enabled = 1
	END 
	
END
GO



---------------------------------------------Step 3 Populate FileVersionWO WO End -------------------------------------  

---------------------------------------------Step 4 Populate FileConversionInputOutput Start -------------------------------------  1 minute
--EXEC Populate_fileconversioninputoutput_tbl_prc 1700000
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_fileconversioninputoutput_tbl_prc%')
	Drop  Procedure Populate_fileconversioninputoutput_tbl_prc
Go
Create Procedure Populate_fileconversioninputoutput_tbl_prc
(
@BatchCount int =10000
)
AS 
BEGIN
Declare @NewId uniqueidentifier = (Select NEWID())
Declare @KFCDID int = (Select Top 1 lastRecordId from CodexStepLastId_tbl WITH(NOLOCK) Where StepId =4)
	INSERT INTO CodeMigrationScriptLog_tbl (
		[keycodemigrationscriptlog],
		stepId
		,stepName
		,startDate
		,enddate
		,InsertedDatetime
		)
	VALUES (
		@newId
		,4
		,'Begin Populate fileconversioninputoutput_tbl'
		,GETDATE()
		,NULL
		,GETDATE()
		)
		IF EXISTS (Select top 1 1 from sys.tables where name like '%TempCodexConversionWorkingSet%')
		BEGIN
			Drop table TempCodexConversionWorkingSet
		END
		SELECT  cxwt.newKeyFileCollectionDetail,
			cx.KeyFileVersion as InputKeyFileVersion, 
			cx.KeyFileVersionCodexAnnot,
			cx.KeyFileVersionCodexText,
			cx.KeyFileVersionCodexCoord,
			cx.KeyFileVersionCodexPDFInfo,
			cx.KeyFileVersionCodexDisplay,
			cx.CodexDocubotVersion
			into TempCodexConversionWorkingSet
		FROM Codex_IR_00..CodexWorkingSet_tbl cxwt  WITH (NOLOCK)
		inner join InternalUseOnly..CodexIntermediate cx ON cx.KeyCodexIntermediate = cxwt.KeyCodexIntermediate
		WHERE cxwt.newKeyFileCollectionDetail > @KFCDID and cxwt.newKeyFileCollectionDetail <= (select @KFCDID + @BatchCount);

	Insert Into Codex_IR_00..fileconversioninputoutput_tbl
			(
			KeyFileConversionCodexAnnot,
			KeyFileConversionCodexText, 
			KeyFileConversionCodexCoord,
			KeyFileConversionCodexPDFInfo,  
			KeyFileConversionCodexDisplay,
			InputKeyFileVersion,
			OutPutKeyFileVersionCodexAnnot,
			OutPutKeyFileVersionCodexText,
			OutPutKeyFileVersionCodexCoord,
			OutPutKeyFileVersionCodexPDFInfo,
			OutPutKeyFileVersionCodexDisplay,
			CodexDocubotVersion
			)
	Select		newid() as KeyFileConversionCodexAnnot, 
				newid() as KeyFileConversionCodexText, 
				newid() as KeyFileConversionCodexCoord, 
				newid() as KeyFileConversionCodexPDFInfo, 
				newid() as KeyFileConversionCodexDisplay,
			    cx.InputKeyFileVersion as InputKeyFileVersion, 
				cx.KeyFileVersionCodexAnnot,
				cx.KeyFileVersionCodexText,
				cx.KeyFileVersionCodexCoord,
				cx.KeyFileVersionCodexPDFInfo,
				cx.KeyFileVersionCodexDisplay,
				cx.CodexDocubotVersion
	from TempCodexConversionWorkingSet cx
	

	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE [keycodemigrationscriptlog] = @NewId

	Declare @MaxId  bigint =(Select MAX(newKeyFileCollectionDetail) from TempCodexConversionWorkingSet)
	Update
	CodexStepLastId_tbl 
	Set lastRecordId = @MaxId
	Where StepId =4

	IF NOT EXISTS (Select Top 1 1 from Codex_IR_00..CodexWorkingSet_tbl WITH(NOLOCK) Where newKeyFileCollectionDetail > @MaxId)
	BEGIN
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversioninputoutputWO',@enabled = 0
		EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversionWO',@enabled = 1
	END 
	--- Inserting one new record in FileCollectionDetail table for each basic workingsetId with new filecollection Detail Id
---7/21/22 - pageCount. Intermediate does nto have it. We use the one from existing record but this may not be accurate?
----pulling mainfile from FileCollectionDetail but we need something to reset the nonCodex to 0? 
END

go

---------------------------------------------Step 4 Populate FileConversionInputOutput End -------------------------------------  

---------------------------------------------Step 5 Populate FileConversionInput Start -------------------------------------  30 seconds

---------------------------------------------Step 3 Populate FileVersionWO WO End -------------------------------------  

---------------------------------------------Step 4 Populate FileConversionInputOutput Start -------------------------------------  1 minute
--EXEC Populate_fileconversionWO_tbl_prc
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_fileconversionWO_tbl_prc%')
	Drop  Procedure Populate_fileconversionWO_tbl_prc
Go
Create Procedure Populate_fileconversionWO_tbl_prc
AS 
BEGIN

	INSERT INTO CodeMigrationScriptLog_tbl (
	stepId
	,stepName
	,startDate
	,enddate
	,InsertedDatetime
	)
	VALUES (
	5
	,'Begin Populate FileConversionInputWO'
	,GETDATE()
	,NULL
	,GETDATE()
	)
	
	Insert into Codex_IR_00..FileConversionInputWO(KeyFileConversionInput,KeyFileConversion,KeyFileVersion,KeyConversionProcessVersion,UpdOperation, Upddate)
	select 
	newid() as KeyFileConversionInput, KeyFileConversionCodexAnnot , InputKeyFileVersion ,b.KeyConversionProcessVersion , 0 , GETDATE()
	from Codex_IR_00..fileconversioninputoutput_tbl a (nolock) inner join publish2..ConversionProcessVersion b (nolock) on a.CodexDocubotVersion= b.ConversionProcessVersion
	Where b.KeyConversionProcess in (13)
	
	Insert into Codex_IR_00..FileConversionInputWO(KeyFileConversionInput,KeyFileConversion,KeyFileVersion,KeyConversionProcessVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionInput, KeyFileConversionCodexText , InputKeyFileVersion ,b.KeyConversionProcessVersion , 0 , GETDATE()
	from Codex_IR_00..fileconversioninputoutput_tbl a (nolock) inner join publish2..ConversionProcessVersion b (nolock) on a.CodexDocubotVersion= b.ConversionProcessVersion
	Where b.KeyConversionProcess in (14)
	
	Insert into Codex_IR_00..FileConversionInputWO(KeyFileConversionInput,KeyFileConversion,KeyFileVersion,KeyConversionProcessVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionInput, KeyFileConversionCodexCoord , InputKeyFileVersion ,b.KeyConversionProcessVersion , 0 , GETDATE()
	from Codex_IR_00..fileconversioninputoutput_tbl a (nolock) inner join publish2..ConversionProcessVersion b (nolock) on a.CodexDocubotVersion= b.ConversionProcessVersion
	Where b.KeyConversionProcess in (15)
	
	Insert into Codex_IR_00..FileConversionInputWO(KeyFileConversionInput,KeyFileConversion,KeyFileVersion,KeyConversionProcessVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionInput, KeyFileConversionCodexPDFInfo , InputKeyFileVersion ,b.KeyConversionProcessVersion , 0 , GETDATE()
	from Codex_IR_00..fileconversioninputoutput_tbl a (nolock) inner join publish2..ConversionProcessVersion b (nolock) on a.CodexDocubotVersion= b.ConversionProcessVersion
	Where b.KeyConversionProcess in (16)
	
	Insert into Codex_IR_00..FileConversionInputWO(KeyFileConversionInput,KeyFileConversion,KeyFileVersion,KeyConversionProcessVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionInput, KeyFileConversionCodexDisplay , InputKeyFileVersion ,b.KeyConversionProcessVersion , 0 , GETDATE()
	from Codex_IR_00..fileconversioninputoutput_tbl a (nolock) inner join publish2..ConversionProcessVersion b (nolock) on a.CodexDocubotVersion= b.ConversionProcessVersion
	Where b.KeyConversionProcess in (12)
	
	UPDATE CodeMigrationScriptLog_tbl
SET enddate = GETDATE()
WHERE stepId = 5
	---------------------------------------------Step 5 Populate FileConversionInput End -------------------------------------  
	
---------------------------------------------Step 6 Populate FileConversionInput Start -------------------------------------  30 seconds
	
	INSERT INTO CodeMigrationScriptLog_tbl (
	stepId
	,stepName
	,startDate
	,enddate
	,InsertedDatetime
	)
VALUES (
	6
	,'Begin Populate FileConversionOutputWO'
	,GETDATE()
	,NULL
	,GETDATE()
	)
	
		
	Insert into Codex_IR_00..FileConversionOutputWO(KeyFileConversionOutput,KeyFileConversion,KeyFileVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionOutput, KeyFileConversionCodexAnnot, OutPutKeyFileVersionCodexAnnot,0,GETDATE() from Codex_IR_00..fileconversioninputoutput_tbl (nolock)
	Where OutPutKeyFileVersionCodexAnnot is not null
	
	Insert into Codex_IR_00..FileConversionOutputWO(KeyFileConversionOutput,KeyFileConversion,KeyFileVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionOutput, KeyFileConversionCodexText, OutPutKeyFileVersionCodexText,0,GETDATE() from Codex_IR_00..fileconversioninputoutput_tbl  (nolock)
	Where OutPutKeyFileVersionCodexText is not null
	
	Insert into Codex_IR_00..FileConversionOutputWO(KeyFileConversionOutput,KeyFileConversion,KeyFileVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionOutput, KeyFileConversionCodexCoord, OutPutKeyFileVersionCodexCoord,0,GETDATE() from Codex_IR_00..fileconversioninputoutput_tbl  (nolock) 
	Where OutPutKeyFileVersionCodexCoord is not null
	
	Insert into Codex_IR_00..FileConversionOutputWO(KeyFileConversionOutput,KeyFileConversion,KeyFileVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionOutput, KeyFileConversionCodexPDFInfo, OutPutKeyFileVersionCodexPDFInfo,0,GETDATE() from Codex_IR_00..fileconversioninputoutput_tbl (nolock)
	Where OutPutKeyFileVersionCodexPDFInfo is not null
	
	Insert into Codex_IR_00..FileConversionOutputWO(KeyFileConversionOutput,KeyFileConversion,KeyFileVersion,UpdOperation, Upddate)
	select newid() as KeyFileConversionOutput, KeyFileConversionCodexDisplay, OutPutKeyFileVersionCodexDisplay,0,GETDATE() from Codex_IR_00..fileconversioninputoutput_tbl  (nolock)
	Where OutPutKeyFileVersionCodexDisplay is not null
	
	
	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE stepId = 6
	---------------------------------------------Step 6 Populate FileConversionOutput End -------------------------------------  
	---------------------------------------------Step 7 Populate FileConversion Start -------------------------------------  2 minutes
	
	INSERT INTO CodeMigrationScriptLog_tbl (
	stepId
	,stepName
	,startDate
	,enddate
	,InsertedDatetime
	)
VALUES (
	7
	,'Begin Populate FileConversion'
	,GETDATE()
	,NULL
	,GETDATE()
	)
	
	Insert into Codex_IR_00..fileconversionWO(KeyFileConversion,KeyConversionProcessVersion,KeyFileConversionStatus, UpdOperation, Upddate)
	select KeyFileConversion,KeyConversionProcessVersion,3,0, GETDATE() from Codex_IR_00..FileconversionInputWO (nolock)
	
	Create clustered index cx_temp on FileConversionInputWO(KeyFileConversion)
	Create clustered index cx_temp1 on FileConversionOutputWO(KeyFileConversion)
	--Create clustered index cx_temp2 on fileconversionWO(KeyFileConversion)
	
	IF Object_Id('tempdb..#fileconversionwithoutOutputversion_tbl') IS NOT NULL  
		Drop table  #fileconversionwithoutOutputversion_tbl
	
	Select KeyFileConversion
Into #fileconversionwithoutOutputversion_tbl
from Codex_IR_00..FileConversionInputWO a with (nolock) Where not exists 
(
Select 1 from Codex_IR_00..FileConversionOutputWO b with(nolock) Where a.KeyFileConversion =b.KeyFileConversion
)
	
	Create clustered index cx_temp on #fileconversionwithoutOutputversion_tbl(KeyFileConversion)
	
	Delete a from  Codex_IR_00..FileConversionInputWO a Where Exists (Select 1 from #fileconversionwithoutOutputversion_tbl b Where a.KeyFileConversion=b.KeyFileConversion)
	Delete a from  Codex_IR_00..FileConversionOutputWO a Where Exists (Select 1 from #fileconversionwithoutOutputversion_tbl b Where a.KeyFileConversion=b.KeyFileConversion)
	Delete a from  Codex_IR_00..fileconversionWO a Where Exists (Select 1 from #fileconversionwithoutOutputversion_tbl b Where a.KeyFileConversion=b.KeyFileConversion)
	
	
	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE stepId = 7


	Alter table FileConversionInputWO drop column KeyConversionProcessVersion

	EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversionWO',@enabled = 0
	
END
Go
---------------------------------------------Step 7 Populate FileConversion End -------------------------------------
---------------------------------------------Step 8 Update CodexIntermediate Start -------------------------------------  
--EXEC Populate_fileconversioninputoutput_tbl_prc 1700000
IF EXISTS (Select Top 1 1 from sys.procedures where name like '%Populate_CodexIntermediateEndDate_prc%')
	Drop  Procedure Populate_CodexIntermediateEndDate_prc
Go
Create Procedure Populate_CodexIntermediateEndDate_prc

AS 
BEGIN

	INSERT INTO CodeMigrationScriptLog_tbl (
	stepId
	,stepName
	,startDate
	,enddate
	,InsertedDatetime
	)
VALUES (
	8
	,'Update SweeperProcessingCompleted Date'
	,GETDATE()
	,NULL
	,GETDATE()
	)
	

--select 1
--while 2>1
begin 
	IF OBJECT_ID('tempdb..#FC_batch_tbl') IS NOT NULL		
	DROP TABLE #FC_batch_tbl 
	

	select  top (10000) cxwt.KeyFileCollectionDetail, cxwt.keyfileversion 
		into #FC_batch_tbl
		from Codex_IR_00..CodexWorkingSet_tbl cxwt  WITH (NOLOCK)
				inner Join InternalUseOnly..CodexIntermediate cx WITH (NOLOCK)
					ON cx.KeyFileVersion = cxwt.KeyFileVersion
				where SweeperProcessingCompleted is null 




		Update cx
		set SweeperProcessingCompleted = getdate()
			FROM #FC_batch_tbl cxwt  WITH (NOLOCK)
				inner Join InternalUseOnly..CodexIntermediate cx  ON cx.KeyFileVersion = cxwt.KeyFileVersion
				where SweeperProcessingCompleted is null 

		
--	IF @@ROWCOUNT = 0 
	
--		BREAK

End 

	UPDATE CodeMigrationScriptLog_tbl
	SET enddate = GETDATE()
	WHERE stepId = 8

	If not exists (select count(*) from Codex_IR_00..CodexWorkingSet_tbl cxwt  WITH (NOLOCK)
				inner Join InternalUseOnly..CodexIntermediate cx WITH (NOLOCK)
					ON cx.KeyFileVersion = cxwt.KeyFileVersion
				where SweeperProcessingCompleted is null)
				
				begin 
					EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_CodexIntermediateEndDate',@enabled = 0
				
				end 

	END


---------------------------------------------Step 8 Update CodexIntermediate End -------------------------------------



USE [msdb]
GO

declare @job_id_DFP nvarchar (200) = null
set @job_id_DFP = (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'IR_Codex_Populate_CodexWorkingSet')

IF  @job_id_DFP is not null
EXEC msdb.dbo.sp_delete_job @job_id=@job_id_DFP, @delete_unused_schedule=1
GO


/****** Object:  Job [IR_Codex_Populate_FileCollectionDetailWO]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_CodexWorkingSet', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate File Collection Detail', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_CodexWorkingSet_prc', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO
EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_CodexWorkingSet',@enabled = 0
Go

declare @job_id_DFP nvarchar (200) = null
set @job_id_DFP = (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'IR_Codex_Populate_FileCollectionDetailWO')

IF  @job_id_DFP is not null
EXEC msdb.dbo.sp_delete_job @job_id=@job_id_DFP, @delete_unused_schedule=1
GO


/****** Object:  Job [IR_Codex_Populate_FileCollectionDetailWO]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_FileCollectionDetailWO', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'MHF\JITESHH', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate File Collection Detail', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_FileCollectionDetailWO_prc 10000', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Run Every 10 sec', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20220728, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'c67de6e7-442c-4172-9f60-db3974c06756'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO



GO


declare @job_id_DFP nvarchar (200) = null
set @job_id_DFP = (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'IR_Codex_Populate_FileVersionWO')

IF  @job_id_DFP is not null
EXEC msdb.dbo.sp_delete_job @job_id=@job_id_DFP, @delete_unused_schedule=1
GO


/****** Object:  Job [IR_Codex_Populate_FileCollectionDetailWO]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_FileVersionWO', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'MHF\JITESHH', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate File Collection Detail', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_FileCollectionVersionWO_prc 10000', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Run Every 10 sec', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20220728, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'c67de6e7-442c-4172-9f60-db3974c06756' 
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


GO


declare @job_id_DFP nvarchar (200) = null
set @job_id_DFP = (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'IR_Codex_Populate_fileconversioninputoutputWO')

IF  @job_id_DFP is not null
EXEC msdb.dbo.sp_delete_job @job_id=@job_id_DFP, @delete_unused_schedule=1
GO


/****** Object:  Job [IR_Codex_Populate_FileCollectionDetailWO]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_fileconversioninputoutputWO', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'MHF\JITESHH', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate File Collection Detail', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_fileconversioninputoutput_tbl_prc 10000', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Run Every 10 sec', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20220728, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'c67de6e7-442c-4172-9f60-db3974c06756'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversioninputoutputWO',@enabled = 0
Go


declare @job_id_DFP nvarchar (200) = null
set @job_id_DFP = (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'IR_Codex_Populate_fileconversionWO')

IF  @job_id_DFP is not null
EXEC msdb.dbo.sp_delete_job @job_id=@job_id_DFP, @delete_unused_schedule=1
GO


/****** Object:  Job [IR_Codex_Populate_FileCollectionDetailWO]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_fileconversionWO', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'MHF\JITESHH', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate File Collection Detail', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_fileconversionWO_tbl_prc', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


EXEC msdb.dbo.sp_update_job @job_name='IR_Codex_Populate_fileconversionWO',@enabled = 0
Go




/****** Object:  Job [IR_Codex_Populate_CodexIntermediateEndDate]    Script Date: 7/28/2022 3:51:05 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Application Related]]    Script Date: 7/28/2022 3:51:07 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Application Related]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Application Related]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'IR_Codex_Populate_CodexIntermediateEndDate', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Application Related]', 
		@owner_login_name=N'MHF\JITESHH', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Populate File Collection Detail]    Script Date: 7/28/2022 3:51:11 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Populate Codex_Intermediate EndDate', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Populate_CodexIntermediateEndDate_prc', 
		@database_name=N'Codex_IR_00', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Run Every 10 sec', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20220728, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'c67de6e7-442c-4172-9f60-db3974c06756'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO





--Codex_IR_00 restored on all the subscribers  
--		-> only be staging 
--		-> Historcal indexing in Solr in 

--Before -> 

--Codex_IR_00
--All subscriber 
--Except SNLDB2 and AVRPTSQL
----Filetr to Stop for attunity will restrict any data of IR moving to Attunity -> SNLDB2 -> 



--DocsFileCollectionDetail -> 9 million
--DocsFileVersion -> 45 million
--DocsFileConversion -> 45 million
--DocsFileConversionInput -> 45 million
--DocsFileConversionOutput -> 45 million
----10 days
--->