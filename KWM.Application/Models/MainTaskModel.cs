using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWM.Application.Models;
public class MainTaskModel : BaseTaskModel
{
    public string UserId { get; set; }
    public IEnumerable<SubTaskModel> SubTasks { get; set; }
}
