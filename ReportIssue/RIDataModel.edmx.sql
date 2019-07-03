
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/03/2019 18:29:23
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

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Filters'
CREATE TABLE [dbo].[Filters] (
    [Id] nvarchar(256)  NOT NULL,
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
    [Id] nchar(256)  NOT NULL,
    [Template] nvarchar(max)  NOT NULL,
    [UpdateTime] datetime  NOT NULL,
    [Parameter1] nvarchar(max)  NOT NULL,
    [Parameter2] nvarchar(max)  NOT NULL,
    [Parameter3] nvarchar(max)  NOT NULL,
    [Parameter4] nvarchar(max)  NOT NULL,
    [Parameter5] nvarchar(max)  NOT NULL,
    [Parameter6] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [PK_Filters]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Issues'
ALTER TABLE [dbo].[Issues]
ADD CONSTRAINT [PK_Issues]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------