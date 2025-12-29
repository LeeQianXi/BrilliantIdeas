using DIAbstract.Services;
using MultiPanel.Abstractions.DTOs;

namespace MultiPanel.Client.Abstract.ViewModels;

public interface IMainMenuViewModel : IDependencyInjection
{
    AuthDto Auth { get; set; }
}