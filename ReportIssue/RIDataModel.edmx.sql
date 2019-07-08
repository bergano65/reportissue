
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/08/2019 22:50:33
-- Generated from EDMX file: C:\projects\ReportIssue\ReportIssue\RIDataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RIssue];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Filters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Filters];
GO
IF OBJECT_ID(N'[dbo].[Issues]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Issues];
GO
IF OBJECT_ID(N'[dbo].[Pictures]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Pictures];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Filters'
CREATE TABLE [dbo].[Filters] (
    [ID] nvarchar(256)  NOT NULL,
    [Issue] nvarchar(max)  NOT NULL,
    [Product] nvarchar(max)  NOT NULL,
    [Wrong] nvarchar(max)  NOT NULL,
    [Right] nvarchar(max)  NOT NULL,
    [Submitted] nvarchar(max)  NOT NULL,
    [Fixed] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Issues'
CREATE TABLE [dbo].[Issues] (
    [ID] nchar(256)  NOT NULL,
    [Template] nvarchar(max)  NULL,
    [UpdateTime] datetime  NULL,
    [Parameter1] nvarchar(max)  NOT NULL,
    [Parameter2] nvarchar(max)  NOT NULL,
    [Parameter3] nvarchar(max)  NULL,
    [Parameter4] nvarchar(max)  NULL,
    [Parameter5] nvarchar(max)  NULL,
    [Parameter6] nvarchar(max)  NULL,
    [Parameter7] nvarchar(max)  NULL,
    [Parameter8] nvarchar(max)  NOT NULL,
    [Parameter10] nvarchar(max)  NOT NULL,
    [Parameter9] nvarchar(max)  NOT NULL,
    [Parameter11] nvarchar(max)  NOT NULL,
    [Parameter12] nvarchar(max)  NOT NULL,
    [Parameter13] nvarchar(max)  NOT NULL,
    [Parameter14] nvarchar(max)  NOT NULL,
    [Parameter15] nvarchar(max)  NOT NULL,
    [Parameter16] nvarchar(max)  NOT NULL,
    [Parameter17] nvarchar(max)  NOT NULL,
    [Parameter18] nvarchar(max)  NOT NULL,
    [Parameter19] nvarchar(max)  NOT NULL,
    [Parameter20] nvarchar(max)  NOT NULL,
    [Url] nvarchar(max)  NOT NULL,
    [BugPath] nvarchar(max)  NOT NULL,
    [Fixed] bit  NOT NULL,
    [Submitted] bit  NOT NULL,
    [Product] nvarchar(max)  NOT NULL,
    [Reason] nvarchar(max)  NOT NULL,
    [Wrong] nvarchar(max)  NOT NULL,
    [WhereFound] nvarchar(max)  NOT NULL,
    [Right] nvarchar(max)  NOT NULL,
    [English] nvarchar(max)  NOT NULL,
    [IssueTxt] nvarchar(max)  NOT NULL,
    [PictureString] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Pictures'
CREATE TABLE [dbo].[Pictures] (
    [ID] nvarchar(256)  NOT NULL,
    [Bytes] varbinary(max)  NOT NULL,
    [MarkerString] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [PK_Filters]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Issues'
ALTER TABLE [dbo].[Issues]
ADD CONSTRAINT [PK_Issues]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Pictures'
ALTER TABLE [dbo].[Pictures]
ADD CONSTRAINT [PK_Pictures]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------