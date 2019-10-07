
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/06/2019 00:33:10
-- Generated from EDMX file: C:\Users\evadm\Source\Repos\reportissue\ReportIssue\RIDataModel.edmx
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
    [Fixed] nvarchar(max)  NOT NULL,
    [Status] nvarchar(max)  NOT NULL
);

-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [PK_Filters]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Issues'

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------