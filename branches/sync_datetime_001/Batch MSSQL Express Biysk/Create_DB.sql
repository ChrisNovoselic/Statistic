USE [master]
GO
USE [master]
GO

/****** Object:  Database [techsite-2.X.X]    Script Date: 11/28/2014 15:44:03 ******/
CREATE DATABASE [techsite-2.X.X] ON  PRIMARY 
( NAME = N'techsite-2.X.X', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\techsite-2.X.X.mdf' , SIZE = 923904KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'techsite-2.X.X_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\techsite-2.X.X_log.ldf' , SIZE = 688384KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [techsite-2.X.X].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [techsite-2.X.X] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET ARITHABORT OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [techsite-2.X.X] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [techsite-2.X.X] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [techsite-2.X.X] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET  DISABLE_BROKER 
GO

ALTER DATABASE [techsite-2.X.X] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [techsite-2.X.X] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [techsite-2.X.X] SET  READ_WRITE 
GO

ALTER DATABASE [techsite-2.X.X] SET RECOVERY FULL 
GO

ALTER DATABASE [techsite-2.X.X] SET  MULTI_USER 
GO

ALTER DATABASE [techsite-2.X.X] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [techsite-2.X.X] SET DB_CHAINING OFF 
GO

