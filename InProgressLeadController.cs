using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace LeadManagment.WebUI
{
    /// <summary>
    /// 
    /// </summary>
    [Area("Master")]
    [Authorize(Roles = "Admin,User")]
    public class InProgressLeadController : BaseController
    {
        public IActionResult Index()
        {
            var model = new DataTableViewModel()
            {
                ID = "TblInProgressLead",
                PageTitle = "InProgress Lead",
                Headers = new List<string> { "no-sort_Id","FollowUp Status","FollowUp Remarks","Next FollowUp Date",
                                             "Sender Name", "Sender Email", "Mobile", "Subject","Quotation"
                                            // , "Enq Date Time"
                                             ,"no-sort_View"},
                JsonUrl = Url.Action("InProgressLeadDataTable", "InProgressLead", new { area = "Master" })
            };
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult InProgressLeadView()
        {
            var model = new DataTableViewModel()
            {
                ID = "TblInProgressLead",
                PageTitle = "InProgress Lead",
                Headers = new List<string> { "no-sort_Id","FollowUp Status","FollowUp Remarks","Next FollowUp Date",
                                             "Sender Name", "Sender Email", "Mobile", "Subject","Quotation"
                                            // , "Enq Date Time"
                                             ,"no-sort_View"},
                JsonUrl = Url.Action("InProgressLeadDataTable", "InProgressLead", new { area = "Master" })
            };
            return PartialView(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> InProgressLeadDataTable()
        {
            var assignLeads = await (from al in _dbContext.AssingLeads
                                     where (al.FkUserId == Convert.ToInt64(User.FindFirstValue("UserId")) &&  (al.LeadStatus !=4 && al.LeadStatus !=6))
                                     join lm in _dbContext.LeadMaster on al.FkLeadId equals lm.Id
                                     join us in _dbContext.Users on al.FkUserId equals us.UserId
                                     let fw = al.LeadFollowUpDetails.OrderByDescending(x => x.CreatedDate).FirstOrDefault()
                                     select new List<string>()
                             {
                                  "<a onclick=fnAddUpdate("+al.AssignLeadId+"); title='Follow-Up' class='btn btn-default btn-icon btn-circle btn-lg'>"+ al.AssignLeadId.ToString()+"</a>",
                                    al.LeadStatus==1?"Assigned"
                                                    :al.LeadStatus==2?"Inprogress"
                                                    :al.LeadStatus==3?"<a onclick=fnConvertToCustomer("+al.FkLeadId+"); title='Convert To Customer' class='btn btn-link'>Done</a>"
                                                    :al.LeadStatus==4?"Close"
                                                    :al.LeadStatus==5?"Hold"
                                                    :"Customer",
                                    !string.IsNullOrEmpty(fw.FollowUpComment)?fw.FollowUpComment.Length>100?
                                    "<p title='"+fw.FollowUpComment+"'>"+ fw.FollowUpComment.Substring(0, 100) +"</p>":fw.FollowUpComment:String.Empty,
                                    fw.FollowUpDate.HasValue ? fw.FollowUpDate.Value.ToString("dd/MM/yyyy HH:mm") : "[N/A]",
                                    lm.Sendername,
                                    lm.Senderemail,
                                    lm.Mob,
                                    !string.IsNullOrEmpty(lm.Subject)?lm.Subject.Length>100?
                                    "<p title='"+lm.Subject+"'>"+ lm.Subject.Substring(0, 100) +"</p>":lm.Subject:String.Empty,
                                    //  lm.LogTime?.ToString(),
                                    "<a onclick=fnSendQuotation("+lm.Id+"); title='Send Quotation' class='btn btn-default btn-icon btn-circle btn-lg'><i class='fa fa-send-o'></i></a>  " +
                                    "<a onclick=fnViewQuotation("+lm.Id+"); title = 'View Quotations' class='btn btn-default btn-icon btn-circle btn-lg'><i class='fa fa-code-fork'></i></a>",
                                    "<a onclick=fnViewLeadDetails("+lm.Id+"); title='View Lead Details' class='btn btn-default btn-icon btn-circle btn-lg'><i class='fa fa-edit'></i></a>"
                             }).ToArrayAsync();

            return Ok(new { data = assignLeads });
        }
    }
}