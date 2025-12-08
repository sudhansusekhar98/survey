-- =============================================
-- EXECUTE THIS ON REMOTE SERVER: 10.0.32.135
-- Database: VLDev
-- Purpose: Auto-assign team leader on survey creation using existing @EmpID parameter
-- =============================================
-- Command: sqlcmd -S 10.0.32.135 -U adminrole -P @dminr0le -d VLDev -i SQL_Add_SurveyTeamId_AutoAssignment.sql
-- =============================================

USE [VLDev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[SpSurvey]
( 
	 @SpType int = null
	,@SurveyName varchar(100) =null
	,@ImplementationType  varchar(100) =null
	,@SurveyDate  varchar(100) =null
	,@SurveyTeamName  varchar(100) =null
	,@SurveyTeamContact  varchar(100) =null
	,@AgencyName  varchar(100) =null
	,@LocationSiteName  varchar(100) =null
	,@CityDistrict  varchar(100) =null
	,@RegionID  int =null
	,@ScopeOfWork  varchar(100) =null
	,@Latitude  varchar(100) =null
	,@Longitude  varchar(100) =null
	,@MapMarking  varchar(max) =null
	,@SurveyStatus varchar(50) = null
	,@SurveyID numeric(11,0) = null
	,@EmpID int = null
	,@CreatedBy  int =null
	,@LocName varchar(100) = null
	,@LocLat decimal(18,10) = null
	,@LocLog decimal(18,10) = null
	,@LocID int = null
	,@ItemTypeID int = null
	,@ItemQtyExist int = null
	,@ItemQtyReq int = null
	,@ImgPath varchar(50) = null
	,@ImgID varchar(50) = null
	,@Remarks varchar(300) = null
	,@Isactive char = null
	,@IsAssigned int = null
	,@TransID int = null
	,@DueDate date = null
	,@LoginID VARCHAR(20)=null
	,@RegionName VARCHAR(100)=null
	,@LocationType VARCHAR(150)=null
	,@ClientID INT = null
	,@ClientName VARCHAR(150)=null
	,@ClientType VARCHAR(150)=null
	,@ContactPerson VARCHAR(150)=null
	,@ContactNumber VARCHAR(20) = null
	,@Address1 VARCHAR(150)=null
	,@Address3 VARCHAR(150)=null
	,@State VARCHAR(150)=null
	,@City VARCHAR(150)=null
)
AS
BEGIN TRY
 BEGIN TRANSACTION
	--- insert Survey ---

	if @SpType = 1
	begin
		declare  @SurId numeric(11,0)
		Declare @Dcnt int  
		select @Dcnt=count(*) + 1 from Survey where SurveyDate = cast(GETDATE() as date)
		
		set @SurId = cast(cast(CONVERT(VARCHAR(8), GETDATE(), 112) as varchar) + RIGHT(REPLICATE('0', 3) + cast(@Dcnt as varchar), 3) as numeric(11,0))

		INSERT INTO dbo.Survey (SurveyId,SurveyName,ImplementationType,SurveyDate
		,SurveyTeamName,SurveyTeamContact,AgencyName,LocationSiteName,CityDistrict
		,RegionID,ScopeOfWork,Latitude,Longitude,MapMarking,SurveyStatus,CreatedBy,ClientID)
		  values
		  (@SurId,@SurveyName,@ImplementationType,@SurveyDate
		,@SurveyTeamName,@SurveyTeamContact,@AgencyName,@LocationSiteName,@CityDistrict
		,@RegionID,@ScopeOfWork,@Latitude,@Longitude,@MapMarking,'Created',@CreatedBy,@ClientID)
		
		-- AUTO-ASSIGN: If EmpID (team leader) is provided, automatically add to SurveyAssignment
		IF @EmpID IS NOT NULL
		BEGIN
			INSERT INTO SurveyAssignment (SurveyID, EmpID, CreateBy, CreateOn)
			VALUES (@SurId, @EmpID, @CreatedBy, SYSDATETIME())
		END
	end

	--- Survey Detail List -------------

	if @SpType = 2
	begin
	SELECT SurveyId,SurveyName,ImplementationType,
	SurveyDate,SurveyTeamName,SurveyTeamContact,
	AgencyName,LocationSiteName,CityDistrict,
	ScopeOfWork,Latitude,Longitude,MapMarking,
	SurveyStatus,s.RegionID,
	ISNULL(r.RegionName, '') AS RegionName,
	s.CreatedBy,
	s.ClientID,
	ISNULL(c.ClientName, '') AS ClientName
	FROM dbo.Survey s
	LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
	LEFT JOIN dbo.ClientMaster c ON s.ClientID = c.ClientID
	where SurveyId in (select Distinct surveyID from Fn_SurveyListByUser(@CreatedBy))
	ORDER BY s.SurveyId DESC
	end

	--- insert  SurveyAssignment -------------
	if @SpType = 3
	begin
		insert into SurveyAssignment (SurveyID,EmpID,CreateBy,CreateOn)
		values (@SurveyID,@EmpID,@CreatedBy,SYSDATETIME())
	end
	
	--- Emp Master List -------------
	if @SpType = 4
	begin
		select EmpID,EmpCode,EmpName
		,case Gender when 'M' then 'Male' else 'Female' end Gender
		,MobileNo,Email,AddressLine1,AddressLine2,City,State,Country,PinCode,A.DeptID
		,DeptName,Designation,EmploymentType
		from EmpMaster A, DeptMaster B where A.DeptID=B.DeptID and A.IsActive=1
	end

	--- Insert Survey Locations List -------------
	if @SpType = 5
	begin
		insert into SurveyLocation (SurveyID, LocName,LocationType, LocLat, LocLog, CreateOn, CreateBy)
		values (@SurveyID, @LocName,@LocationType, @LocLat, @LocLog, SYSDATETIME(), @CreatedBy)
		
		update Survey set SurveyStatus='In Progress' where SurveyId=@SurveyID
	end

	--- Insert Survey Details List -------------
	if @SpType = 6
	begin
		insert into SurveyDetails (SurveyID,LocID, ItemTypeID,ItemQtyExist,ItemQtyReq,ImgPath,ImgID,Remarks,Isactive,CreateOn,CreateBy)
		values (@SurveyID,@LocID, @ItemTypeID,@ItemQtyExist,@ItemQtyReq,@ImgPath,@ImgID,@Remarks,'Y',SYSDATETIME(),@CreatedBy)
	end

	--- Survey Single Row  -------------
	if @SpType = 7
	begin
		SELECT SurveyId,SurveyName,ImplementationType,
		SurveyDate,SurveyTeamName,SurveyTeamContact,
		AgencyName,LocationSiteName,CityDistrict,
		ScopeOfWork,Latitude,Longitude,MapMarking,
		SurveyStatus,s.RegionID,
		ISNULL(r.RegionName, '') AS RegionName,
		s.CreatedBy,
		s.ClientID,
		ISNULL(c.ClientName, '') AS ClientName
		FROM dbo.Survey s
		LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
		LEFT JOIN dbo.ClientMaster c ON s.ClientID = c.ClientID
		where SurveyId = @SurveyID
	end

	--- Survey Loc List -------------
	if @SpType = 8
	begin
		SELECT LocId,SurveyID,LocName,LocLat,LocLog,LocationType
		FROM dbo.SurveyLocation
		where SurveyId=@SurveyID
	end


	--- Survey Assign List -------------
	if @SpType = 9
	begin
		SELECT A.SurveyID,A.EmpID,B.EmpName,B.MobileNo as Phone,A.DueDate
		FROM dbo.SurveyAssignment A, dbo.EmpMaster B
		where SurveyId=@SurveyID  and A.EmpID = B.EmpID
	end

	--- Survey Details List -------------
	if @SpType = 10
	begin
		SELECT TransID,A.SurveyID,A.LocID,B.LocName,A.ItemTypeID,C.ItemTypeName,
			   A.ItemQtyExist,A.ItemQtyReq,
			   ImgPath,A.ImgID,A.Remarks,A.Isactive,CreateOn,CreateBy,
			   IsComplete,
			   ISNULL(A.CamImgPath, '') AS CamImgPath,
			   ISNULL(A.CamImgID, '') AS CamImgID,
			   ISNULL(A.CamRemarks, '') AS CamRemarks,
			   A.CamUpdatedOn,
			   A.CamUpdatedBy
		FROM dbo.SurveyDetails A, dbo.SurveyLocation B, dbo.ItemTypeMaster C
		where A.SurveyId=@SurveyID  and A.LocID = B.LocId and A.ItemTypeID = C.ItemTypeID
	end


	--- Update Survey Status  -------------
	if @SpType = 11
	begin
		update dbo.Survey set SurveyStatus=@SurveyStatus where SurveyId=@SurveyID
	end


	--- Delete SurveyLocation and SurveyDetails -------------
	if @SpType = 12
	begin
		delete from SurveyDetails where LocID=@LocID
		delete from SurveyLocation where LocID=@LocID
	end


	--- Update Survey  -------------
	if @SpType = 13
	begin
		update dbo.Survey
		set SurveyName=@SurveyName
		,ImplementationType=@ImplementationType
		,SurveyDate=@SurveyDate
		,SurveyTeamName=@SurveyTeamName
		,SurveyTeamContact=@SurveyTeamContact
		,AgencyName=@AgencyName
		,LocationSiteName=@LocationSiteName
		,CityDistrict=@CityDistrict
		,ScopeOfWork=@ScopeOfWork
		,Latitude=@Latitude
		,Longitude=@Longitude
		,MapMarking=@MapMarking
		,RegionID=@RegionID
		,ClientID=@ClientID
		where SurveyId=@SurveyID
	end


	--- Update SurveyDetails  -------------
	if @SpType = 14
	begin
		update dbo.SurveyDetails
		set ItemQtyExist=@ItemQtyExist
		,ItemQtyReq=@ItemQtyReq
		,ImgPath=@ImgPath
		,ImgID=@ImgID
		,Remarks=@Remarks
		where TransID=@TransID
	end


	--- Survey Details List -------------
	if @SpType = 15
	begin
		SELECT TransID,A.SurveyID,A.LocID,B.LocName,A.ItemTypeID,C.ItemTypeName,
			   A.ItemQtyExist,A.ItemQtyReq,
			   ImgPath,A.ImgID,A.Remarks,A.Isactive,CreateOn,CreateBy,
			   IsComplete,
			   ISNULL(A.CamImgPath, '') AS CamImgPath,
			   ISNULL(A.CamImgID, '') AS CamImgID,
			   ISNULL(A.CamRemarks, '') AS CamRemarks,
			   A.CamUpdatedOn,
			   A.CamUpdatedBy
		FROM dbo.SurveyDetails A, dbo.SurveyLocation B, dbo.ItemTypeMaster C
		where A.SurveyId=@SurveyID  and A.LocID = @LocID and A.LocID = B.LocId and A.ItemTypeID = C.ItemTypeID
	end


	--- Get Survey Assignments  -------------
	if @SpType = 16
	begin
		SELECT sa.SurveyID, sa.EmpID, e.EmpName, e.MobileNo as Phone, sa.DueDate
		FROM dbo.SurveyAssignment sa
		INNER JOIN dbo.EmpMaster e ON sa.EmpID = e.EmpID
		WHERE sa.SurveyID = @SurveyID
	end


	--- Update Survey Assignment DueDate -------------
	if @SpType = 17
	begin
		UPDATE SurveyAssignment
		SET DueDate = @DueDate
		WHERE SurveyID = @SurveyID AND EmpID = @EmpID
	end


	--- Delete Survey Assignment -------------
	if @SpType = 18
	begin
		DELETE FROM SurveyAssignment
		WHERE SurveyID = @SurveyID AND EmpID = @EmpID
	end


	--- Update Survey Details (Camera Capture) -------------
	if @SpType = 19
	begin
		update dbo.SurveyDetails
		set CamImgPath=@ImgPath
		,CamImgID=@ImgID
		,CamRemarks=@Remarks
		,ItemQtyExist=@ItemQtyExist
		,CamUpdatedOn=SYSDATETIME()
		,CamUpdatedBy=@CreatedBy
		where TransID=@TransID
	end


	--- Update SurveyDetails IsComplete flag -------------
	if @SpType = 20
	begin
		update dbo.SurveyDetails
		set IsComplete=1
		where TransID=@TransID
	end


	--- Update SurveyLocation completion status -------------
	if @SpType = 21
	begin
		-- Mark all details for this location as complete
		UPDATE dbo.SurveyDetails
		SET IsComplete = 1
		WHERE LocID = @LocID AND SurveyID = @SurveyID
		
		-- Check if all locations in survey are complete
		DECLARE @TotalDetails INT
		DECLARE @CompletedDetails INT
		
		SELECT @TotalDetails = COUNT(*) FROM dbo.SurveyDetails WHERE SurveyID = @SurveyID
		SELECT @CompletedDetails = COUNT(*) FROM dbo.SurveyDetails WHERE SurveyID = @SurveyID AND IsComplete = 1
		
		IF @TotalDetails > 0 AND @TotalDetails = @CompletedDetails
		BEGIN
			UPDATE dbo.Survey SET SurveyStatus = 'Completed' WHERE SurveyId = @SurveyID
		END
	end


	--- Delete SurveyDetails by TransID -------------
	if @SpType = 22
	begin
		delete from SurveyDetails where TransID=@TransID
	end


	--- Survey Progress Calculation  -------------
	if @SpType = 23
	begin
		-- Calculate progress based on: Survey created (5%) + Locations added (30%) + Items added (30%) + Team assigned (5%) + Photos captured (30%)
		DECLARE @Progress INT = 0
		
		-- Survey exists = 5%
		IF EXISTS(SELECT 1 FROM Survey WHERE SurveyId = @SurveyID)
			SET @Progress = 5
		
		-- Has locations = 30%
		IF EXISTS(SELECT 1 FROM SurveyLocation WHERE SurveyId = @SurveyID)
			SET @Progress = @Progress + 30
		
		-- Has items = 30%
		IF EXISTS(SELECT 1 FROM SurveyDetails WHERE SurveyId = @SurveyID)
			SET @Progress = @Progress + 30
		
		-- Has team assigned = 5%
		IF EXISTS(SELECT 1 FROM SurveyAssignment WHERE SurveyId = @SurveyID)
			SET @Progress = @Progress + 5
		
		-- Calculate photo completion (up to 30%)
		DECLARE @TotalItems INT, @PhotoItems INT
		SELECT @TotalItems = COUNT(*) FROM SurveyDetails WHERE SurveyId = @SurveyID
		SELECT @PhotoItems = COUNT(*) FROM SurveyDetails WHERE SurveyId = @SurveyID AND CamImgPath IS NOT NULL AND CamImgPath != ''
		
		IF @TotalItems > 0
			SET @Progress = @Progress + ((@PhotoItems * 30) / @TotalItems)
		
		SELECT @Progress AS Progress
	end

	--- Insert Client Master -------------
	if @SpType = 24
	begin
		INSERT INTO dbo.ClientMaster (ClientName, ClientType, ContactPerson, ContactNumber, Address1, Address3, State, City, CreatedBy, CreatedOn)
		VALUES (@ClientName, @ClientType, @ContactPerson, @ContactNumber, @Address1, @Address3, @State, @City, @CreatedBy, SYSDATETIME())
	end


	--- Get All Clients -------------
	if @SpType = 25
	begin
		SELECT ClientID, ClientName, ClientType, ContactPerson, ContactNumber, Address1, Address3, State, City, IsActive
		FROM dbo.ClientMaster
		ORDER BY ClientName
	end


	--- Get Single Client -------------
	if @SpType = 26
	begin
		SELECT ClientID, ClientName, ClientType, ContactPerson, ContactNumber, Address1, Address3, State, City, IsActive
		FROM dbo.ClientMaster
		WHERE ClientID = @ClientID
	end


	--- Update Client Master -------------
	if @SpType = 27
	begin
		UPDATE dbo.ClientMaster
		SET ClientName = @ClientName,
			ClientType = @ClientType,
			ContactPerson = @ContactPerson,
			ContactNumber = @ContactNumber,
			Address1 = @Address1,
			Address3 = @Address3,
			State = @State,
			City = @City
		WHERE ClientID = @ClientID
	end


	--- Delete/Deactivate Client -------------
	if @SpType = 28
	begin
		UPDATE dbo.ClientMaster
		SET IsActive = 0
		WHERE ClientID = @ClientID
	end

COMMIT TRANSACTION
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
    DECLARE @ErrorState INT = ERROR_STATE()
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
END CATCH
GO

PRINT 'SpSurvey updated successfully with team leader auto-assignment logic using @EmpID'
GO
