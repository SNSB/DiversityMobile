USE [DiversityCollection]

SET LANGUAGE us_english

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON


DELETE FROM [dbo].[CollectionSpecimen]
DELETE FROM [dbo].[CollectionEventLocalisation]
DELETE FROM [dbo].[CollectionEvent]
DELETE FROM [dbo].[CollectionEventSeries]
DELETE FROM [dbo].[Analysis]
DELETE FROM [dbo].[ProjectAnalysis]
DELETE FROM [dbo].[AnalysisTaxonomicGroup]
DELETE FROM [dbo].[Property]
GO

DBCC CHECKIDENT ([CollectionEventSeries], RESEED, 0)
DBCC CHECKIDENT ([CollectionEvent], RESEED, 0)
DBCC CHECKIDENT ([CollectionSpecimen], RESEED, 0)
DBCC CHECKIDENT ([IdentificationUnit], RESEED, 0)
GO

DECLARE @specimenID integer
DECLARE @unitID integer
DECLARE @projectID integer
DECLARE @analysisID integer

SET @projectID = 0

INSERT INTO [dbo].[CollectionEventSeries]
           ([SeriesParentID]
           ,[Description]
           ,[SeriesCode]
           ,[Notes]
           ,[Geography]
           ,[DateStart]
           ,[DateEnd]
           ,[DateCache]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID])
     VALUES
           (NULL,
           'TestDescription',
           'TestCode',
           NULL,
           NULL,
           '2013-03-22 15:19:09.410',
           '2013-03-23 15:19:09.413',
           NULL,
           NULL,
           NULL,
           NULL,
           NULL,
           '999552C8-AC93-E211-9BD0-000C29BE0B84')
INSERT INTO [dbo].[CollectionEvent]
           ([Version]
           ,[SeriesID]
           ,[CollectorsEventNumber]
           ,[CollectionDate]
           ,[CollectionDay]
           ,[CollectionMonth]
           ,[CollectionYear]
           ,[CollectionDateSupplement]
           ,[CollectionDateCategory]
           ,[CollectionTime]
           ,[CollectionTimeSpan]
           ,[LocalityDescription]
           ,[HabitatDescription]
           ,[ReferenceTitle]
           ,[ReferenceURI]
           ,[ReferenceDetails]
           ,[CollectingMethod]
           ,[Notes]
           ,[CountryCache]
           ,[DataWithholdingReason]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID])
     VALUES
           (0
           ,@@identity
           ,NULL
           ,'2013-03-23 00:00:00.000'
           ,'23'
           ,'03'
           ,'2013'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,'TestLocality'
           ,'TestHabitat'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,'0F1BFD90-C593-E211-9BD0-000C29BE0B84')
INSERT INTO [dbo].[CollectionSpecimen]
           ([Version]
           ,[CollectionEventID]
           ,[CollectionID]
           ,[AccessionNumber]
           ,[AccessionDate]
           ,[AccessionDay]
           ,[AccessionMonth]
           ,[AccessionYear]
           ,[AccessionDateSupplement]
           ,[AccessionDateCategory]
           ,[DepositorsName]
           ,[DepositorsAgentURI]
           ,[DepositorsAccessionNumber]
           ,[LabelTitle]
           ,[LabelType]
           ,[LabelTranscriptionState]
           ,[LabelTranscriptionNotes]
           ,[ExsiccataURI]
           ,[ExsiccataAbbreviation]
           ,[OriginalNotes]
           ,[AdditionalNotes]
           ,[ReferenceTitle]
           ,[ReferenceURI]
           ,[ReferenceDetails]
           ,[Problems]
           ,[DataWithholdingReason]
           ,[InternalNotes]
           ,[ExternalDatasourceID]
           ,[ExternalIdentifier]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID])
     VALUES
           (0
           ,@@identity
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,'TestAccession'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NEWID())
		
SET @specimenID=@@IDENTITY

