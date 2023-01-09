using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWM.Application.ViewModels.Task;
public class SubTaskViewModel
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public int OrderIndex { get; set; }
}
