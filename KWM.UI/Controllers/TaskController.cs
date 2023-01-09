using KWM.Application.Data;
using KWM.Application.Models;
using KWM.Application.ViewModels.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KWM.UI.Controllers;

[Route("Task")]
[Authorize]
public class TaskController : Controller
{
    private readonly ITaskRepository _taskRepository;

    public TaskController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public IActionResult Index()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        List<TaskViewModel> tasks = new List<TaskViewModel>();

        var list = _taskRepository.GetAllMainTasks(userId);

        foreach (var item in list)
        {
            var task = new TaskViewModel();

            task.Id = item.Id;
            task.Title = item.Title;
            task.Description = item.Description;
            task.IsCompleted = item.IsCompleted;
            task.OrderIndex = item.OrderIndex;

            tasks.Add(task);
        }

        var sorted = tasks.OrderBy(task => task.OrderIndex).ToList();

        return View(sorted);
    }

    [HttpGet("Add")]
    public IActionResult AddNewTask()
    {
        TaskViewModel task = new TaskViewModel();
        return View(task);
    }

    [HttpPost("Add")]
    public IActionResult AddNewTask(TaskViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return View(model);
        }

        string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _taskRepository.AddMainTask(model, UserId);

        return RedirectToAction("Index");
    }

    [HttpPost("CompleteTask")]
    public bool CompleteTask(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = _taskRepository.CompleteTask(id, userId);

        return result;
    }

    [HttpPost("CompleteSubTask")]
    public bool CompleteSubTask(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = _taskRepository.CompleteSubTask(id, userId);

        return result;
    }

    [HttpGet("Delete/{id}")]
    public IActionResult DeleteTask(int id)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (_taskRepository.DeleteTask(id, userId))
        {
            return RedirectToAction("Index");
        }
        else
        {
            return RedirectToAction("Forbbiden", "Account");
        }
    }

    [HttpPost("ReorderTask")]
    public bool Reorder(int taskId, int oldPosition, int newPosition)
    {

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        bool isSuccessed = _taskRepository.ReorderTask(taskId, oldPosition, newPosition, userId);

        return isSuccessed;

    }

    [HttpGet("OpenTask/{id}")]
    public IActionResult OpenTask(int id)
    {

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = _taskRepository.GetMainTask(id, userId);

        TaskViewModel model = new TaskViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            SubTasks = task.SubTasks.Select(x => new SubTaskViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                IsCompleted = x.IsCompleted,
                OrderIndex = x.OrderIndex
            }).ToList()
        };

        model.SubTasks = model.SubTasks.OrderBy(x => x.OrderIndex).ToList();

        return View(model);
    }

    [HttpPost("UpdateTaskTitle")]
    public bool UpdateTaskTitle(int id, string title)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        bool resutl = _taskRepository.UpdateTaskTitle(id, title, userId);

        return resutl;
    }

    [HttpPost("UpdateTaskDesc")]
    public bool UpdateTaskDesc(int id, string desc)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        bool resutl = _taskRepository.UpdateTaskDesc(id, desc, userId);

        return resutl;
    }

    [HttpPost("UpdateSubTask")]
    public bool UpdateSubTask(int id, string title, string desc)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        bool resutl = _taskRepository.UpdateSubTask(id, title, desc, userId);

        return resutl;
    }
}
