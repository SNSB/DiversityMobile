USE [DiversityCollection]
GO

SET LANGUAGE us_english



DELETE FROM [dbo].[CollectionSpecimen]
DELETE FROM [dbo].[CollectionEventLocalisation]
DELETE FROM [dbo].[CollectionEvent]
DELETE FROM [dbo].[CollectionEventSeries]
GO

DBCC CHECKIDENT ([CollectionEventSeries], RESEED, 0)
DBCC CHECKIDENT ([CollectionEvent], RESEED, 0)
DBCC CHECKIDENT ([CollectionSpecimen], RESEED, 0)
DBCC CHECKIDENT ([IdentificationUnit], RESEED, 0)
GO

DECLARE @specimenID integer
DECLARE @unitID integer

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
GO








