using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadManagment.WebUI
{
    [Area("Master")]
    [Authorize(Roles = "Admin,User")]
    public class DashboardController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public IActionResult TodayScheduleFollowUp(DateTime? selectedDate)
        {
            var date = selectedDate.HasValue ? selectedDate.Value.Date : DateTime.Now.Date;
            if (User.IsInRole("Admin"))
            {

                var todaySchedule = (from al in _dbContext.AssingLeads
                                     where (al.LeadStatus != 4 && al.LeadStatus != 6 && al.LeadStatus != 3)
                                     join fd in (from ffd in _dbContext.LeadFollowUpDetails
                                                 where (ffd.FollowUpDate != null && ffd.FollowUpDate.Value.Date == date)
                                                 group ffd by ffd.FkAssignLeadId into gg
                                                 select new
                                                 {
                                                     assignId = gg.Key,
                                                     fid = gg.Max(x => x.PkLeadFollowUpId)
                                                 }) on al.AssignLeadId equals fd.assignId
                                     join lm in _dbContext.LeadMaster on al.FkLeadId equals lm.Id
                                     join us in _dbContext.Users on al.FkUserId equals us.UserId
                                     join foll in _dbContext.LeadFollowUpDetails on fd.fid equals foll.PkLeadFollowUpId
                                     select new
                                     {

                                         name = $"{us.FirstName } {us.LastName } ",
                                         senderName = lm.Sendername,
                                         lastComment = foll.FollowUpComment ?? string.Empty,
                                         time = foll.FollowUpDate.HasValue ? foll.FollowUpDate.Value.ToString("HH:mm") : "[N/A]",
                                     }).ToList();
                return Ok(todaySchedule);
            }
            else
            {
                var todaySchedule = (from al in _dbContext.AssingLeads
                                     where (al.FkUserId == Convert.ToInt64(User.FindFirstValue("UserId")) && (al.LeadStatus != 4 && al.LeadStatus != 6 && al.LeadStatus != 3))
                                     join fd in (from ffd in _dbContext.LeadFollowUpDetails
                                                 where (ffd.FollowUpDate != null && ffd.FollowUpDate.Value.Date == date)
                                                 group ffd by ffd.FkAssignLeadId into gg
                                                 select new
                                                 {
                                                     assignId = gg.Key,
                                                     fid = gg.Max(x => x.PkLeadFollowUpId)
                                                 }) on al.AssignLeadId equals fd.assignId
                                     join lm in _dbContext.LeadMaster on al.FkLeadId equals lm.Id
                                     join us in _dbContext.Users on al.FkUserId equals us.UserId
                                     join foll in _dbContext.LeadFollowUpDetails on fd.fid equals foll.PkLeadFollowUpId
                                     select new
                                     {
                                         name = $"{lm.Sendername }  [<a href='mailto:#'>{lm.Senderemail }</a>]",
                                         lastComment = foll.FollowUpComment ?? string.Empty,
                                         time = foll.FollowUpDate.HasValue ? foll.FollowUpDate.Value.ToString("HH:mm") : "[N/A]",
                                     }).ToList();
                return Ok(todaySchedule);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> TodayLeads()
        {
            //if (User.IsInRole("Admin"))
            //{
            //    var unassignLeads = await _dbContext.LeadMaster.Where(x => (x.Isassigned ?? false) == false).OrderByDescending(x => x.LogTime).ToListAsync();
            //    return Ok(unassignLeads);
            //}
            //else
            //{
            //    var todayLeads = await _dbContext.LeadMaster.Where(x => (x.CreateDate != null && x.CreateDate.Value.Date == DateTime.Now.Date) && (x.Isassigned ?? false) == false).ToListAsync();
            //    return Ok(todayLeads);
            //}
            var unassignLeads = await _dbContext.LeadMaster.Where(x => (x.Isassigned ?? false) == false).OrderByDescending(x => x.LogTime).ToListAsync();
            return Ok(unassignLeads);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> MissedFollowup()
        {
            if (User.IsInRole("Admin"))
            {
                var todaySchedule = (from al in _dbContext.AssingLeads
                                     where ((al.LeadStatus ?? 0) == 2 || (al.LeadStatus ?? 0) == 5)
                                     join fd in (from ffd in _dbContext.LeadFollowUpDetails
                                                 where (ffd.FollowUpDate != null && ffd.FollowUpDate.Value.Date < DateTime.Now.Date)
                                                 group ffd by ffd.FkAssignLeadId into gg
                                                 select new
                                                 {
                                                     assignId = gg.Key,
                                                     fid = gg.Max(x => x.PkLeadFollowUpId)
                                                 }) on al.AssignLeadId equals fd.assignId
                                     join lm in _dbContext.LeadMaster on al.FkLeadId equals lm.Id
                                     join us in _dbContext.Users on al.FkUserId equals us.UserId
                                     join foll in _dbContext.LeadFollowUpDetails on fd.fid equals foll.PkLeadFollowUpId 
                                     orderby foll.FollowUpDate descending
                                     select new
                                     {
                                         name = $"{us.FirstName } {us.LastName } ",
                                         senderName = lm.Sendername,
                                         time = foll.FollowUpDate.HasValue ? foll.FollowUpDate.Value.ToString("dd/MM/yyyy HH:mm") : "[N/A]",
                                     }).ToList();
                return Ok(todaySchedule);
            }
            else
            {
                var todaySchedule = (from al in _dbContext.AssingLeads
                                     where (((al.LeadStatus ?? 0) == 2 || (al.LeadStatus ?? 0) == 5) && al.FkUserId== Convert.ToInt64(User.FindFirstValue("UserId")))
                                     join fd in (from ffd in _dbContext.LeadFollowUpDetails
                                                 where (ffd.FollowUpDate != null && ffd.FollowUpDate.Value.Date < DateTime.Now.Date)
                                                 group ffd by ffd.FkAssignLeadId into gg
                                                 select new
                                                 {
                                                     assignId = gg.Key,
                                                     fid = gg.Max(x => x.PkLeadFollowUpId)
                                                 }) on al.AssignLeadId equals fd.assignId
                                     join lm in _dbContext.LeadMaster on al.FkLeadId equals lm.Id
                                     join us in _dbContext.Users on al.FkUserId equals us.UserId
                                     join foll in _dbContext.LeadFollowUpDetails on fd.fid equals foll.PkLeadFollowUpId
                                     orderby foll.FollowUpDate descending
                                     select new
                                     {
                                         name = lm.Sendername,
                                         lastComment = foll.FollowUpComment,
                                         time = foll.FollowUpDate.HasValue ? foll.FollowUpDate.Value.ToString("dd/MM/yyyy HH:mm") : "[N/A]",
                                     }).ToList();
                return Ok(todaySchedule);
            }
        }
    }
}