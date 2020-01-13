using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Crnc.Oms.Security.Domain.Dto
{
    public class UserFilterDto
    {
        private List<string> _roles;

        public List<string> Roles
        {
            get => _roles;
            set
            {
                if (value != null && value.Any())
                {
                    _roles = value.Select(r => r.ToLower()).ToList();
                    return;
                }

                _roles = value;
            }
        }

        public bool IsShortInfo { get; set; } = false;
    }
}