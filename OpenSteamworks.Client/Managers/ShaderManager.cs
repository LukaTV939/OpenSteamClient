using OpenSteamworks.Client.Startup;
using OpenSteamClient.DI;
using OpenSteamworks.Generated;
using OpenSteamworks.Data;
using OpenSteamClient.DI.Lifetime;

namespace OpenSteamworks.Client.Managers;

public class ShaderManager : IClientLifetime
{
    private readonly IClientShader shader;
    private readonly Bootstrapper bootstrapper;

    public bool HadShadersEnabled { get; private set; }

    public ShaderManager(Bootstrapper bootstrapper, IClientShader shader) {
        this.bootstrapper = bootstrapper;
        this.shader = shader;
    }

    public async Task RunShutdown(IProgress<OperationProgress> operation)
    {
        shader.EnableShaderManagement(false);
        shader.EnableShaderBackgroundProcessing(false);
        shader.EnableShaderManagementSystem(false);
        await Task.CompletedTask;
    }

    public async Task RunStartup(IProgress<OperationProgress> progress)
    {
        HadShadersEnabled = shader.BIsShaderManagementEnabled();
        shader.EnableShaderManagement(false);
        shader.EnableShaderBackgroundProcessing(false);
        shader.EnableShaderManagementSystem(false);
        await Task.CompletedTask;

        // Steam no longer persists shader management settings, so the disablement is only for this session (which should still be fine, since this is done before user logon)
    }
}