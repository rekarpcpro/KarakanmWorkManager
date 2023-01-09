using KWM.Application.Models;
using KWM.Application.ViewModels.Task;

namespace KWM.Application.Data;
public interface ITaskRepository
{
    void AddMainTask(TaskViewModel task, string UserId);
    void UpdateMainTask(MainTaskModel task);
    void AddSubTask(SubTaskModel subTask);
    void RemoveSubTask(SubTaskModel subTask);
    void UpdateSubTask(SubTaskModel subTask);
    List<MainTaskModel> GetAllMainTasks(string useId);
    IEnumerable<SubTaskModel> GetSubTasks(int id);
    MainTaskModel? GetMainTask(int id, string userId);
    bool CompleteTask(int id, string userId);
    bool DeleteTask(int id, string userId);
    bool ReorderTask(int taskId, int oldPosition, int newPosition, string userId);
    bool UpdateTaskTitle(int id, string title, string userId);
    bool UpdateTaskDesc(int id, string desc, string userId);
    bool UpdateSubTask(int id, string title, string desc, string userId);
    bool CompleteSubTask(int id, string userId);
}