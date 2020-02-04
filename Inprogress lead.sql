
/* =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
 exec InProgressLeadDetails @UserId = 2
-- ============================================= */
CREATE PROCEDURE  [dbo].[InProgressLeadDetails]
(
  @UserId bigInt =0
)
AS
BEGIN
	 
	  select  fd.AssignLeadId, fd.LeadStatus,flp.FollowUp_Comment as [FollowUpComment],fd.Date, lm.SENDERNAME as [SenderName]
	  ,lm.SENDEREMAIL as [SenderEmail],lm.MOB as [Mob],lm.SUBJECT as [Subject] ,lm.Id,us.FirstName +' ' + us.LastName as [UserName] from LeadMaster lm 
	   inner join (
	   select * from (
					select ls.AssignLeadId,ls.FK_LeadId,ls.Fk_UserId ,ls.LeadStatus ,max(lfd.Pk_LeadFollowUp_Id) as [Pk_LeadFollowUp_Id]
					,max(case when lfd.FollowUp_Date > ls.AssignDate then lfd.FollowUp_Date else ls.AssignDate end) as [Date]
					from AssingLeads ls 
					left join LeadFollowUpDetails lfd on ls.AssignLeadId = lfd.Fk_AssignLead_Id
					group by ls.AssignLeadId,ls.FK_LeadId,ls.Fk_UserId,ls.LeadStatus
					) lfd where lfd.LeadStatus <> 4 and lfd.LeadStatus <>6 and lfd.Fk_UserId =CASE WHEN @UserId = 0 THEN lfd.Fk_UserId ELSE @UserId END
				) as fd  on fd.FK_LeadId = lm.Id
	 inner join Users us on us.UserId = fd.Fk_UserId 
	 left join dbo.LeadFollowUpDetails flp on fd.Pk_LeadFollowUp_Id = flp.Pk_LeadFollowUp_Id
	--order by fd.Date asc

END

GO
