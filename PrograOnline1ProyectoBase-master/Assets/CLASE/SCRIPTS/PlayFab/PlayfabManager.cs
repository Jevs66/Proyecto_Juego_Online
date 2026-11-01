using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System;

public class PlayfabManager : MonoBehaviour
{
    public void RegisterUser()
    {
        var request = new RegisterPlayFabUserRequest()
        {
            Username = "jevs",
            Password = "contra",
            Email = "jesusemiliovs@gmail.com",
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterUserSuccess, OnPlayfabError);
    }

    public async void RegisterUserInPlayfab()
    {
        try
        {
            var result = await RegisterUserInPlayfabTask();
        }
        catch
        {

        }
    }

    private async Task<RegisterPlayFabUserResult> RegisterUserInPlayfabTask()
    {
        var taskResult = new TaskCompletionSource<RegisterPlayFabUserResult>();

        var request = new RegisterPlayFabUserRequest()
        {
            Username = "jevs",
            Password = "contra",
            Email = "jesusemiliovs@gmail.com",
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, result => taskResult.SetResult(result), error => taskResult.SetException(new Exception(error.ErrorMessage)));

        return await taskResult.Task;
    }

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest()
        {
            Email = "jesusemiliovs@gmail.com",
            Password = "contra",
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnPlayfabError);
    }

    public void OnRegisterUserSuccess(RegisterPlayFabUserResult result)
    {

    }

    public void OnLoginSuccess(LoginResult result)
    {

    }

    public void OnPlayfabError(PlayFabError error)
    {
        Debug.Log(error);
    }

}
