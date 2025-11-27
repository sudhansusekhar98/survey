-- =============================================
-- EXECUTE THIS ON REMOTE SERVER: 10.0.32.135
-- Database: VLDev
-- User: adminrole
-- =============================================
-- Command: sqlcmd -S 10.0.32.135 -U adminrole -P @dminr0le -d VLDev -i RestoreSpSurvey.sql
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
	,@ContactNumber VARCHAR(20) = null  -- FIXED: Changed from INT to VARCHAR(20)
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
		,RegionID,ScopeOfWork,Latitude,Longitude,MapMarking,SurveyStatus,CreatedBy)
		  values
		  (@SurId,@SurveyName,@ImplementationType,@SurveyDate
		,@SurveyTeamName,@SurveyTeamContact,@AgencyName,@LocationSiteName,@CityDistrict
		,@RegionID,@ScopeOfWork,@Latitude,@Longitude,@MapMarking,'Created',@CreatedBy)
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
	s.CreatedBy
	FROM dbo.Survey s
	LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
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
		insert into SurveyDetails (SurveyID, LocID, ItemTypeID, ItemQtyExist, ItemQtyReq, ImgPath, ImgID, Remarks, CreateOn, CreateBy)
		values (@SurveyID, @LocID, @ItemTypeID, @ItemQtyExist, @ItemQtyReq, @ImgPath, @ImgID, @Remarks, SYSDATETIME(), @CreatedBy)
	end

	if @SpType = 7
	begin
		SELECT 
        s.SurveyId,
        s.SurveyName,
        s.ImplementationType,
        s.SurveyDate,
        s.SurveyTeamName,
        s.SurveyTeamContact,
        s.AgencyName,
        s.LocationSiteName,
        s.CityDistrict,
        s.ScopeOfWork,
        s.Latitude,
        s.Longitude,
        s.MapMarking,
        s.SurveyStatus,
        s.RegionID,
        ISNULL(r.RegionName, '') AS RegionName,
        s.CreatedBy
    FROM dbo.Survey s
    LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
    WHERE s.SurveyId = @SurveyId
	end

	--- Update Survey Details List -------------

	if @SpType = 8
	begin
		UPDATE Survey 
                SET SurveyName = @SurveyName,
                    ImplementationType = @ImplementationType,
                    SurveyDate = @SurveyDate,
                    SurveyTeamName = @SurveyTeamName,
                    SurveyTeamContact = @SurveyTeamContact,
                    AgencyName = @AgencyName,
                    LocationSiteName = @LocationSiteName,
                    CityDistrict = @CityDistrict,
                    RegionID = @RegionID,
                    ScopeOfWork = @ScopeOfWork,
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    MapMarking = @MapMarking,
                    SurveyStatus = @SurveyStatus,
                    CreatedBy = @CreatedBy
                WHERE SurveyId = @SurveyId
	end

	--- Get Survey Locations List -------------
	
	IF @SpType = 9
	BEGIN
		SELECT SurveyID, LocID, LocName, LocationType, LocLat, LocLog, CreateOn, CreateBy, 
			   CASE Isactive WHEN 'Y' THEN 1 ELSE 0 END Isactive
		FROM dbo.SurveyLocation
		WHERE SurveyID = @SurveyID
		ORDER BY CreateOn DESC
	END

	--- insert device types -------------
	if @SpType = 10
	begin
		if exists (select * from AssignedItems where SurveyId=@SurveyID and LocID=@LocID and ItemTypeID=@ItemTypeID)
		begin
			update AssignedItems set IsAssigned = @IsAssigned where SurveyId=@SurveyID and LocID=@LocID and ItemTypeID=@ItemTypeID
		end
		else 
		begin
			INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID,TypeName,IsAssigned, CreatedOn, CreatedBy)
			VALUES (@SurveyID, @LocID, @ItemTypeID,(Select TypeName from ItemTypeMaster where Id=@ItemTypeID),@IsAssigned, SYSDATETIME(), @CreatedBy)
		end
	end

	--- Delete Survey Locations List -------------
	BEGIN
		SET NOCOUNT OFF;

		IF (@SpType = 11)
		BEGIN
			DELETE FROM SurveyLocation
			WHERE LocID = @LocID;
		END
	END
	
	--- Update Survey Locations List -------------
	BEGIN
		SET NOCOUNT OFF;
		IF (@SpType = 12)
		BEGIN
		  UPDATE SurveyLocation
			SET 
				SurveyID = @SurveyID,
				LocName = @LocName,
				LocationType = @LocationType,
				LocLat = @LocLat,
				LocLog = @LocLog,
				Isactive = CASE 
							   WHEN @Isactive IN ('true', 'True', '1', 'Y', 'y') THEN 'Y' 
							   ELSE 'N' 
						   END
			WHERE LocID = @LocID;
		END
	END
	
	--- Get Survey Item Type Master List -------------

	if @SpType = 13
	begin
		SELECT
		id,
		TypeName,
		TypeDesc,
		GroupName
		FROM
		ItemTypeMaster
		WHERE
		IsActive = 1
	end

	-- delete AssignedItems ---

	if @SpType = 14
	begin
		delete from AssignedItems where SurveyId=@SurveyID and LocID=@LocID 
	end
	
	-- getAssignedItems ---
	if @SpType = 15
	begin
		select Id ItemTypeID,A.TypeName,TypeDesc,GroupName
		,isnull(IsAssigned,0) IsAssigned
		from ItemTypeMaster A left join 
		(select ItemTypeID,IsAssigned  from AssignedItems where SurveyId=@SurveyID and LocID=@LocID) B on A.Id=B.ItemTypeID
		where A.IsActive=1
	end

	--Get Survey Assignment --
	if @SpType = 16
	begin
		SELECT 
			sa.TransID,
			sa.SurveyID,
			sa.EmpID,
			ISNULL(e.EmpName, '') AS EmpName,
			sa.DueDate,
			sa.CreateBy,
			sa.CreateOn
		FROM dbo.SurveyAssignment sa
		LEFT JOIN dbo.EmpMaster e ON sa.EmpID = e.EmpID
		WHERE sa.SurveyID = @SurveyID
		ORDER BY sa.TransID DESC
	end

	--Assign a Survey--
	if @SpType = 17
	begin 
		INSERT INTO dbo.SurveyAssignment (SurveyID, EmpID, CreateBy, CreateOn, DueDate)
		VALUES (@SurveyID, @EmpID, @CreatedBy, GETDATE(), @DueDate);

		update Survey set SurveyStatus='Assigned' where SurveyId=@SurveyID
	end

	-- SpType 18: Update Survey Assignment

	IF @SpType = 18
	BEGIN
		UPDATE SurveyAssignment
		SET 
			EmpID = @EmpID,
			DueDate = @DueDate
		WHERE TransID = @TransID
		  AND SurveyID = @SurveyID
	END

	-- SpType 19: Delete Survey Assignment
	IF @SpType = 19
	BEGIN
		DELETE FROM SurveyAssignment
		WHERE TransID = @TransID
	END

	-- SpType 20: Survey Status
	IF @SpType = 20
	BEGIN
		select Sno,M.SurveyStatus,isnull(cnt,0) RecordCnt from 
		(select Sno,SurveyStatus from SurveyStatusData)M Left Join
		(select SurveyStatus,count(*) Cnt from Survey
		group by SurveyStatus)A on M.SurveyStatus = A.SurveyStatus
	END

	-- 1. INSERT CLIENT
	IF(@SpType = 21)
	BEGIN
		INSERT INTO ClientMaster
		(
			ClientName, ClientType, Address1, Address3,
			State, City, ContactPerson, ContactNumber,
			Isactive, CreatedOn, CreatedBy
		)
		VALUES
		(
			@ClientName, @ClientType, @Address1, @Address3,
			@State, @City, @ContactPerson, @ContactNumber,
			1, GETDATE(), @CreatedBy
		);

		SELECT SCOPE_IDENTITY() AS NewClientID;
	END

	-- 2. UPDATE CLIENT
	IF(@SpType = 22)
	BEGIN
		UPDATE ClientMaster
		SET 
			ClientName = @ClientName,
			ClientType = @ClientType,
			Address1 = @Address1,
			Address3 = @Address3,
			State = @State,
			City = @City,
			ContactPerson = @ContactPerson,
			ContactNumber = @ContactNumber
		WHERE ClientID = @ClientID;

		SELECT 'Updated Successfully' AS Message;
	END

	-- 3. DELETE (Soft delete)
	IF(@SpType = 23)
	BEGIN
		UPDATE ClientMaster
		SET Isactive = 0
		WHERE ClientID = @ClientID;

		SELECT 'Deleted Successfully' AS Message;
	END

	-- 4. GET / SELECT
	IF(@SpType = 24)
	BEGIN
		IF(@ClientID IS NOT NULL)
		BEGIN
			SELECT * FROM ClientMaster WHERE ClientID = @ClientID;
		END
		ELSE 
		BEGIN
			SELECT * FROM ClientMaster WHERE Isactive = 1;
		END
	END

	 -- Survey Progress Percentage 
	IF(@SpType = 25)
	BEGIN
	   SELECT
		Percentage = CASE
			WHEN TotalPoints >= 100 THEN 100
			WHEN TotalPoints < 1    THEN 1
			ELSE TotalPoints
		END
	FROM (
		SELECT
			-- +5 for assignment if any
			(CASE WHEN EXISTS(SELECT 1 FROM SurveyAssignment WHERE SurveyId = @SurveyId) THEN 5 ELSE 0 END) +
			-- +5 for location if any
			(CASE WHEN EXISTS(SELECT 1 FROM SurveyLocation WHERE SurveyId = @SurveyId) THEN 5 ELSE 0 END) +
			-- status-based points 
			(CASE WHEN L.locCnt = 0 THEN 0 ELSE (CAST(90 / L.locCnt AS INT) * S.statCnt) END) AS TotalPoints
		FROM
			(SELECT COUNT(*) AS locCnt FROM SurveyLocation WHERE SurveyId = @SurveyId) L
		CROSS JOIN
			(SELECT COUNT(*) AS statCnt FROM SurveyLocationStatus WHERE SurveyId = @SurveyId) S
	) T;
	END

 COMMIT
 END TRY                                                              
 BEGIN CATCH                                                                
 ROLLBACK                                                                            
 DECLARE @ErrorMessage NVARCHAR(4000);
 SELECT @ErrorMessage = ERROR_MESSAGE();                                                                            
 RAISERROR (@ErrorMessage, 16, 1);                                                                            
 END CATCH
GO

PRINT '======================================================'
PRINT 'SpSurvey restored successfully!'
PRINT 'ContactNumber parameter is now VARCHAR(20)'
PRINT 'All ClientMaster operations (SpType 21-24) are ready'
PRINT '======================================================'
GO
