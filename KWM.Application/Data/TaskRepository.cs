using KWM.Application.Models;
using KWM.Application.ViewModels.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KWM.Application.Data;
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public void AddMainTask(TaskViewModel model, string UserId)
    {

        MainTaskModel task = new()
        {
            Title = model.Title,
            Description = model.Description,
            UserId = UserId
        };

        if (_context.Task.Where(x => x.UserId == UserId).Count() > 0)
        {
            task.OrderIndex = _context.Task.Where(x => x.UserId == UserId).Select(x => x.OrderIndex).Max() + 1;
        }


        try
        {
            _context.Task.Add(task);
            SaveToDatabase();

            if (model.SubTasks != null)
            {
                for (int i = 0; i < model.SubTasks.Count; i++)
                {
                    SubTaskViewModel subTask = model.SubTasks[i];
                    SubTaskModel s = new()
                    {
                        Title = subTask.Title,
                        Description = subTask.Description,
                        MainTaskModelId = task.Id,
                        OrderIndex = i,
                    };

                    AddSubTask(s);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void UpdateMainTask(MainTaskModel task)
    {
        try
        {
            _context.Task.Update(task);
            SaveToDatabase();
        }
        catch (Exception)
        {
            throw;
        }

    }

    public void AddSubTask(SubTaskModel subTask)
    {
        try
        {
            _context.SubTask.Add(subTask);
            SaveToDatabase();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void RemoveSubTask(SubTaskModel subTask)
    {
        try
        {
            _context.SubTask.Remove(subTask);
            SaveToDatabase();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void UpdateSubTask(SubTaskModel subTask)
    {
        try
        {
            _context.SubTask.Update(subTask);
            SaveToDatabase();
        }
        catch (Exception)
        {
            throw;
        }

    }

    private void SaveToDatabase() => _context.SaveChanges();

    public List<MainTaskModel> GetAllMainTasks(string userId)
    {
        List<MainTaskModel> list = _context.Task.Where(x => x.UserId == userId).ToList();

        foreach (var item in list)
        {

            item.SubTasks = GetSubTasks(item.Id);
        }

        return list;
    }

    public IEnumerable<SubTaskModel> GetSubTasks(int id)
    {
        return _context.SubTask.Where(x => x.MainTaskModelId == id).ToList();
    }

    public MainTaskModel? GetMainTask(int id, string userId)
    {
        var task = _context.Task.Include(x => x.SubTasks).FirstOrDefault(x => x.Id == id && x.UserId == userId);

        if (task == null)
        {
            return null;
        }

        task.SubTasks = GetSubTasks(id);

        return task;
    }

    public bool CompleteTask(int id, string userId)
    {
        var task = _context.Task.Single(x => x.Id == id && x.UserId == userId);

        if (task == null)
        {
            return false;
        }

        try
        {
            if (task.IsCompleted == false)
            {
                task.IsCompleted = true;
            }
            else
            {
                task.IsCompleted = false;
            }
            SaveToDatabase();
        }
        catch (Exception ex)
        {
            // todo - log this event
            return false;
        }
        return true;
    }

    public bool DeleteTask(int id, string userId)
    {
        try
        {
            var task = GetMainTask(id, userId);

            if (task == null)
            {
                return false;
            }

            var userTasks = _context.Task.Where(x => x.OrderIndex > task.OrderIndex && x.UserId == userId).ToList();

            foreach (var t in userTasks)
            {
                t.OrderIndex--;
            }

            _context.SubTask.RemoveRange(task.SubTasks);
            _context.Task.Remove(task);

            //bool check = CheckOrderSequentiality(_context.Task.Select(x => x.OrderIndex).ToList());

            //if (check == false)
            //{
            //    throw new Exception("Checking sequentiality failed.");
            //}

            SaveToDatabase();
        }
        catch (Exception ex)
        {
            // todo -log this exception
            return false;
        }

        return true;
    }

    public bool ReorderTask(int taskId, int oldPosition, int newPosition, string userId)
    {
        if (_context.Task.Where(x => x.Id == taskId && x.UserId == userId) == null)
        {
            return false;
        }

        try
        {
            var userTasks = _context.Task.Where(x => x.UserId == userId).ToList();

            if (oldPosition - newPosition > 1)
            {
                for (int i = oldPosition - 1; i >= newPosition; i--)
                {
                    var t = userTasks.Where(x => x.OrderIndex == i).First();
                    t.OrderIndex++;
                }

                var task = userTasks.Where(x => x.Id == taskId).First();
                task.OrderIndex = newPosition;

            }
            else if (newPosition - oldPosition == -1)
            {
                var t = userTasks.Where(x => x.OrderIndex == newPosition).First();
                t.OrderIndex++;

                var task = userTasks.Where(x => x.Id == taskId).First();
                task.OrderIndex--;
            }
            else if (newPosition - oldPosition == 1)
            {
                var t = userTasks.Where(x => x.OrderIndex == newPosition).First();
                t.OrderIndex--;

                var task = userTasks.Where(x => x.Id == taskId).First();
                task.OrderIndex++;
            }
            else if (oldPosition - newPosition < 1)
            {
                for (int i = oldPosition + 1; i <= newPosition; i++)
                {
                    var t = userTasks.Where(x => x.OrderIndex == i).First();
                    t.OrderIndex--;
                }

                var task = userTasks.Where(x => x.Id == taskId).First();
                task.OrderIndex = newPosition;
            }

            bool check = CheckOrderSequentiality(userTasks.Select(x => x.OrderIndex).ToList());

            if (check == false)
            {
                throw new Exception("Checking sequentiality failed.");
            }

            _context.Task.UpdateRange(userTasks);

            SaveToDatabase();
            return true;
        }
        catch (Exception ex)
        {
            // todo -log this exception
            return false;
        }
    }

    private bool CheckOrderSequentiality(List<int> list)
    {
        list.Sort();

        if (Enumerable.Range(1, list.Count - 1).All(i => list[i] - 1 == list[i - 1]) == false)
        {
            return false;
        }

        return true;
    }

    public bool UpdateTaskTitle(int id, string title, string userId)
    {
        var task = _context.Task.Where(x => x.Id == id && x.UserId == userId).FirstOrDefault();

        if (task == null)
        {
            return false;
        }

        try
        {
            task.Title = title;
            UpdateMainTask(task);
            return true;
        }
        catch (Exception ex)
        {
            // todo - log this event
            return false;
        }

    }

    public bool UpdateTaskDesc(int id, string desc, string userId)
    {
        var task = _context.Task.Where(x => x.Id == id && x.UserId == userId).FirstOrDefault();

        if (task == null)
        {
            return false;
        }

        try
        {
            task.Description = desc;
            UpdateMainTask(task);
            return true;
        }
        catch (Exception ex)
        {
            // todo - log this event
            return false;
        }
    }

    public bool UpdateSubTask(int id, string title, string desc, string userId)
    {
        var subTask = _context.SubTask.Where(x => x.Id == id && _context.Task.Where(t => t.Id == x.MainTaskModelId).First().UserId == userId).FirstOrDefault();

        if (subTask == null)
        {
            return false;
        }

        try
        {
            subTask.Title = title;
            subTask.Description = desc;
            UpdateSubTask(subTask);
            return true;
        }
        catch (Exception ex)
        {
            // todo - log this event
            return false;
        }
    }

    public bool CompleteSubTask(int id, string userId)
    {
        var subTask = _context.SubTask.Where(x => x.Id == id && _context.Task.Where(t => t.Id == x.MainTaskModelId).First().UserId == userId).FirstOrDefault();


        if (subTask == null)
        {
            return false;
        }

        try
        {
            if (subTask.IsCompleted == false)
            {
                subTask.IsCompleted = true;
            }
            else
            {
                subTask.IsCompleted = false;
            }
            SaveToDatabase();
        }
        catch (Exception ex)
        {
            // todo - log this event
            return false;
        }
        return true;
    }
}