INSERT INTO [dbo].[IdentificationUnit]
           ([CollectionSpecimenID]
           ,[LastIdentificationCache]
           ,[FamilyCache]
           ,[OrderCache]
           ,[TaxonomicGroup]
           ,[OnlyObserved]
           ,[RelatedUnitID]
           ,[RelationType]
           ,[ColonisedSubstratePart]
           ,[LifeStage]
           ,[Gender]
           ,[NumberOfUnits]
           ,[ExsiccataNumber]
           ,[ExsiccataIdentification]
           ,[UnitIdentifier]
           ,[UnitDescription]
           ,[Circumstances]
           ,[DisplayOrder]
           ,[Notes]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID]
           ,[HierarchyCache])
     VALUES
           (@specimenID
           ,'TestCache'
           ,'TestFamily'
           ,'TestOrder'
           ,'plant'
           ,1
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,1
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NEWID()
           ,NULL)

SET @unitID = @@IDENTITY

INSERT INTO [dbo].[Identification]
           ([CollectionSpecimenID]
           ,[IdentificationUnitID]
           ,[IdentificationSequence]
           ,[IdentificationDate]
           ,[IdentificationDay]
           ,[IdentificationMonth]
           ,[IdentificationYear]
           ,[IdentificationDateSupplement]
           ,[IdentificationDateCategory]
           ,[VernacularTerm]
           ,[TaxonomicName]
           ,[NameURI]
           ,[IdentificationCategory]
           ,[IdentificationQualifier]
           ,[TypeStatus]
           ,[TypeNotes]
           ,[ReferenceTitle]
           ,[ReferenceURI]
           ,[ReferenceDetails]
           ,[Notes]
           ,[ResponsibleName]
           ,[ResponsibleAgentURI]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID])
     VALUES
           (@specimenID
           ,@unitID
           ,0
           ,'2013-03-23 00:00:00.000'
           ,'23'
           ,'03'
           ,'2013'
           ,NULL
           ,NULL
           ,NULL
           ,'TestIdentification'
           ,'TestURI'
           ,NULL
           ,'?'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NEWID())
INSERT INTO [dbo].[IdentificationUnit]
           ([CollectionSpecimenID]
           ,[LastIdentificationCache]
           ,[FamilyCache]
           ,[OrderCache]
           ,[TaxonomicGroup]
           ,[OnlyObserved]
           ,[RelatedUnitID]
           ,[RelationType]
           ,[ColonisedSubstratePart]
           ,[LifeStage]
           ,[Gender]
           ,[NumberOfUnits]
           ,[ExsiccataNumber]
           ,[ExsiccataIdentification]
           ,[UnitIdentifier]
           ,[UnitDescription]
           ,[Circumstances]
           ,[DisplayOrder]
           ,[Notes]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID]
           ,[HierarchyCache])
     VALUES
           (@specimenID
           ,'TestCache'
           ,'TestFamily'
           ,'TestOrder'
           ,'plant'
           ,1
           ,@unitID
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,1
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NEWID()
           ,NULL)

INSERT INTO [dbo].[Identification]
           ([CollectionSpecimenID]
           ,[IdentificationUnitID]
           ,[IdentificationSequence]
           ,[IdentificationDate]
           ,[IdentificationDay]
           ,[IdentificationMonth]
           ,[IdentificationYear]
           ,[IdentificationDateSupplement]
           ,[IdentificationDateCategory]
           ,[VernacularTerm]
           ,[TaxonomicName]
           ,[NameURI]
           ,[IdentificationCategory]
           ,[IdentificationQualifier]
           ,[TypeStatus]
           ,[TypeNotes]
           ,[ReferenceTitle]
           ,[ReferenceURI]
           ,[ReferenceDetails]
           ,[Notes]
           ,[ResponsibleName]
           ,[ResponsibleAgentURI]
           ,[LogCreatedWhen]
           ,[LogCreatedBy]
           ,[LogUpdatedWhen]
           ,[LogUpdatedBy]
           ,[RowGUID])
     VALUES
           (@specimenID
           ,@@IDENTITY
           ,0
           ,'2013-03-23 00:00:00.000'
           ,'23'
           ,'03'
           ,'2013'
           ,NULL
           ,NULL
           ,NULL
           ,'TestIdentification'
           ,'TestURI'
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NEWID())

INSERT INTO [dbo].[Analysis]
           ([AnalysisParentID]
           ,[DisplayText]
           ,[Description]
           ,[MeasurementUnit]
           ,[Notes]
           ,[AnalysisURI]
           ,[OnlyHierarchy]
           ,[RowGUID])
     VALUES
           (NULL
           ,'TestAnalysis'
           ,'TestAnalysisDescription'
           ,'buckets'
           ,NULL
           ,'TestURI'
           ,0
           ,NEWID())

