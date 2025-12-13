-- =============================================
-- Script: CreateSurveyLocationCableCountTable.sql
-- Description: Creates table to store global cable count per survey location
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurveyLocationCableCount]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SurveyLocationCableCount](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SurveyID] [bigint] NOT NULL,
        [LocID] [int] NOT NULL,
        [CableCount] [nvarchar](100) NULL,
        [Remarks] [nvarchar](500) NULL,
        [CreatedBy] [int] NULL,
        [CreatedDate] [datetime] NULL DEFAULT GETDATE(),
        [ModifiedBy] [int] NULL,
        [ModifiedDate] [datetime] NULL,
        CONSTRAINT [PK_SurveyLocationCableCount] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    -- Create index for faster lookups
    CREATE NONCLUSTERED INDEX [IX_SurveyLocationCableCount_SurveyLoc] 
    ON [dbo].[SurveyLocationCableCount] ([SurveyID], [LocID])

    PRINT 'Table SurveyLocationCableCount created successfully.'
END
ELSE
BEGIN
    PRINT 'Table SurveyLocationCableCount already exists.'
END
GO
