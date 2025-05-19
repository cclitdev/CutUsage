using CutUsage.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace CutUsage.ViewModels
{
    public class DocketDetailsViewModel
    {
        // Skip validation for Header so that missing non-posted properties won't cause errors.
        [ValidateNever]
        public DocketDetail Header { get; set; }

        // List of usage roles.
        public List<UsageRoleViewModel> UsageRoles { get; set; }
    }
}