SET @analysisID = @@IDENTITY

INSERT INTO [dbo].[ProjectAnalysis]
           ([AnalysisID]
           ,[ProjectID]
           ,[RowGUID])
     VALUES
           (@analysisID
           ,@projectID
           ,NEWID())


INSERT INTO [dbo].[AnalysisTaxonomicGroup]
           ([AnalysisID]
           ,[TaxonomicGroup]
           ,[RowGUID])
     VALUES
           (@analysisID
           ,'plant'
           ,NEWID())
GO

USE [DiversityMobile]

IF OBJECT_ID('[dbo].[DiversityScientificTerms]','U') IS NOT NULL
DROP TABLE [dbo].[DiversityScientificTerms]
GO

CREATE TABLE [dbo].[DiversityScientificTerms](
	[PropertyID] [int] NOT NULL,
	[PropertyURI] [nvarchar](285) NULL,
	[DisplayText] [nvarchar](400) NOT NULL,
	[HierarchyCache] [nvarchar](400) NULL,
	[TermID] [int] NOT NULL,
	[BroaderTermID] [int] NULL,
 CONSTRAINT [PK_DiversityScientificTerms] PRIMARY KEY CLUSTERED 
(
	[PropertyID] ASC,
	[TermID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

GRANT SELECT ON [dbo].[DiversityScientificTerms] TO [DiversityMobileUser]
GO




IF OBJECT_ID('[dbo].[LebensraumTypen]','V') IS NOT NULL
DROP VIEW [dbo].[LebensraumTypen]
/*EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LebensraumTypen'

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LebensraumTypen'*/


GO

CREATE VIEW [dbo].[LebensraumTypen]
AS
SELECT     T.PropertyID, T.PropertyURI, T.DisplayText, T.HierarchyCache, T.TermID, T.BroaderTermID
FROM         [dbo].[DiversityScientificTerms] AS T
GO

GRANT SELECT ON [dbo].[LebensraumTypen] TO [DiversityMobileUser]
GO


DECLARE @propertyID integer

SET @propertyID = 0

DELETE FROM [dbo].[TermsLists]
DELETE FROM [dbo].[UserTermsLists]

INSERT INTO [dbo].[TermsLists]
           ([PropertyID]
           ,[Datasource])
     VALUES
           (@propertyID
           ,'Lebensraumtypen')



INSERT INTO [dbo].[UserTermsLists]
           ([Login]
           ,[PropertyID])
     VALUES
           ('dwb'
           ,@propertyID)

INSERT INTO [dbo].[UserTermsLists]
           ([Login]
           ,[PropertyID])
     VALUES
           ('test'
           ,@propertyID)

INSERT INTO [dbo].[DiversityScientificTerms]
           ([PropertyID]
           ,[PropertyURI]
           ,[DisplayText]
           ,[HierarchyCache]
           ,[TermID]
           ,[BroaderTermID])
     VALUES
           (@propertyID
           ,'TestPropertyUri1'
           ,'PropertyValue1'
           ,NULL
           ,0
           ,NULL),
		   (@propertyID
           ,'TestPropertyUri2'
           ,'PropertyValue2'
           ,NULL
           ,1
           ,NULL),
		   (@propertyID
           ,'TestPropertyUri3'
           ,'PropertyValue3'
           ,NULL
           ,2
           ,NULL)

USE [DiversityCollection]

INSERT INTO [dbo].[Property]
           ([PropertyID]
		   ,[PropertyParentID]
           ,[PropertyName]
           ,[DefaultAccuracyOfProperty]
           ,[DefaultMeasurementUnit]
           ,[ParsingMethodName]
           ,[DisplayText]
           ,[DisplayEnabled]
           ,[DisplayOrder]
           ,[Description]
           ,[RowGUID])
     VALUES
           (@propertyID
		   ,NULL
           ,'TestProperty'
           ,NULL
           ,'acres'
           ,'TestParsingMethod'
           ,'TestPropertyDisplay'
           ,1
           ,0
           ,'TestDescription'
           ,NEWID())
		   